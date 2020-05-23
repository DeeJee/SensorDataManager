using System;
using System.Collections.Generic;

namespace MySensorData.Common.Data
{
    public partial class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public bool? Active { get; set; }
    }
}
