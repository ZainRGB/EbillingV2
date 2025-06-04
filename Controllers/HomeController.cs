using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EbillingV2.Models;
using Microsoft.EntityFrameworkCore;
using EbillingV2.Data;
using System.Data;

namespace EbillingV2.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index(string hospitalid)
    {
        var results = new List<StockLocationViewModel>();

        // Validate hospitalid
        if (!int.TryParse(hospitalid, out int parsedHospitalId))
        {
            // You can handle it however you like here — return an empty list, show a message, redirect, etc.
            return View(results);
        }

        string query = @"
            SELECT sl.id AS StockLocationId, sl.location AS LocationName, sl.hospitalid,
                   sub.id AS LocationId, sub.sublocation AS SubLocationName
            FROM tblstocklocation sl
            INNER JOIN tblstocksublocation sub ON CAST(sl.id AS VARCHAR) = sub.locationid
            WHERE sl.hospitalid = @hospitalid
            ORDER BY sl.location, sub.sublocation";

        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = query;

            // Add parameter to avoid SQL injection and type mismatch
            var parameter = command.CreateParameter();
            parameter.ParameterName = "hospitalid";
            parameter.Value = parsedHospitalId;
            parameter.DbType = DbType.Int32;
            command.Parameters.Add(parameter);

            await _context.Database.OpenConnectionAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    results.Add(new StockLocationViewModel
                    {
                        StockLocationId = Convert.ToInt32(reader["StockLocationId"]),
                        LocationName = reader["LocationName"]?.ToString(),
                        LocationId = Convert.ToInt32(reader["LocationId"]),
                        SubLocationName = reader["SubLocationName"]?.ToString(),
                        HospitalId = Convert.ToInt32(reader["hospitalid"])
                    });
                }
            }
        }

        return View(results);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
