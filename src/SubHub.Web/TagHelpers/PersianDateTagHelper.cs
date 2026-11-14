using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace SubHub.Web.TagHelpers
{
    // این هلپر برای نمایش تاریخ به صورت متن ساده در جدول‌ها عالی است
    [HtmlTargetElement("display-persian-date", Attributes = "date")]
    public class PersianDateDisplayTagHelper : TagHelper
    {
        public DateTime? Date { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (Date == null || Date == DateTime.MinValue)
            {
                output.Content.SetContent("-");
                return;
            }

            var pc = new PersianCalendar();
            var dt = Date.Value.ToLocalTime();
            var persianDate = $"{pc.GetYear(dt)}/{pc.GetMonth(dt):00}/{pc.GetDayOfMonth(dt):00}";

            output.TagName = "span";
            output.Content.SetContent(persianDate);
        }
    }
}
