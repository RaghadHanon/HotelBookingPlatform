using Microsoft.AspNetCore.Http;

namespace HotelBookingPlatform.Infrastructure.Interfaces;

public interface IImageServiceValidator
{
    void ValidateImage(IFormFile imageData);
}
