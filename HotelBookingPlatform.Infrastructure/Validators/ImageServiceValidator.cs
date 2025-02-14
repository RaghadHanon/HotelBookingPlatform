using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Infrastructure.Interfaces.Validators;
using HotelBookingPlatform.Infrastructure.Utilities;
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

        string? fileExtension = Path.GetExtension(imageData.FileName)?.ToLower();
        if (!AllowedExtensions.Contains(fileExtension))
        {
            throw new BadFileException(InfrastructureErrorMessages.InvalidImageTypeError);
        }
    }
}
