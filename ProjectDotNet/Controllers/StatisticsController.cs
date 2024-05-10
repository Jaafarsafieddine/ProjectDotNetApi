using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly StatisticsService _statisticsService;

    public StatisticsController(StatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        var userCount = await _statisticsService.GetUserCountAsync();
        var carCount = await _statisticsService.GetCarCountAsync();
        var categoryCount = await _statisticsService.GetCategoryCountAsync();
        var totalPurchasesInMonth = await _statisticsService.GetTotalPurchasesInMonthAsync(2024, 5);
        var totalPurchasesInHistory = await _statisticsService.GetTotalPurchasesInHistoryAsync();

        return Ok(new 
        {
            UserCount = userCount,
            CarCount = carCount,
            CategoryCount = categoryCount,
            TotalPurchasesInMonth = totalPurchasesInMonth,
            TotalPurchasesInHistory = totalPurchasesInHistory
        });
    }
}