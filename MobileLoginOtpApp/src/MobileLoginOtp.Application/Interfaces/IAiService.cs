using System;
using System.Collections.Generic;
using System.Text;

namespace MobileLoginOtp.Application.Interfaces
{
    public interface IAiService
    {
        Task<string> GetHumanResourcesInfoAsync(string userPrompt);
    }

}
