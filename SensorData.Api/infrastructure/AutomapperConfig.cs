using AutoMapper;
using MySensorData.Common.Data;
using SensorDataCommon.Models;

namespace SensorDataApi.infrastructure
{
    public static class AutomapperConfig
    {
        static MapperConfiguration config;
        public static void Configure()
        {
            config = new MapperConfiguration(cfg => {
                cfg.CreateMap<DataSource, DatasourceModel>();
                cfg.CreateMap<DatasourceModel, DataSource>().ForMember(x => x.DataType, opt => opt.Ignore());
            }
            );                        
        }

        public static IMapper CreateMapper()
        {
            return config.CreateMapper();
        }
    }
}