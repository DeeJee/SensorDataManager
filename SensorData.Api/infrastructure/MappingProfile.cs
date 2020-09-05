using AutoMapper;
using SensorData.Api.Data;
using SensorData.Api.Data.CosmosDB;
using SensorData.Api.Data.SqlServer;
using SensorData.Api.Models;

namespace SensorData.Api.infrastructure
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            // Add as many of these lines as you need to map your objects
            CreateMap<SqlDataSource, DataSourceModel>();
            CreateMap<SensorDataModel, SqlSensorData>();
            CreateMap<SqlSensorData, SensorDataModel>();
            CreateMap<NotificationModel, SqlNotification>();
            CreateMap<SqlNotification, NotificationModel>();

            CreateMap<SensorDataModel, CdbSensorData>();
            CreateMap<CdbSensorData, SensorDataModel>();
            CreateMap<NewDatasourceModel, CdbDataSource>()
                .ForMember(x => x.EntityType, opt => opt.Ignore())
                .ForMember(x=>x.id, opt=>opt.Ignore());
            CreateMap<CdbDataSource, NewDatasourceModel>();
            CreateMap<DataSourceModel, CdbDataSource>();
            CreateMap<CdbDataSource, DataSourceModel>().ForMember(x=>x.DataTypeId, opt=>opt.MapFrom(mf=>mf.DataType.Id));
            CreateMap<NotificationModel, CdbNotification>();
            CreateMap<CdbNotification, NotificationModel>();

            CreateMap<DataTypeModel, CdbDataType>();
            CreateMap<CdbDataType, DataTypeModel>();

        }
    }
}
