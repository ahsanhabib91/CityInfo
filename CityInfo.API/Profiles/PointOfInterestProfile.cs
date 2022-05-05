using AutoMapper;

namespace CityInfo.API.Profiles;

public class PointOfInterestProfile : Profile
{
    public PointOfInterestProfile()
    {
        CreateMap<Entities.PointOfInterest, Models.PointOfInterestDto>();
        CreateMap<Entities.PointOfInterest, Models.PointOfInterestForUpdateDto>();
        CreateMap<Models.PointOfInterestForCreationDto, Entities.PointOfInterest>();
        CreateMap<Models.PointOfInterestForUpdateDto, Entities.PointOfInterest>();
    }
}
