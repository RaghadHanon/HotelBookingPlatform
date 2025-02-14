using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Amenity;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;
public class AmenityService : IAmenityService
{
    private readonly IAmenityRepository _amenityRepository;
    private readonly ILogger<AmenityService> _logger;
    private readonly IMapper _mapper;

    public AmenityService(IAmenityRepository amenityRepository, ILogger<AmenityService> logger, IMapper mapper)
    {
        _amenityRepository = amenityRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<AmenityOutputDto> CreateAmenity(CreateAndUpdateAmenityDto request)
    {
        Amenity amenity = _mapper.Map<Amenity>(request);
        amenity.Id = Guid.NewGuid();
        amenity.CreationDate = DateTime.UtcNow;
        amenity.LastModified = DateTime.UtcNow;
        Amenity entity = await _amenityRepository.AddAmenityAsync(amenity);
        await _amenityRepository.SaveChangesAsync();

        return _mapper.Map<AmenityOutputDto>(entity);
    }

    public async Task<bool> UpdateAmenity(Guid id, CreateAndUpdateAmenityDto request)
    {
        Amenity amenity = await _amenityRepository.GetAmenityAsync(id)
            ?? throw new NotFoundException(nameof(Amenity), id);
        _mapper.Map(request, amenity);
        amenity.LastModified = DateTime.UtcNow;
        await _amenityRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAmenityAsync(Guid id)
    {
        bool deleted = await _amenityRepository.DeleteAmenityAsync(id);
        if (deleted)
        {
            await _amenityRepository.SaveChangesAsync();
        }

        return deleted;
    }

    public async Task<AmenityOutputDto> GetAmenity(Guid id)
    {
        Amenity amenity = await _amenityRepository.GetAmenityAsync(id) ??
            throw new NotFoundException(nameof(Amenity), id);

        return _mapper.Map<AmenityOutputDto>(amenity);
    }

    public async Task<PaginatedResult<AmenityOutputDto>> GetAllAmenitiesAsync(GetAmenitiesQueryParametersDto request)
    {
        PaginatedResult<Amenity> paginatedResult = await _amenityRepository.GetAllAmenitiesAsync(request);
        IEnumerable<AmenityOutputDto> mapped = _mapper.Map<IEnumerable<AmenityOutputDto>>(paginatedResult.Data);

        return new PaginatedResult<AmenityOutputDto>(mapped, paginatedResult.Metadata);
    }
}

