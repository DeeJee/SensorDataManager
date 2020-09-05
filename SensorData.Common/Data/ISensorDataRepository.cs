using SensorDataCommon.Models;
using System.Collections.Generic;
using System.Linq;

namespace MySensorData.Common.Data
{
    public interface ISensorDataRepository
    {
        IList<SensorData> Get(string deviceId, string vanDatum, string totDatum);
        IList<SensorData> Get();
        SensorData MostRecent(string id);
        IList<SensorData> MostRecent();
        int Count(string id);
        void DeleteSensorData(string deviceId);
        void PostSensorData(DataModel sensorData);
        void blaat();
    }
}
