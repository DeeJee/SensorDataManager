using SensorDataCommon.Models;
using System.Collections.Generic;

namespace MySensorData.Common.Data
{
    public interface IDataSourceRepository
    {
        NewDatasource[] NewDatasources();
        List<DatasourceModel> GetDataSources(string channel);
        DataSource GetDataSource(string id);
        void UploadImage(string id, Microsoft.AspNetCore.Http.IFormFile postedFile);
        List<ChannelViewModel> GetChannels();
        void Add(DataSource dataSource);
        void Add(CosmosDB.DataSource dataSource);

        void Delete(int id);
    }
}
