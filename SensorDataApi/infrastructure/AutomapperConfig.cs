using AutoMapper;
using SensorDataCommon.Data;
using SensorDataCommon.Models;

namespace SensorDataApi.infrastructure
{
    public static class AutomapperConfig
    {
        static MapperConfiguration config;
        public static void Configure()
        {
            config = new MapperConfiguration(cfg => cfg.CreateMap<DataSource, Datasource>());
        }

        public static IMapper CreateMapper()
        {
            return config.CreateMapper();
        }
    }
}