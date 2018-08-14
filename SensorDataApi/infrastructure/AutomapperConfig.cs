using AutoMapper;
using SensorDataCommon.Data;

namespace SensorDataApi.infrastructure
{
    public static class AutomapperConfig
    {
        static MapperConfiguration config;
        public static void Configure()
        {
            config = new MapperConfiguration(cfg => cfg.CreateMap<DataSource, Models.Datasource>());
        }

        public static IMapper CreateMapper()
        {
            return config.CreateMapper();
        }
    }
}