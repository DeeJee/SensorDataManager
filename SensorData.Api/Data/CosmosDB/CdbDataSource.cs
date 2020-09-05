namespace SensorData.Api.Data.CosmosDB
{
    public class CdbDataSource
    {
        public string id { get; set; }
        public string EntityType = "DataSource";
        public string DeviceId { get; set; }
        public CdbDataType DataType { get; set; }
        public string Description { get; set; }
    }
}
