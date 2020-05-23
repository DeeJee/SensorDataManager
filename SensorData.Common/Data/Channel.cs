using System;
using System.Collections.Generic;

namespace MySensorData.Common.Data
{
    public partial class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
    }
}
