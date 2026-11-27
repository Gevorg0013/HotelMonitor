using HotelMonitor.Api.Models;
using HotelMonitor.Api.Service.Helpers;
using HotelMonitor.Common.Models;
using HotelMonitor.Core.DataLayer.Entities;
using HotelMonitor.Core.DataLayer.Repository;
using HotelMonitor.Core.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace HotelMonitor.Api.Service;

public class MonitorImageService(
    IHotelMonitorUnitOfWork unitOfWork,
    IMonitorImageNotifier notifier,
    IConfiguration configuration,
    IWebHostEnvironment environment)
    : BaseCoreService(unitOfWork), IMonitorImageService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    public const string UploadRequestPath = "/AppFiles";

    public async Task<(BaseResponse, ICollection<MonitorImageResponse>?)> UploadAsync(ImageUploadRequest model)
    {
        if (model.Images is null || model.Images.Count == 0)
            return (new BaseResponse(HttpStatusCode.UnprocessableEntity, "No files were uploaded."), null);

        if (model.Images.Any(x => x.File is null))
            return (new BaseResponse(HttpStatusCode.UnprocessableEntity, "Each image must contain a file."), null);

        if (model.Images.Any(x => x.SequenceId < 1))
            return (new BaseResponse(HttpStatusCode.UnprocessableEntity, "SeqId must be 1 or greater."), null);

        var sequenceNumbers = model.Images.Select(x => x.SequenceId).ToList();

        var images = new List<MonitorImage>(model.Images.Count);
        var imageData = new List<byte[]>(model.Images.Count);
        var now = DateTime.UtcNow;

        foreach (var item in model.Images)
        {
            var file = item.File;

            if (file.Length == 0 || file.Length > MaxFileSizeBytes)
                return (new BaseResponse(HttpStatusCode.UnprocessableEntity, $"File '{file.FileName}' is empty or larger than 10 MB."), null);

            var data = new byte[file.Length];

            await using (var stream = file.OpenReadStream())
            {
                await stream.ReadExactlyAsync(data);
            }

            var contentType = ImageSignature.Detect(data);

            if (contentType is null)
                return (new BaseResponse(HttpStatusCode.UnsupportedMediaType, $"File '{file.FileName}' is not a JPEG, PNG or WebP image."), null);

            images.Add(new MonitorImage
            {
                SequenceNumber = item.SequenceId,
                FileName = Path.GetFileName(file.FileName),
                ContentType = contentType,
                FileSize = data.Length,
                CreatedAt = now
            });
            imageData.Add(data);
        }

        if (await unitOfWork.MonitorImageRepository.AnyAsync(
            x => sequenceNumbers.Contains(x.SequenceNumber)))
        {
            return (new BaseResponse(HttpStatusCode.Conflict, "A SeqId is already used by an existing image."), null);
        }

        var savedFiles = new List<string>(images.Count);

        try
        {
            for (var i = 0; i < images.Count; i++)
            {
                var image = images[i];
                var (webPath, serverPath) = await UploadAndSaveImage(imageData[i], image.ContentType, image.SequenceNumber);
                savedFiles.Add(serverPath);
                image.WebPath = webPath;
            }

            await unitOfWork.MonitorImageRepository.AddRangeAsync(images);

            await unitOfWork.SaveChangesAsync();
        }
        catch
        {
            foreach (var serverPath in savedFiles)
                File.Delete(serverPath);

            throw;
        }

        var notifications = images.Select(x => new MonitorImageNotification
        {
            Id = x.Id,
            SequenceNumber = x.SequenceNumber,
            FileName = x.FileName,
            ContentType = x.ContentType,
            WebPath = x.WebPath,
            CreatedAt = x.CreatedAt
        }).ToList();

        await notifier.NotifyAsync(notifications);

        var response = images.Select(x => new MonitorImageResponse(
            x.Id,
            x.SequenceNumber,
            x.FileName,
            x.ContentType,
            x.FileSize,
            x.WebPath,
            x.CreatedAt))
            .ToList();

        return (new BaseResponse(HttpStatusCode.Created), response);
    }

    private async Task<(string webPath, string serverPath)> UploadAndSaveImage(byte[] data, string contentType, int sequenceNumber)
    {
        var fileName = sequenceNumber + contentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        var fileDirectory = GetUploadDirectory(configuration, environment);

        Directory.CreateDirectory(fileDirectory);
        var filePath = Path.Combine(fileDirectory, fileName);

        await File.WriteAllBytesAsync(filePath, data);

        return ($"{UploadRequestPath}/{fileName}", filePath);
    }

    public static string GetUploadDirectory(IConfiguration configuration, IWebHostEnvironment environment)
        => Path.Combine(environment.ContentRootPath, configuration.GetSection("UploadPhisicalPath").Value ?? "AppFiles");


    public async Task<(BaseResponse, ICollection<MonitorImageResponse>)> GetAllAsync()
    {
        var entities = await unitOfWork.MonitorImageRepository.GetAllAsync();

        var images = entities.Select(x => new MonitorImageResponse(
            x.Id,
            x.SequenceNumber,
            x.FileName,
            x.ContentType,
            x.FileSize,
            x.WebPath,
            x.CreatedAt
        )).ToList();

        return (new BaseResponse(), images);
    }
}
