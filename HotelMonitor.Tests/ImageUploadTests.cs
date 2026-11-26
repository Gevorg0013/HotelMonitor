using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;

namespace HotelMonitor.Tests;

public sealed class ImageUploadTests(ApiFactory api) : IClassFixture<ApiFactory>
{
    private const string Upload = "/api/monitor/images/upload";
    private const string Images = "/api/monitor/images";
    private static readonly TimeSpan Quiet = TimeSpan.FromSeconds(1.5);

    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];
    private static readonly byte[] WebP = [.. "RIFF"u8, 0x24, 0x00, 0x00, 0x00, .. "WEBP"u8, .. "VP8 "u8];
    private static readonly byte[] Text = "this is not an image"u8.ToArray();

    // Every test uses its own SequenceNumbers, because all tests share one database.
    private static int _nextSequence = 1000;
    private static int NextSequence() => Interlocked.Increment(ref _nextSequence);

    private static MultipartFormDataContent Form(params (string FileName, byte[] Data, int? SequenceNumber)[] files)
    {
        var form = new MultipartFormDataContent();
        foreach (var (fileName, data, sequenceNumber) in files)
        {
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            form.Add(content, "Files", fileName);
            if (sequenceNumber is { } number)
                form.Add(new StringContent(number.ToString()), "SequenceNumbers");
        }
        return form;
    }

    private Task<HttpResponseMessage> PostAsync(MultipartFormDataContent form) => api.CreateClient().PostAsync(Upload, form);

    private Task<int> CountAsync(params int[] sequenceNumbers) =>
        api.WithDb(db => db.MonitorImages.CountAsync(x => sequenceNumbers.Contains(x.SequenceNumber)));

    /// <summary>Records every NewMonitorImages notification the hub sends to a connected client.</summary>
    private sealed class Listener : IAsyncDisposable
    {
        private readonly HubConnection _hub;
        private readonly TaskCompletionSource _first = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public ConcurrentQueue<JsonElement> Notifications { get; } = new();

        private Listener(HubConnection hub)
        {
            _hub = hub;
            hub.On<JsonElement>("NewMonitorImages", payload =>
            {
                Notifications.Enqueue(payload.Clone());
                _first.TrySetResult();
            });
        }

        public static async Task<Listener> StartAsync(ApiFactory api)
        {
            var hub = await api.ConnectHubAsync();
            return new Listener(hub);
        }

        /// <summary>Waits for the first notification, then a quiet period so any extra one would be caught.</summary>
        public async Task<JsonElement> SingleAsync()
        {
            await _first.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await Task.Delay(Quiet);
            return Assert.Single(Notifications);
        }

        public async Task AssertNoneAsync()
        {
            await Task.Delay(Quiet);
            Assert.Empty(Notifications);
        }

        public ValueTask DisposeAsync() => _hub.DisposeAsync();
    }

    // ---- Successful uploads ----

    [Fact]
    public async Task Single_image_is_saved_and_one_notification_is_sent()
    {
        await using var listener = await Listener.StartAsync(api);
        var seq = NextSequence();

        var response = await PostAsync(Form(("one.jpg", Jpeg, seq)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var saved = Assert.Single((await response.Content.ReadFromJsonAsync<JsonElement>()).EnumerateArray());
        Assert.Equal(seq, saved.GetProperty("sequenceNumber").GetInt32());
        Assert.Equal("image/jpeg", saved.GetProperty("contentType").GetString());

        var record = await api.WithDb(db => db.MonitorImages.AsNoTracking().SingleAsync(x => x.SequenceNumber == seq));
        Assert.Equal((saved.GetProperty("id").GetInt32(), "one.jpg", "image/jpeg", (long)Jpeg.Length),
            (record.Id, record.FileName, record.ContentType, record.FileSize));
        Assert.Equal(Jpeg, record.ImageData);

        var notification = Assert.Single((await listener.SingleAsync()).EnumerateArray());
        Assert.Equal(record.Id, notification.GetProperty("id").GetInt32());
        Assert.Equal(seq, notification.GetProperty("sequenceNumber").GetInt32());
    }

    [Fact]
    public async Task Multiple_images_in_one_request_are_all_saved_and_exactly_one_notification_is_sent()
    {
        await using var listener = await Listener.StartAsync(api);
        int a = NextSequence(), b = NextSequence(), c = NextSequence();

        // The declared Content-Type is octet-stream for all three: the stored type comes from the bytes.
        var response = await PostAsync(Form(("a.jpg", Jpeg, a), ("b.png", Png, b), ("c.webp", WebP, c)));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(3, (await response.Content.ReadFromJsonAsync<JsonElement>()).GetArrayLength());

        var records = await api.WithDb(db => db.MonitorImages.AsNoTracking()
            .Where(x => new[] { a, b, c }.Contains(x.SequenceNumber)).OrderBy(x => x.SequenceNumber).ToListAsync());
        Assert.Equal(
            [(a, "a.jpg", "image/jpeg"), (b, "b.png", "image/png"), (c, "c.webp", "image/webp")],
            records.Select(x => (x.SequenceNumber, x.FileName, x.ContentType)));
        Assert.Equal([Jpeg, Png, WebP], records.Select(x => x.ImageData));
        Assert.Equal(records[0].CreatedAt, records[2].CreatedAt);

        // Exactly one notification, carrying all three images.
        var items = (await listener.SingleAsync()).EnumerateArray().ToList();
        Assert.Equal(records.Select(x => (x.Id, x.SequenceNumber, x.FileName, x.ContentType)),
            items.Select(x => (x.GetProperty("id").GetInt32(), x.GetProperty("sequenceNumber").GetInt32(),
                x.GetProperty("fileName").GetString()!, x.GetProperty("contentType").GetString()!)));

        // Metadata only: no image bytes (and nothing else) goes through SignalR.
        foreach (var item in items)
        {
            Assert.Equal(["contentType", "createdAt", "fileName", "id", "sequenceNumber"],
                item.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal));
            Assert.EndsWith("Z", item.GetProperty("createdAt").GetString());
        }
    }

    // ---- Rejected uploads: nothing saved, no notification ----

    [Fact]
    public async Task Invalid_file_in_a_batch_rejects_the_whole_request()
    {
        await using var listener = await Listener.StartAsync(api);
        int good = NextSequence(), bad = NextSequence();

        var response = await PostAsync(Form(("good.jpg", Jpeg, good), ("notes.jpg", Text, bad)));

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        Assert.Contains("notes.jpg", await response.Content.ReadAsStringAsync());
        Assert.Equal(0, await CountAsync(good, bad));
        await listener.AssertNoneAsync();
    }

    [Fact]
    public async Task Empty_file_is_rejected()
    {
        await using var listener = await Listener.StartAsync(api);
        var seq = NextSequence();

        var response = await PostAsync(Form(("empty.jpg", [], seq)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await CountAsync(seq));
        await listener.AssertNoneAsync();
    }

    [Fact]
    public async Task File_over_10_MB_is_rejected()
    {
        var seq = NextSequence();
        var big = new byte[10 * 1024 * 1024 + 1];
        Jpeg.CopyTo(big, 0);

        var response = await PostAsync(Form(("big.jpg", big, seq)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await CountAsync(seq));
    }

    [Fact]
    public async Task Empty_list_is_rejected()
    {
        await using var listener = await Listener.StartAsync(api);
        var before = await api.WithDb(db => db.MonitorImages.CountAsync());

        var response = await PostAsync(new MultipartFormDataContent { { new StringContent("x"), "unrelated" } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, await api.WithDb(db => db.MonitorImages.CountAsync()));
        await listener.AssertNoneAsync();
    }

    [Fact]
    public async Task Files_and_sequence_numbers_must_match_one_to_one()
    {
        var seq = NextSequence();

        var response = await PostAsync(Form(("a.jpg", Jpeg, seq), ("b.jpg", Jpeg, null)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await CountAsync(seq));
    }

    [Fact]
    public async Task Duplicate_sequence_number_within_a_request_is_rejected()
    {
        var seq = NextSequence();

        var response = await PostAsync(Form(("a.jpg", Jpeg, seq), ("b.png", Png, seq)));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await CountAsync(seq));
    }

    [Fact]
    public async Task Sequence_number_that_is_already_used_rejects_the_whole_request()
    {
        var existing = NextSequence();
        Assert.Equal(HttpStatusCode.Created, (await PostAsync(Form(("first.jpg", Jpeg, existing)))).StatusCode);

        await using var listener = await Listener.StartAsync(api);
        var fresh = NextSequence();
        var response = await PostAsync(Form(("new.png", Png, fresh), ("again.jpg", Jpeg, existing)));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(0, await CountAsync(fresh));
        Assert.Equal(1, await CountAsync(existing));
        await listener.AssertNoneAsync();
    }

    [Fact]
    public async Task Failed_save_rolls_back_everything_and_sends_no_notification()
    {
        await using var listener = await Listener.StartAsync(api);
        int a = NextSequence(), b = NextSequence();

        api.SaveFailure.Enabled = true;
        HttpResponseMessage response;
        try
        {
            response = await PostAsync(Form(("a.jpg", Jpeg, a), ("b.png", Png, b)));
        }
        finally
        {
            api.SaveFailure.Enabled = false;
        }

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal(2, api.SaveFailure.RowsWrittenBeforeFailure); // both rows were inserted...
        Assert.Equal(0, await CountAsync(a, b));                   // ...and the transaction rolled them back
        await listener.AssertNoneAsync();

        // The API keeps working afterwards.
        Assert.Equal(HttpStatusCode.Created, (await PostAsync(Form(("a.jpg", Jpeg, a)))).StatusCode);
    }

    // ---- Listing ----

    [Fact]
    public async Task List_returns_every_image_with_correct_data_ordered_by_sequence_number()
    {
        int high = NextSequence() + 500, low = NextSequence();
        Interlocked.Add(ref _nextSequence, 600);
        Assert.Equal(HttpStatusCode.Created, (await PostAsync(Form(("high.png", Png, high), ("low.jpg", Jpeg, low)))).StatusCode);

        var client = await api.AuthorizedClientAsync();
        var list = (await client.GetFromJsonAsync<JsonElement>(Images)).EnumerateArray().ToList();

        var expected = await api.WithDb(db => db.MonitorImages.AsNoTracking().OrderBy(x => x.SequenceNumber)
            .Select(x => new { x.Id, x.SequenceNumber, x.FileName, x.ContentType, x.FileSize }).ToListAsync());
        Assert.Equal(expected.Select(x => (x.Id, x.SequenceNumber, x.FileName, x.ContentType, x.FileSize)),
            list.Select(x => (x.GetProperty("id").GetInt32(), x.GetProperty("sequenceNumber").GetInt32(),
                x.GetProperty("fileName").GetString()!, x.GetProperty("contentType").GetString()!, x.GetProperty("fileSize").GetInt64())));
        Assert.True(list.FindIndex(x => x.GetProperty("sequenceNumber").GetInt32() == low)
                    < list.FindIndex(x => x.GetProperty("sequenceNumber").GetInt32() == high));

        // The list is metadata only; the bytes come from the content endpoint.
        Assert.All(list, x => Assert.False(x.TryGetProperty("imageData", out _)));
        var lowId = list.Single(x => x.GetProperty("sequenceNumber").GetInt32() == low).GetProperty("id").GetInt32();
        var content = await client.GetAsync($"{Images}/{lowId}/content");
        Assert.Equal("image/jpeg", content.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Jpeg, await content.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task List_requires_a_token()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.CreateClient().GetAsync(Images)).StatusCode);
    }
}
