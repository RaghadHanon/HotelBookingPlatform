using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Persistence;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;
    private readonly IImageHandler _imageHandler;
    private readonly ILogger<HotelService> _logger;
    private readonly IEntityValidatorService _entityValidatorService;

    public HotelService(
        IHotelRepository hotelRepository,
        IMapper mapper,
        IImageHandler imageHandler,
        ILogger<HotelService> logger,
        IEntityValidatorService entityValidatorService)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
        _imageHandler = imageHandler;
        _logger = logger;
        _entityValidatorService = entityValidatorService;
    }
    public async Task<HotelWithFullDataOutputDto?> GetHotelAsync(Guid id)
    {
        _logger.LogInformation(ApplicationLogMessages.GetHotelStarted, id);
        var hotel = await _entityValidatorService.ValidateHotelAsync(id);
        var mapped = _mapper.Map<HotelWithFullDataOutputDto>(hotel);
        _logger.LogInformation(ApplicationLogMessages.GetHotelCompleted, id);

        return mapped;
    }

    public async Task<PaginatedResult<HotelOutputDto>> GetAllHotelsAsync(GetHotelsQueryParametersDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.GetAllHotelsStarted, request);

        var paginatedResult = await _hotelRepository.GetAllHotelsAsync(request);
        var mapped = _mapper.Map<IEnumerable<HotelOutputDto>>(paginatedResult.Data);
        _logger.LogInformation(ApplicationLogMessages.GetAllHotelsCompleted, request);

        return new PaginatedResult<HotelOutputDto>(mapped, paginatedResult.Metadata);
    }

    public async Task<bool> DeleteHotelAsync(Guid id)
    {
        _logger.LogInformation(ApplicationLogMessages.DeleteHotelStarted, id);
        var deleted = await _hotelRepository.DeleteHotelAsync(id);
        if (deleted)
        {
            await _hotelRepository.SaveChangesAsync();
            _logger.LogInformation(ApplicationLogMessages.DeleteHotelCompleted, id);
        }

        return deleted;
    }

    public async Task<HotelOutputDto> CreateHotelAsync(CreateHotelDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.CreateHotelStarted, request);
        var city = await _entityValidatorService.ValidateCityAsync(request.CityId);
        var hotel = _mapper.Map<Hotel>(request);
        hotel.Id = Guid.NewGuid();
        hotel.CreationDate = DateTime.UtcNow;
        hotel.LastModified = DateTime.UtcNow;
        hotel.City = city;
        var createdHotel = await _hotelRepository.AddHotelAsync(hotel);
        await _hotelRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.CreateHotelCompleted, createdHotel.Id);

        return _mapper.Map<HotelOutputDto>(createdHotel);
    }

    public async Task<bool> UpdateHotelAsync(Guid id, UpdateHotelDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.UpdateHotelStarted, id);
        var city = await _entityValidatorService.ValidateCityAsync(request.CityId);
        var hotel = await _entityValidatorService.ValidateHotelAsync(id);
        _mapper.Map(request, hotel);
        hotel.City = city;
        hotel.LastModified = DateTime.UtcNow;
        await _hotelRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.UpdateHotelCompleted, id);

        return true;
    }

    public async Task<bool> UploadImageAsync(Guid hotelId, IFormFile file, string basePath, string? alternativeText, bool? thumbnail = false)
    {
        _logger.LogInformation(ApplicationLogMessages.UploadImageStarted, hotelId);
        if (string.IsNullOrWhiteSpace(basePath))
        {
            throw new BadFileException(ServicesErrorMessages.InvalidFilePath);
        }

        var hotel = await _entityValidatorService.ValidateHotelAsync(hotelId);
        var hotelDirectory = Path.Combine(basePath, "images", "hotels", hotelId.ToString());
        var uploadedImageUrl = await _imageHandler.UploadImageAsync(file, hotelDirectory, thumbnail.GetValueOrDefault(false));
        var image = new HotelImage
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            ImageUrl = uploadedImageUrl,
            AlternativeText = alternativeText,
            HotelId = hotel.Id
        };
        hotel.LastModified = DateTime.UtcNow;
        await _hotelRepository.AddHotelImageAsync(hotel, image);
        await _hotelRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.UploadImageCompleted, hotelId);

        return true;
    }

    public async Task<PaginatedResult<HotelSearchResultOutputDto>> SearchAndFilterHotelsAsync(HotelSearchAndFilterParametersDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.SearchAndFilterHotelsStarted, request);
        var paginatedResult = await _hotelRepository.SearchAndFilterHotelsAsync(request);
        var mapped = _mapper.Map<IEnumerable<HotelSearchResultOutputDto>>(paginatedResult.Data);
        _logger.LogInformation(ApplicationLogMessages.SearchAndFilterHotelsCompleted, request);

        return new PaginatedResult<HotelSearchResultOutputDto>(mapped, paginatedResult.Metadata);
    }

    public async Task<bool> AddAmenitiesToHotelAsync(Guid hotelId, List<Guid> amenityIds)
    {
        _logger.LogInformation(ApplicationLogMessages.AddAmenityFroHotelStarted, hotelId);
        var hotel = await _entityValidatorService.ValidateHotelAsync(hotelId);
        foreach (var amenityId in amenityIds)
        {
            var amenity = await _entityValidatorService.ValidateAmenityAsync(amenityId);
            if (await _hotelRepository.HotelContainsAmenityAsync(hotelId, amenityId))
            {
                throw new BadRequestException(string.Format(ServicesErrorMessages.HotelAlreadyHasAmenity, hotel.Id, amenityId));
            }

            _hotelRepository.AddAmenityToHotel(hotel, amenity);
        }

        hotel.LastModified = DateTime.UtcNow;
        await _hotelRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.AddAmenityFroHotelCompleted, hotelId);

        return true;
    }

    public async Task<PaginatedResult<AmenityOutputDto>> GetAmenitiesForHotelAsync(Guid hotelId, GetAmenitiesQueryParametersDto request)
    {
        var paginatedResult = await _hotelRepository.GetAmenitiesForHotelAsync(hotelId, request);
        var mapped = _mapper.Map<IEnumerable<AmenityOutputDto>>(paginatedResult.Data);

        return new PaginatedResult<AmenityOutputDto>(mapped, paginatedResult.Metadata);
    }
}