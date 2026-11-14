using System.ComponentModel.DataAnnotations;

namespace SubHub.Domain.Enums
{
    public enum PlanDurationType
    {
        [Display(Name = "ماهانه")]
        Monthly = 0,

        [Display(Name = "سه ماهه")]
        ThreeMonths = 1,

        [Display(Name = "شش ماهه")]
        SixMonths = 2,

        [Display(Name = "سالانه")]
        Annual = 3
    }
}

