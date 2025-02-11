using AutoMapper;
using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;

namespace HotelBookingPlatform.Application.Services;

public class CityService : ICityService
{
    private readonly ICityRepository _cityRepository;
    private readonly IMapper _mapper;
    private readonly IImageHandler _imageHandler;
    private readonly ILogger<CityService> _logger;

    public CityService(
        ICityRepository cityRepository,
        IMapper mapper,
        IImageHandler imageHandler,
        ILogger<CityService> logger)
    {
        _cityRepository = cityRepository;
        _mapper = mapper;
        _imageHandler = imageHandler;
        _logger = logger;
    }

    public async Task<PaginatedResult<CityOutputDto>> GetAllCitiesAsync(GetCitiesQueryParametersDto request)
    {
        var paginatedResult = await _cityRepository.GetAllCitiesAsync(request);
        var mapped = _mapper.Map<IEnumerable<CityOutputDto>>(paginatedResult.Data);

        return new PaginatedResult<CityOutputDto>(mapped, paginatedResult.Metadata);
    }

    public async Task<CityOutputWithHotelsAndImagesDto?> GetCityAsync(Guid id)
    {
        var city = await _cityRepository.GetCityAsync(id)
            ?? throw new NotFoundException(nameof(City), id);

        return _mapper.Map<CityOutputWithHotelsAndImagesDto>(city);
    }

    public async Task<bool> DeleteCityAsync(Guid id)
    {
        var deleted = await _cityRepository.DeleteCityAsync(id);
        if (deleted)
        {
            await _cityRepository.SaveChangesAsync();
        }

        return deleted;
    }

    public async Task<CityOutputDto> CreateCityAsync(CreateCityDto request)
    {
        var city = _mapper.Map<City>(request);
        city.Id = Guid.NewGuid();
        city.CreationDate = DateTime.UtcNow;
        city.LastModified = DateTime.UtcNow;
        var createdCity = await _cityRepository.AddCityAsync(city);
        await _cityRepository.SaveChangesAsync();

        return _mapper.Map<CityOutputDto>(createdCity);
    }

    public async Task<bool> UpdateCityAsync(Guid id, UpdateCityDto request)
    {
        var city = await _cityRepository.GetCityAsync(id)
            ?? throw new NotFoundException(nameof(City), id);
        _mapper.Map(request, city);
        city.LastModified = DateTime.UtcNow;
        await _cityRepository.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<CityAsTrendingDestinationOutputDto>> MostVisitedCitiesAsync(int count = 5)
    {
        _logger.LogInformation(ApplicationLogMessages.MostVisitedCitiesStarted, count);
        _logger.LogDebug(ApplicationLogMessages.ValidatingMostVisitedCitiesCount, count);
        if (count <= 0 || count > 100)
        {
            throw new BadRequestException(ServicesErrorMessages.InvalidNumberOfCities);
        }

        _logger.LogDebug(ApplicationLogMessages.FetchingTheMostVisitedCities);
        var cities = await _cityRepository.GetMostVisitedCitiesAsync(count);
        if (!cities.Any())
        {
            _logger.LogError(ApplicationLogMessages.NoCitiesRetrieved);
        }

        _logger.LogDebug(ApplicationLogMessages.MappingCitiesToOutputModel);
        var mapped = _mapper.Map<IEnumerable<CityAsTrendingDestinationOutputDto>>(cities);
        _logger.LogInformation(ApplicationLogMessages.MostVisitedCitiesCompleted, count);

        return mapped;
    }

    public async Task<bool> UploadImageAsync(Guid cityId, IFormFile file, string basePath, string? alternativeText, bool? thumbnail = false)
    {
        if (string.IsNullOrWhiteSpace(basePath))
        {
            _logger.LogError(ApplicationLogMessages.InvalidBasePathForImageUpload);
            throw new BadFileException(ServicesErrorMessages.ErrorWithUploadingImage);
        }

        var city = await _cityRepository.GetCityAsync(cityId) ?? throw new NotFoundException(nameof(City), cityId);
        var cityDirectory = Path.Combine(basePath, "images", "cities", cityId.ToString());
        var uploadedImageUrl = await _imageHandler.UploadImageAsync(file, cityDirectory, thumbnail.GetValueOrDefault(false));
        var image = new CityImage
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            ImageUrl = uploadedImageUrl,
            AlternativeText = alternativeText,
            CityId = city.Id
        };
        city.LastModified = DateTime.UtcNow;
        await _cityRepository.AddCityImageAsync(city, image);
        await _cityRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CityExistsAsync(Guid id)
    {
        return await _cityRepository.CityExistsAsync(id);
    }
}
