using Microsoft.AspNetCore.Mvc;
using MobileLoginOtp.Application.Dtos.ViewModels;
using MobileLoginOtp.Application.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;

namespace MobileLoginOtp.Controllers
{
    public class HomeController : Controller
    {
        
       
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new Application.Models.ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
