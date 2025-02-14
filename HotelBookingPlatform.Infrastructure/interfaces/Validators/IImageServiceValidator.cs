using Microsoft.AspNetCore.Http;

namespace HotelBookingPlatform.Infrastructure.Interfaces.Validators;

public interface IImageServiceValidator
{
    void ValidateImage(IFormFile imageData);
}
