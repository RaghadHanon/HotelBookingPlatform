using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Discount;
using HotelBookingPlatform.Application.DTOs.Hotel;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;

public class DiscountService : IDiscountService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IDiscountRepository _discountRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(
        IRoomRepository roomRepository,
        IDiscountRepository discountRepository,
        IMapper mapper,
        ILogger<DiscountService> logger)
    {
        _roomRepository = roomRepository;
        _discountRepository = discountRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DiscountOutputDto> CreateDiscountAsync(Guid roomId, CreateDiscountDto createRequest)
    {
        _logger.LogInformation(ApplicationLogMessages.AddDiscountStarted, roomId, createRequest);
        _logger.LogDebug(ApplicationLogMessages.ValidateDiscountRequest);
        if (createRequest.DiscountedPrice == null && createRequest.Percentage == null)
        {
            throw new BadRequestException(ServicesErrorMessages.ErrorDiscountedPriceOrPercentage);
        }

        _logger.LogDebug(ApplicationLogMessages.FetchRoomFromRepository, roomId);
        Room room = await _roomRepository.GetRoomAsync(roomId)
            ?? throw new NotFoundException(nameof(Room), roomId);
        _logger.LogInformation(ApplicationLogMessages.CreatingDiscountEntity, roomId);
        Discount discount =
            createRequest.DiscountedPrice != null ?
              new Discount(room, room.Price, (decimal)createRequest.DiscountedPrice, createRequest.StartDate, createRequest.EndDate)
            : new Discount(room, (decimal)createRequest.Percentage, createRequest.StartDate, createRequest.EndDate);
        _logger.LogInformation(ApplicationLogMessages.DiscountCreated, discount);
        _logger.LogDebug(ApplicationLogMessages.AddingDiscountToRoom, roomId);
        discount.RoomId = room.Id;
        await _discountRepository.AddDiscountAsync(room, discount);
        _logger.LogDebug(ApplicationLogMessages.SavingDiscountToDatabase);
        await _roomRepository.SaveChangesAsync();
        _logger.LogDebug(ApplicationLogMessages.MappingDiscountToOutputModel);
        DiscountOutputDto mapped = _mapper.Map<DiscountOutputDto>(discount);
        mapped.OriginalPrice = discount.OriginalPrice;
        mapped.DiscountedPrice = discount.DiscountedPrice;
        _logger.LogInformation(ApplicationLogMessages.AddDiscountCompleted, roomId, discount);

        return mapped;
    }

    public async Task<DiscountOutputDto?> GetDiscountAsync(Guid roomId, Guid discountId)
    {
        _logger.LogInformation(ApplicationLogMessages.GetDiscountStarted, roomId, discountId);
        _logger.LogDebug(ApplicationLogMessages.FetchDiscountFromRepository, discountId);
        Discount discount = await _discountRepository.GetDiscountAsync(roomId, discountId)
            ?? throw new NotFoundException(nameof(Discount), discountId);
        _logger.LogDebug(ApplicationLogMessages.MappingDiscountToOutputModel);
        DiscountOutputDto mapped = _mapper.Map<DiscountOutputDto>(discount);
        mapped.OriginalPrice = discount.OriginalPrice;
        mapped.DiscountedPrice = discount.DiscountedPrice;
        _logger.LogInformation(ApplicationLogMessages.GetDiscountCompleted, roomId, discountId);

        return mapped;
    }

    public async Task<PaginatedResult<DiscountForRoomOutputDto>> GetDiscountsForRoomAsync(Guid roomId, GetDiscountsQueryParametersDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.GetDiscountsStarted, roomId);
        _logger.LogDebug(ApplicationLogMessages.FetchDiscountsFromRepository, roomId);
        PaginatedResult<Discount>? paginatedResult = await _discountRepository.GetDiscountsForRoomAsync(roomId, request);
        _logger.LogDebug(ApplicationLogMessages.MappingDiscountToOutputModel);
        IEnumerable<DiscountForRoomOutputDto> mapped = _mapper.Map<IEnumerable<DiscountForRoomOutputDto>>(paginatedResult?.Data);
        _logger.LogInformation(ApplicationLogMessages.GetDiscountsCompleted, roomId);

        return new PaginatedResult<DiscountForRoomOutputDto>(mapped, paginatedResult?.Metadata!);
    }

    public async Task<bool> DeleteDiscountAsync(Guid roomId, Guid discountId)
    {
        _logger.LogInformation(ApplicationLogMessages.DeleteDiscountStarted, roomId, discountId);
        _logger.LogDebug(ApplicationLogMessages.FetchDiscountFromRepository, discountId);
        bool deleted = await _discountRepository.DeleteDiscountAsync(roomId, discountId);
        if (deleted)
        {
            _logger.LogDebug(ApplicationLogMessages.SavingDiscountToDatabase);
            await _discountRepository.SaveChangesAsync();
        }
        else
        {
            _logger.LogWarning(ApplicationLogMessages.DeleteDiscountNotFound, discountId);
        }

        _logger.LogInformation(ApplicationLogMessages.DeleteDiscountCompleted, roomId, discountId);

        return deleted;
    }

    public async Task<IEnumerable<FeaturedDealOutputDto>> GetFeaturedDealsAsync(int deals = 5)
    {
        _logger.LogInformation(ApplicationLogMessages.GetFeaturedDealsStarted, deals);
        _logger.LogDebug(ApplicationLogMessages.ValidateNumberOfDeals);
        if (deals <= 0 || deals > 20)
        {
            throw new BadRequestException(string.Format(ServicesErrorMessages.ErrorInvalidNumberOfDeals, deals));
        }

        _logger.LogDebug(ApplicationLogMessages.FetchFeaturedDealsFromRepository);
        IEnumerable<Room> rooms = await _roomRepository.GetRoomsWithMostRecentHighestDiscounts(deals);
        if (!rooms.Any())
        {
            _logger.LogWarning(ApplicationLogMessages.NoAvailableDeals);
        }

        _logger.LogDebug(ApplicationLogMessages.MappingRoomsToDealsOutputModel);
        IEnumerable<FeaturedDealOutputDto> featuredDeals = _mapper.Map<IEnumerable<FeaturedDealOutputDto>>(rooms);
        _logger.LogInformation(ApplicationLogMessages.GetFeaturedDealsCompleted, deals);

        return featuredDeals;
    }
}
