using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Infrastructure.Utilities;
using HotelBookingPlatform.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HotelBookingPlatform.Infrastructure.Validators;

public class ImageServiceValidator : IImageServiceValidator
{
    private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg" };

    public void ValidateImage(IFormFile imageData)
    {
        if (imageData == null || imageData.Length <= 0)
        {
            throw new BadFileException(InfrastructureErrorMessages.EmptyImageError);
        }

        var fileExtension = Path.GetExtension(imageData.FileName)?.ToLower();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            throw new BadFileException(InfrastructureErrorMessages.InvalidImageTypeError);
        }
    }
}
