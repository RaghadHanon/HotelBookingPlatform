using AutoMapper;
using HotelBookingPlatform.Application.Mapping;

namespace HotelBookingBlatform.Application.Tests.Common;

public static class AutoMapperSingleton
{
    private static readonly Lazy<Task<IMapper>> _lazyMapper = new(() => InitializeMapperAsync());
    public static Task<IMapper> GetMapperAsync() => _lazyMapper.Value;
    private static async Task<IMapper> InitializeMapperAsync()
    {
        return await Task.Run(() =>
        {
            MapperConfiguration mappingConfig = new MapperConfiguration(config =>
            {
                config.AddProfile(new CityProfile());
                config.AddProfile(new HotelProfile());
                config.AddProfile(new RoomProfile());
                config.AddProfile(new BookingProfile());
                config.AddProfile(new BookingRoomProfile());
                config.AddProfile(new DiscountProfile());
                config.AddProfile(new FeaturedDealProfile());
                config.AddProfile(new ReviewProfile());
                config.AddProfile(new GuestProfile());
            });

            return mappingConfig.CreateMapper();
        });
    }
}
