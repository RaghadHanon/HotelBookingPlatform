using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Infrastructure.Interfaces.Validators;
using Microsoft.AspNetCore.Http;

namespace HotelBookingPlatform.Infrastructure.Services.Image;

public class ImageHandler : IImageHandler
{
    private readonly IImageServiceValidator _imageValidationService;

    public ImageHandler(IImageServiceValidator imageValidationService)
    {
        _imageValidationService = imageValidationService;
    }

    public async Task<string> UploadImageAsync(IFormFile imageData, string targetDirectory, bool isThumbnail = false)
    {
        _imageValidationService.ValidateImage(imageData);
        string directoryPath = PrepareDirectory(targetDirectory);
        string fileName = GenerateFileName(isThumbnail, imageData);
        string filePath = Path.Combine(directoryPath, fileName);
        DeleteExistingFile(filePath);
        await SaveImageAsync(imageData, filePath);

        return FormatFilePathForWeb(filePath);
    }

    private string PrepareDirectory(string directory)
    {
        string fullPath = Path.GetFullPath(directory);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }

        return fullPath;
    }

    private string GenerateFileName(bool isThumbnail, IFormFile imageData)
    {
        string? extension = Path.GetExtension(imageData.FileName)?.ToLower();

        return isThumbnail ? $"thumbnail{extension}" :
            $"{Path.GetFileNameWithoutExtension(Path.GetRandomFileName())}{extension}";
    }

    private void DeleteExistingFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private async Task SaveImageAsync(IFormFile imageData, string filePath)
    {
        using FileStream fileStream = new FileStream(filePath, FileMode.Create);
        await imageData.CopyToAsync(fileStream);
    }

    private string FormatFilePathForWeb(string filePath)
    {
        return filePath.Substring(filePath.IndexOf("images")).Replace(@"\", "/");
    }
}
