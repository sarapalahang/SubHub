using System;
using System.Collections.Generic;
using System.Text;

namespace MobileLoginOtp.Application.Services
{
    public class GapGptSettings
    {
        public string BaseUrl { get; set; } = "https://api.gapgpt.app/v1";
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-4o-mini";
    }
}
