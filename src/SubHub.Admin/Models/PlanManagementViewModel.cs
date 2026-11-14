using System.ComponentModel.DataAnnotations;
using SubHub.Domain.Enums;

namespace SubHub.Admin.Models;

public class PlanManagementViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "عنوان پلن را وارد کنید.")]
    [StringLength(150, ErrorMessage = "عنوان پلن حداکثر ۱۵۰ نویسه است.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "توضیحات حداکثر ۵۰۰ نویسه است.")]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "قیمت باید بیشتر از صفر باشد.")]
    public decimal Price { get; set; }

    [EnumDataType(typeof(PlanDurationType), ErrorMessage = "نوع دوره معتبر نیست.")]
    public PlanDurationType DurationType { get; set; }

    [Range(1, 36500, ErrorMessage = "مدت اعتبار باید بین ۱ تا ۳۶۵۰۰ روز باشد.")]
    public int DurationInDays { get; set; } = 30;

    public bool IsActive { get; set; } = true;

    public string DurationTypeLabel => DurationType switch
    {
        PlanDurationType.Monthly => "ماهانه",
        PlanDurationType.ThreeMonths => "سه‌ماهه",
        PlanDurationType.SixMonths => "شش‌ماهه",
        PlanDurationType.Annual => "سالانه",
        _ => "نامشخص"
    };
}
