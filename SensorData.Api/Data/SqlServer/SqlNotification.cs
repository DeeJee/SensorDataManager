using System;
using System.Collections.Generic;

namespace SensorData.Api.Data.SqlServer
{
    public partial class SqlNotification
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string LogLevel { get; set; }
        public string Text { get; set; }
        public DateTime? Created { get; set; }
    }
}
