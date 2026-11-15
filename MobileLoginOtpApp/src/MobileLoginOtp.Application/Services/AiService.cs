using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MobileLoginOtp.Application.Interfaces;

namespace MobileLoginOtp.Application.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly GapGptSettings _settings;

        public AiService(HttpClient httpClient, IOptions<GapGptSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<string> GetHumanResourcesInfoAsync(string userPrompt)
        {
            var systemPrompt = @"
شما یک دستیار متخصص منابع انسانی هستید.
پاسخ‌ها را به زبان فارسی، دقیق، کاربردی و دسته‌بندی‌شده ارائه بده.
در صورت امکان این بخش‌ها را پوشش بده:
- جذب و استخدام
- آنبوردینگ
- حضور و غیاب
- حقوق و مزایا
- ارزیابی عملکرد
- آموزش و توسعه
- نگهداشت کارکنان
- سیاست‌ها و آیین‌نامه‌های داخلی
";

            var requestBody = new
            {
                model = _settings.Model,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = systemPrompt
                    },
                    new
                    {
                        role = "user",
                        content = userPrompt
                    }
                },
                temperature = 0.7
            };

            var requestJson = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"GapGPT API Error: {response.StatusCode} - {responseText}");
            }

            using var document = JsonDocument.Parse(responseText);

            if (!document.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
            {
                throw new Exception("پاسخ API معتبر نیست: choices خالی است.");
            }

            var content = choices[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return string.IsNullOrWhiteSpace(content)
                ? "پاسخی از سرویس دریافت نشد."
                : content;
        }
    }
}

