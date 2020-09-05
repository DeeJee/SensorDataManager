using SensorDataCommon.Models;
using System.Collections.Generic;

namespace MySensorData.Common.Data.CosmosDB
{
    public interface ICdbSensorDataRepository
    {
        void blaat();
        int Count(string id);
        void DeleteSensorData(string deviceId);
        IList<SensorData> Get();
        IList<SensorData> Get(string deviceId, string vanDatum, string totDatum);
        IList<SensorData> MostRecent();
        SensorData MostRecent(string id);
        void PostSensorData(DataModel sensorData);
    }
}