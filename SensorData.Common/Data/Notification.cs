using System;
using System.Collections.Generic;

namespace MySensorData.Common.Data
{
    public partial class Notification
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string LogLevel { get; set; }
        public string Text { get; set; }
        public DateTime? Created { get; set; }
    }
}
