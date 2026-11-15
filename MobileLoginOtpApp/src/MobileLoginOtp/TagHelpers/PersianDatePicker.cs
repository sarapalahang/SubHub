using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace MobileLoginOtp.TagHelpers
{
    [HtmlTargetElement("persian-date-picker", Attributes = ForAttributeName)]
    public class PersianDatePickerTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";
        private const string ReadonlyAttributeName = "asp-readonly";//فوکوس صورت می گیرد.دیتا ارسال می شد
        private const string DisabledAttributeName = "asp-disabled";//فوکوس صورت ننننمی گیرد.دیتا ارسال ننننمی شد

        [HtmlAttributeName(ReadonlyAttributeName)]
        public bool Readonly { get; set; } = false;
        //
        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }
        //
        [HtmlAttributeName(DisabledAttributeName)]
        public bool Disabled { get; set; } = false;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            output.TagName = "input"; // تبدیل به input
            output.TagMode = TagMode.SelfClosing;
            var persianCalendar = new PersianCalendar();
            var today = $"{new PersianCalendar().GetYear(DateTime.Now)}/" +
                        $"{new PersianCalendar().GetMonth(DateTime.Now):00}/" +
                        $"{new PersianCalendar().GetDayOfMonth(DateTime.Now):00}";
            string persianValue = string.Empty;

            if (For?.Model is DateTime value)
            {
                if (value != DateTime.MinValue)
                {
                    persianValue = $"{persianCalendar.GetYear(value)}/" +
                             $"{persianCalendar.GetMonth(value):00}/" +
                             $"{persianCalendar.GetDayOfMonth(value):00}";

                }

            }
            if (Disabled)
            {
                output.Attributes.SetAttribute("disabled", Disabled);
            }
            if (Readonly)
            {
                output.Attributes.SetAttribute("readonly", Readonly);
            }
            else
            {
                output.Attributes.SetAttribute("onclick", $"PersianDatePicker.Show(this, '{today}');");
            }
            output.Attributes.SetAttribute("type", "text");
            output.Attributes.SetAttribute("name", For.Name);
            output.Attributes.SetAttribute("id", For.Name.Replace(".", "_"));
            output.Attributes.Add("value", persianValue);
            output.Attributes.Add("class", "form-control");

            output.Attributes.SetAttribute("dir", "ltr");

            // شروع تگ والد قبل از تگ اصلی
            output.PreElement.SetHtmlContent("<div class='input-group'><span class='input-group-text'> <i class='bi bi-calendar'></i></span>");


            // بستن تگ والد بعد از تگ اصلی
            output.PostElement.SetHtmlContent("</div>");
        }


    }
}

//    [HtmlTargetElement("persian-date-picker", Attributes = "asp-for")]
//    public class PersianDatePicker : TagHelper
//    {
//        [HtmlAttributeName("asp-for")]
//        public ModelExpression AspFor { get; set; }

//        public string Class { get; set; } = "form-control";

//        public override void Process(TagHelperContext context, TagHelperOutput output)
//        {
//            output.TagName = "input";
//            output.TagMode = TagMode.SelfClosing;

//            output.Attributes.SetAttribute("type", "text");
//            output.Attributes.SetAttribute("name", AspFor.Name);
//            output.Attributes.SetAttribute("id", AspFor.Name);
//            output.Attributes.SetAttribute("class", Class);
//            output.Attributes.SetAttribute("autocomplete", "off");
//            output.Attributes.SetAttribute("data-jdp", "");
//        }
//    }
//}

