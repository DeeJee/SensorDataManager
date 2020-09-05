using SensorData.Api.Models;

namespace SensorData.Api.Data.SqlServer
{
    public partial class SqlDataSource
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public int? ChannelId { get; set; }
        public string Description { get; set; }
        public int DataTypeId { get; set; }
        public byte[] Image { get; set; }

        public virtual DataTypeModel DataType { get; set; }
    }
}
