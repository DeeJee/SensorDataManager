using SensorData.Api.Data.SqlServer;
using SensorData.Api.Models;
using System.Collections.Generic;

namespace SensorData.Api.Data
{
    public interface IDataSourceRepository
    {
        NewDatasourceModel[] GetNewDatasources();
        List<DataSourceModel> GetDataSources(string channel);
        DataSourceModel GetDataSource(string id);
        //void UploadImage(string id, Microsoft.AspNetCore.Http.IFormFile postedFile);
        List<ChannelModel> GetChannels();
        void Add(DataSourceModel dataSource);
        void Add(string deviceId);
        void Delete(string id);
        void blaat();
    }
}
