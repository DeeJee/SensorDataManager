using SensorDataCommon.Models;
using System.Collections.Generic;

namespace MySensorData.Common.Data.CosmosDB
{
    public interface ICdbDataSourceRepository
    {
        void Add(DataSource dataSource);
        void Delete(int id);
        List<ChannelViewModel> GetChannels();
        DataSource GetDataSource(string id);
        List<DataSource> GetDataSources();
        List<DataSource> GetDataSources(string channel);
        List<NewDatasource> NewDatasources();
    }
}