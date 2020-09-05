using Newtonsoft.Json;

namespace SensorData.Api.Data.CosmosDB
{
    public class CdbDataType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Properties { get; set; }
    }
}
