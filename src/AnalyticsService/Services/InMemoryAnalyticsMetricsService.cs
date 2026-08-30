using AnalyticsService.Services;

public class InMemoryAnalyticsMetricsService : IAnalyticsMetricsService
{
    private readonly ILogger<InMemoryAnalyticsMetricsService> _logger;
    private int _created = 0;
    private int _confirmed = 0;
    private int _shipped = 0;

    public InMemoryAnalyticsMetricsService(ILogger<InMemoryAnalyticsMetricsService> logger) => _logger = logger;

    public void RegisterEvent(string eventType)
    {
        switch (eventType)
        {
            case "ORDER_CREATED": _created++; break;
            case "ORDER_CONFIRMED": _confirmed++; break;
            case "ORDER_SHIPPED": _shipped++; break;
        }
        PrintSummary();
    }

    public void PrintSummary()
    {
        _logger.LogInformation(" -> [📊 Analytics Metrics] Creados: {C} | Confirmados: {Conf} | Enviados: {S}", _created, _confirmed, _shipped);
    }
}