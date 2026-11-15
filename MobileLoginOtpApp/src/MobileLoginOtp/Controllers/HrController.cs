using Microsoft.AspNetCore.Mvc;
using MobileLoginOtp.Application.Interfaces;
using MobileLoginOtp.Application.Dtos.ViewModels; // مطمئن شو که ViewModel آنجا باشد
using System.Threading.Tasks;

namespace MobileLoginOtp.Controllers
{
    public class HrController : Controller
    {
        private readonly IAiService _aiService;

        public HrController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet]
        public async Task<IActionResult> Ask()
        {
            var prompt = "برای یک شرکت متوسط، ساختار کامل واحد منابع انسانی را توضیح بده و بخش‌های جذب، آموزش، ارزیابی عملکرد و نگهداشت کارکنان را هم پوشش بده.";

            var result = await _aiService.GetHumanResourcesInfoAsync(prompt);

            return Content(result);
        }
        // نمایش صفحه اول (خالی)
        [HttpGet]
        public IActionResult Index()
        {
            return View(new HrAssistantViewModel());
        }

        // دریافت سوال و گرفتن پاسخ از هوش مصنوعی
        [HttpPost]
        public async Task<IActionResult> Index(HrAssistantViewModel model)
        {
            if (!string.IsNullOrEmpty(model.Question))
            {
                try
                {
                    model.Answer = await _aiService.GetHumanResourcesInfoAsync(model.Question);
                }
                catch (System.Exception ex)
                {
                    model.Answer = "خطا در برقراری ارتباط با هوش مصنوعی: " + ex.Message;
                }
            }
            return View(model);
        }
    }
}

