using AutoMapper;
using HotelBookingPlatform.Application.DTOs.City;
using HotelBookingPlatform.Domain.Models;

namespace HotelBookingPlatform.Application.Mapping;

public class CityProfile : Profile
{
    public CityProfile()
    {
        CreateMap<CreateCityDto, City>();

        CreateMap<City, CityOutputDto>()
            .ForMember(dest => dest.CityImage, opt => opt.MapFrom(src =>
                                src.Images.FirstOrDefault(i => i.ImageUrl.Contains("thumbnail"))
                                                                  ?? src.Images.FirstOrDefault()
                                                                  )
                      )
            .ForMember(dest => dest.NumberOfHotels, opt => opt.MapFrom(src => src.Hotels.Count));

        CreateMap<UpdateCityDto, City>();

        CreateMap<City, CityOutputWithHotelsAndImagesDto>();

        CreateMap<CityImage, CityImageOutputDto>();

        CreateMap<City, CityAsTrendingDestinationOutputDto>()
            .ForMember(dest => dest.CityImage, opt => opt.MapFrom(src =>
                                src.Images.FirstOrDefault(i => i.ImageUrl.Contains("thumbnail"))
                                                                  ?? src.Images.FirstOrDefault()
                                                                  )
                      );
    }
}
