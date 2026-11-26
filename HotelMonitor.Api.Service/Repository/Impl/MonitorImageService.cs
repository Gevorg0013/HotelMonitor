
using HotelMonitor.Api.Models;
using HotelMonitor.Api.Service.Helpers;
using HotelMonitor.Common.Models;
using HotelMonitor.Core.DataLayer.Entities;
using HotelMonitor.Core.DataLayer.Repository;
using HotelMonitor.Core.Service;
using System.Linq.Expressions;
using System.Net;

namespace HotelMonitor.Api.Service;

public sealed class MonitorImageService(
    IHotelMonitorUnitOfWork unitOfWork,
    IMonitorImageNotifier notifier)
    : BaseCoreService(unitOfWork), IMonitorImageService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly Expression<Func<MonitorImage, MonitorImageResponse>> ToResponse =
        x => new MonitorImageResponse(
            x.Id,
            x.SequenceNumber,
            x.FileName,
            x.ContentType,
            x.FileSize,
            x.CreatedAt);

    public async Task<(BaseResponse, ICollection<MonitorImageResponse>?)> UploadAsync(
        ImageUploadRequest model)
    {
        if (model.Files is null || model.Files.Count == 0)
            return (new BaseResponse(HttpStatusCode.BadRequest, "No files were uploaded."), null);

        if (model.SequenceNumbers is null ||
            model.Files.Count != model.SequenceNumbers.Count)
            return (new BaseResponse(HttpStatusCode.BadRequest,
                "Each file needs exactly one SequenceNumber."), null);

        if (model.SequenceNumbers.Any(x => x < 1))
            return (new BaseResponse(HttpStatusCode.BadRequest,
                "SequenceNumber must be 1 or greater."), null);

        if (model.SequenceNumbers.Distinct().Count() != model.SequenceNumbers.Count)
            return (new BaseResponse(HttpStatusCode.BadRequest,
                "SequenceNumber values must be unique within the request."), null);

        var images = new List<MonitorImage>(model.Files.Count);
        var now = DateTime.UtcNow;

        for (var i = 0; i < model.Files.Count; i++)
        {
            var file = model.Files[i];

            if (file.Length == 0 || file.Length > MaxFileSizeBytes)
                return (new BaseResponse(HttpStatusCode.BadRequest,
                    $"File '{file.FileName}' is empty or larger than 10 MB."), null);

            var data = new byte[file.Length];

            await using var stream = file.OpenReadStream();
            await stream.ReadExactlyAsync(data);

            var contentType = ImageSignature.Detect(data);

            if (contentType is null)
                return (new BaseResponse(HttpStatusCode.UnsupportedMediaType, $"File '{file.FileName}' is not a JPEG, PNG or WebP image."), null);

            images.Add(new MonitorImage
            {
                SequenceNumber = model.SequenceNumbers[i],
                FileName = Path.GetFileName(file.FileName),
                ImageData = data,
                ContentType = contentType,
                FileSize = data.Length,
                CreatedAt = now
            });
        }

        var sequenceNumbers = model.SequenceNumbers;

        var sequenceExists = await unitOfWork.MonitorImageRepository.AnyAsync(
            x => sequenceNumbers.Contains(x.SequenceNumber));

        if (sequenceExists)
            return (new BaseResponse(HttpStatusCode.Conflict,
                "A SequenceNumber is already used by an existing image."), null);

        foreach (var image in images)
            await unitOfWork.MonitorImageRepository.AddAsync(image);

        await unitOfWork.SaveChangesAsync();

        var notifications = images.Select(x =>
            new MonitorImageNotification(
                x.Id,
                x.SequenceNumber,
                x.FileName,
                x.ContentType,
                x.CreatedAt))
            .ToList();

        await notifier.NotifyAsync(notifications);

        var response = images.Select(x => new MonitorImageResponse(
            x.Id,
            x.SequenceNumber,
            x.FileName,
            x.ContentType,
            x.FileSize,
            x.CreatedAt))
            .ToList();

        return (new BaseResponse(HttpStatusCode.Created), response);
    }

    public async Task<(BaseResponse, ICollection<MonitorImageResponse>)> GetAllAsync()
    {
        var images = await unitOfWork.MonitorImageRepository.GetAllAsync(
            ToResponse,
            x => x.SequenceNumber);

        return (new BaseResponse(), images);
    }

    public async Task<(BaseResponse, ImageContentResponse?)> GetContentAsync(int id)
    {
        var content = await unitOfWork.MonitorImageRepository.GetFirstOrDefaultAsync(
            x => x.Id == id,
            x => new ImageContentResponse(x.ImageData, x.ContentType));

        return (
            new BaseResponse(content is null
                ? HttpStatusCode.NotFound
                : HttpStatusCode.OK),
            content);
    }
}
