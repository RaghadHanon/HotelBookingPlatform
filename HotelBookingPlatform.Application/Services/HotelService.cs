using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Image;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
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
    private readonly ICityRepository _cityRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IMapper _mapper;
    private readonly IImageHandler _imageHandler;
    private readonly ILogger<HotelService> _logger;
    private readonly IAmenityRepository _amenityRepository;

    public HotelService(
        IHotelRepository hotelRepository,
        ICityRepository cityRepository,
        IGuestRepository guestRepository,
        IAmenityRepository amenityRepository,
        IMapper mapper,
        IImageHandler imageHandler,
        ILogger<HotelService> logger)
    {
        _hotelRepository = hotelRepository;
        _cityRepository = cityRepository;
        _guestRepository = guestRepository;
        _mapper = mapper;
        _imageHandler = imageHandler;
        _logger = logger;
        _amenityRepository = amenityRepository;
    }
    public async Task<HotelWithFullDataOutputDto?> GetHotelAsync(Guid id)
    {
        _logger.LogInformation(ApplicationLogMessages.GetHotelStarted, id);
        Hotel hotel = await _hotelRepository.GetHotelAsync(id)
            ?? throw new NotFoundException(nameof(Hotel), id);
        HotelWithFullDataOutputDto mapped = _mapper.Map<HotelWithFullDataOutputDto>(hotel);
        _logger.LogInformation(ApplicationLogMessages.GetHotelCompleted, id);

        return mapped;
    }

    public async Task<PaginatedResult<HotelOutputDto>> GetAllHotelsAsync(GetHotelsQueryParametersDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.GetAllHotelsStarted, request);

        PaginatedResult<Hotel> paginatedResult = await _hotelRepository.GetAllHotelsAsync(request);
        IEnumerable<HotelOutputDto> mapped = _mapper.Map<IEnumerable<HotelOutputDto>>(paginatedResult.Data);
        _logger.LogInformation(ApplicationLogMessages.GetAllHotelsCompleted, request);

        return new PaginatedResult<HotelOutputDto>(mapped, paginatedResult.Metadata);
    }

    public async Task<bool> DeleteHotelAsync(Guid id)
    {
        _logger.LogInformation(ApplicationLogMessages.DeleteHotelStarted, id);
        bool deleted = await _hotelRepository.DeleteHotelAsync(id);
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
        City city = await _cityRepository.GetCityAsync(request.CityId)
            ?? throw new NotFoundException(nameof(City), request.CityId);
        Hotel hotel = _mapper.Map<Hotel>(request);
        hotel.Id = Guid.NewGuid();
        hotel.CreationDate = DateTime.UtcNow;
        hotel.LastModified = DateTime.UtcNow;
        hotel.City = city;
        Hotel createdHotel = await _hotelRepository.AddHotelAsync(hotel);
        await _hotelRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.CreateHotelCompleted, createdHotel.Id);

        return _mapper.Map<HotelOutputDto>(createdHotel);
    }

    public async Task<bool> UpdateHotelAsync(Guid id, UpdateHotelDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.UpdateHotelStarted, id);
        City city = await _cityRepository.GetCityAsync(request.CityId)
            ?? throw new NotFoundException(nameof(City), id);
        Hotel hotel = await _hotelRepository.GetHotelAsync(id)
            ?? throw new NotFoundException(nameof(Hotel), id);
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

        Hotel hotel = await _hotelRepository.GetHotelAsync(hotelId)
            ?? throw new NotFoundException(nameof(Hotel), hotelId);
        string hotelDirectory = Path.Combine(basePath, "images", "hotels", hotelId.ToString());
        string uploadedImageUrl = await _imageHandler.UploadImageAsync(file, hotelDirectory, thumbnail.GetValueOrDefault(false));
        HotelImage image = new HotelImage
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
        PaginatedResult<Hotel> paginatedResult = await _hotelRepository.SearchAndFilterHotelsAsync(request);
        IEnumerable<HotelSearchResultOutputDto> mapped = _mapper.Map<IEnumerable<HotelSearchResultOutputDto>>(paginatedResult.Data);
        _logger.LogInformation(ApplicationLogMessages.SearchAndFilterHotelsCompleted, request);

        return new PaginatedResult<HotelSearchResultOutputDto>(mapped, paginatedResult.Metadata);
    }

    public async Task<bool> AddAmenityToHotelAsync(Guid hotelId, Guid anenityId)
    {
        _logger.LogInformation(ApplicationLogMessages.AddAmenityFroHotelStarted, hotelId);
        Hotel hotel = await _hotelRepository.GetHotelAsync(hotelId)
            ?? throw new NotFoundException(nameof(Hotel), hotelId);
        Amenity amenity = await _amenityRepository.GetAmenityAsync(anenityId)
            ?? throw new NotFoundException(nameof(Amenity), anenityId);
        hotel.LastModified = DateTime.UtcNow;
        _hotelRepository.AddAmenityToHotelAsync(hotel, amenity);
        await _hotelRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.AddAmenityFroHotelCompleted, hotelId);

        return true;
    }

    public async Task<PaginatedResult<AmenityOutputDto>> GetAmenitiesForHotelAsync(Guid hotelId, GetAmenitiesQueryParametersDto request)
    {
        PaginatedResult<Amenity> paginatedResult = await _hotelRepository.GetAmenitiesForHotelAsync(hotelId, request);
        IEnumerable<AmenityOutputDto> mapped = _mapper.Map<IEnumerable<AmenityOutputDto>>(paginatedResult.Data);

        return new PaginatedResult<AmenityOutputDto>(mapped, paginatedResult.Metadata);
    }
}