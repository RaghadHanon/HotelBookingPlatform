using Microsoft.AspNetCore.Http;

namespace HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;

public interface IImageHandler
{
    Task<string> UploadImageAsync(IFormFile imageData, string targetDirectory, bool isThumbnail = false);
}
