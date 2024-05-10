using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ProjectDotNet.DataServices;

public class StatisticsService
{
    private readonly AppDataContext _context;

    public StatisticsService(AppDataContext context)
    {
        _context = context;
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<int> GetCarCountAsync()
    {
        return await _context.Cars.CountAsync();
    }

    public async Task<int> GetCategoryCountAsync()
    {
        return await _context.Categories.CountAsync();
    }

    public async Task<int> GetTotalPurchasesInHistoryAsync()
    {
        return await _context.Purchases.SumAsync(p => p.Quantity);
    }
    public async Task<decimal> GetTotalPurchasesInMonthAsync(int year, int month)
    {
        string yearString = year.ToString();
        string monthString = month < 10 ? "0" + month.ToString() : month.ToString();

        return await _context.Purchases
            .Where(p => p.PurchaseDate.Substring(0, 4) == yearString && p.PurchaseDate.Substring(5, 2) == monthString)
            .SumAsync(p => p.Quantity * p.Car.CarPrice);
    }
}