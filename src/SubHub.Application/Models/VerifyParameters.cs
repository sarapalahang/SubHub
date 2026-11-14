using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubHub.Application.Models
{
    public class VerifyParameters
    {
        public string amount { set; get; } = string.Empty;
        public string merchant_id { set; get; } = string.Empty;
        public string authority { set; get; } = string.Empty;
    }
}
