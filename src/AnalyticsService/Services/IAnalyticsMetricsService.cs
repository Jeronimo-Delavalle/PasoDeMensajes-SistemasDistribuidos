namespace AnalyticsService.Services;

public interface IAnalyticsMetricsService
{
    void RegisterEvent(string eventType);
    void PrintSummary();
}