using SensorData.Api.Models;
using System.Collections.Generic;

namespace SensorData.Api.Data
{
    public interface ISensorDataRepository
    {
        IEnumerable<SensorDataModel> Get(string deviceId, string vanDatum, string totDatum);
        IEnumerable<SensorDataModel> Get();
        SensorDataModel MostRecent(string id);
        IEnumerable<SensorDataModel> MostRecent();
        int Count(string id);
        void DeleteSensorData(string deviceId);
        void PostSensorData(SensorDataModel sensorData);
        void blaat();
    }
}
