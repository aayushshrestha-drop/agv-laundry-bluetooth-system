using AutoMapper;
using AGV.Laundry.Tags;
using AGV.Laundry.BaseStations;
using AGV.Laundry.Configurations;

namespace AGV.Laundry
{
    public class AGVLaundryApplicationAutoMapperProfile : Profile
    {
        public AGVLaundryApplicationAutoMapperProfile()
        {
            /* You can configure your AutoMapper mapping configuration here.
             * Alternatively, you can split your mapping configurations
             * into multiple profile classes for a better organization. */
            CreateMap<Tag, TagDto>();
            CreateMap<CreateUpdateTagDto, Tag>();

            CreateMap<BaseStation, BaseStationDto>();
            CreateMap<CreateUpdateBaseStationDto, BaseStation>();

            CreateMap<Configuration, ConfigurationDto>();
            CreateMap<CreateUpdateConfigurationDto, Configuration>();
        }
    }
}
