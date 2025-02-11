using AutoMapper;
using HotelBookingPlatform.Application.DTOs.Common;
using HotelBookingPlatform.Application.DTOs.Identity;
using HotelBookingPlatform.Application.DTOs.Review;
using HotelBookingPlatform.Application.Exceptions;
using HotelBookingPlatform.Application.Utilities;
using HotelBookingPlatform.Application.Utilities.ErrorMessages;
using HotelBookingPlatform.Application.Interfaces;
using HotelBookingPlatform.Application.Interfaces.Infrastructure.Repositories;
using HotelBookingPlatform.Application.Interfaces.Services;
using HotelBookingPlatform.Domain.Models;
using Microsoft.Extensions.Logging;

namespace HotelBookingPlatform.Application.Services;

public class ReviewService : IReviewService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IGuestRepository _guestRepository;
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IHotelRepository hotelRepository, IGuestRepository guestRepository, IReviewRepository reviewRepository, IMapper mapper, ICurrentUser currentUser, ILogger<ReviewService> logger)
    {
        _hotelRepository = hotelRepository;
        _guestRepository = guestRepository;
        _reviewRepository = reviewRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReviewOutputDto> AddReviewAsync(Guid hotelId, CreateOrUpdateReviewDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.AddReviewStarted, hotelId);
        _logger.LogDebug(ApplicationLogMessages.GetHotel);
        var hotel = await _hotelRepository.GetHotelAsync(hotelId) ?? throw new NotFoundException(nameof(Hotel), hotelId);
        var currentGuestDto = await GetGuestFromCurrentUser();
        _logger.LogDebug(ApplicationLogMessages.ValidateGuestAccess);
        await ValidateGuestCanAddReview(currentGuestDto.Guest, hotel);
        _logger.LogDebug(ApplicationLogMessages.MapRequestToEntity);
        var review = _mapper.Map<Review>(request);
        review.Id = Guid.NewGuid();
        review.CreationDate = DateTime.UtcNow;
        review.LastModified = DateTime.UtcNow;
        review.Hotel = hotel;
        review.HotelId = hotel.Id;
        review.Guest = currentGuestDto.Guest;
        review.GuestId = currentGuestDto.Guest.Id;
        _logger.LogDebug(ApplicationLogMessages.AddReviewToRepository);
        await _reviewRepository.AddReviewAsync(hotel, review);
        await _reviewRepository.SaveChangesAsync();
        _logger.LogDebug(ApplicationLogMessages.MapEntityToOutput);
        var outputModel = _mapper.Map<ReviewOutputDto>(review);
        _logger.LogInformation(ApplicationLogMessages.AddReviewCompleted, hotelId, currentGuestDto.Guest.Id);

        return outputModel;
    }

    private async Task ValidateGuestCanAddReview(Guest guest, Hotel hotel)
    {
        _logger.LogDebug(ApplicationLogMessages.ValidateGuestAccess);
        if (!await _guestRepository.HasGuestBookedHotelAsync(hotel, guest))
        {
            throw new BadRequestException(ServicesErrorMessages.GuestNotBooked);
        }

        if (await _guestRepository.HasGuestReviewedHotelAsync(hotel, guest))
        {
            throw new BadRequestException(ServicesErrorMessages.GuestAlreadyReviewed);
        }
    }

    private async Task<CurrentGuestDto> GetGuestFromCurrentUser()
    {
        _logger.LogDebug(ApplicationLogMessages.GetGuestFromCurrentUser);
        var userId = _currentUser.Id;
        var guest = await _guestRepository.GetGuestByUserIdAsync(userId)
                     ?? throw new UnauthenticatedException();

        return new CurrentGuestDto { Guest = guest, UserId = userId };
    }

    public async Task<bool> UpdateReviewAsync(Guid hotelId, Guid reviewId, CreateOrUpdateReviewDto request)
    {
        _logger.LogInformation(ApplicationLogMessages.UpdateReviewStarted, hotelId, reviewId);
        _logger.LogDebug(ApplicationLogMessages.GetHotel);
        var hotel = await _hotelRepository.GetHotelAsync(hotelId) ?? throw new NotFoundException(nameof(Hotel), hotelId);
        var review = await _reviewRepository.GetReviewAsync(hotel, reviewId) ?? throw new NotFoundException(nameof(Review), reviewId);
        var currentGuestDto = await GetGuestFromCurrentUser();
        if (review.GuestId != currentGuestDto.Guest.Id)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, review.GuestId);
        }

        _mapper.Map(request, review);
        review.LastModified = DateTime.UtcNow;
        await _reviewRepository.SaveChangesAsync();
        _logger.LogInformation(ApplicationLogMessages.UpdateReviewCompleted, hotelId, reviewId);

        return true;
    }

    public async Task<bool> DeleteReviewAsync(Guid hotelId, Guid reviewId)
    {
        _logger.LogInformation(ApplicationLogMessages.DeleteReviewStarted, hotelId, reviewId);
        var hotel = await _hotelRepository.GetHotelAsync(hotelId);
        if (hotel == null) return false;
        var review = await _reviewRepository.GetReviewAsync(hotel, reviewId);
        if (review == null) return false;
        var currentGuestDto = await GetGuestFromCurrentUser();
        if (review.GuestId != currentGuestDto.Guest.Id)
        {
            throw new UnauthorizedException(currentGuestDto.UserId, review.GuestId);
        }

        var deleted = await _reviewRepository.DeleteReviewAsync(hotelId, reviewId);
        if (deleted)
        {
            await _reviewRepository.SaveChangesAsync();
        }

        _logger.LogInformation(ApplicationLogMessages.DeleteReviewCompleted, hotelId, currentGuestDto.Guest.Id);

        return deleted;
    }

    public async Task<ReviewOutputDto> GetReviewAsync(Guid hotelId, Guid reviewId)
    {
        var hotel = await _hotelRepository.GetHotelAsync(hotelId) ?? throw new NotFoundException(nameof(Hotel), hotelId);
        var review = await _reviewRepository.GetReviewAsync(hotel, reviewId) ?? throw new NotFoundException(nameof(Review), reviewId);

        return _mapper.Map<ReviewOutputDto>(review);
    }

    public async Task<PaginatedResult<ReviewOutputDto>> GetHotelReviewsAsync(Guid hotelId, GetHotelReviewsQueryParameters request)
    {
        var hotel = await _hotelRepository.GetHotelAsync(hotelId) ?? throw new NotFoundException(nameof(Hotel), hotelId);
        var paginatedResult = await _reviewRepository.GetHotelReviewsAsync(hotel, request);
        var mapped = _mapper.Map<IEnumerable<ReviewOutputDto>>(paginatedResult.Data);

        return new PaginatedResult<ReviewOutputDto>(mapped, paginatedResult.Metadata);
    }

    public async Task<double> GetHotelAverageRatingAsync(Guid id)
    {
        _logger.LogInformation(ApplicationLogMessages.StartGetHotelAverageRating, id);
        _logger.LogDebug(ApplicationLogMessages.GettingHotelFromRepository);
        var hotel = await _hotelRepository.GetHotelAsync(id) ?? throw new NotFoundException(nameof(Hotel), id);
        _logger.LogDebug(ApplicationLogMessages.GettingAverageRatingFromRepository);
        var rating = await _reviewRepository.GetHotelAverageRatingAsync(hotel);
        _logger.LogDebug(ApplicationLogMessages.RoundingRating);
        double roundedRating = double.Round(rating, 1);
        _logger.LogInformation(ApplicationLogMessages.CompletedGetHotelAverageRating, id);

        return roundedRating;
    }
}