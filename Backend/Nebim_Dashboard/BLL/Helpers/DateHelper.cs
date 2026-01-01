namespace BLL.Helpers;

/// <summary>
/// Tarih işlemleri için helper
/// </summary>
public static class DateHelper
{
    /// <summary>
    /// Bugünün başlangıcı (00:00:00 UTC)
    /// </summary>
    public static DateTime TodayStart => DateTime.UtcNow.Date;
    
    /// <summary>
    /// Bugünün sonu (23:59:59 UTC)
    /// </summary>
    public static DateTime TodayEnd => DateTime.UtcNow.Date.AddDays(1).AddTicks(-1);
    
    /// <summary>
    /// Bu haftanın başlangıcı (Pazartesi)
    /// </summary>
    public static DateTime WeekStart
    {
        get
        {
            var today = DateTime.UtcNow.Date;
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return today.AddDays(-diff);
        }
    }
    
    /// <summary>
    /// Bu ayın başlangıcı
    /// </summary>
    public static DateTime MonthStart => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    
    /// <summary>
    /// Yüzde değişim hesapla
    /// </summary>
    public static decimal CalculateChangePercentage(decimal current, decimal previous)
    {
        if (previous == 0)
            return current > 0 ? 100 : 0;
        
        return Math.Round((current - previous) / previous * 100, 2);
    }
}
