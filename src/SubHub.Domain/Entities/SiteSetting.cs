namespace SubHub.Domain.Entities;

public class SiteSetting
{
    public int Id { get; set; } // مقدار اولیه = 1 را حذف کنید
    public bool EnableSmsNotification { get; set; } = true;
    public bool EnableEmailNotification { get; set; } = true;
    public bool EnablePdfAttachment { get; set; } = true;
}
