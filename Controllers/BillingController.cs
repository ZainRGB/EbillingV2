using EbillingV2.Data;
using EbillingV2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace EbillingV2.Controllers
{
    public class BillingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ConnectionFactory _connectionFactory;
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BillingController(ApplicationDbContext context, ConnectionFactory connectionFactory, IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _context = context;
            _connectionFactory = connectionFactory;
            _contextFactory = contextFactory;
        }

        [Route("Billing/{HospitalId:int}/{LocationId:int}/{SubLocationId:int}/{Location}/{SubLocation}")]
        public IActionResult Index(int HospitalId, int LocationId, int SubLocationId, string Location, string SubLocation)
        {
            var model = new PatientBillingViewModel
            {
                HospitalId = HospitalId,
                LocationId = LocationId,
                SubLocationId = SubLocationId,
                Location = Location,
                SubLocation = SubLocation
            };

            if (Request.Query["show"] == "IT")
            {
                return RedirectToAction("LinkPage");
            }

            return View(model);
        }

        [HttpPost]
        [Route("Billing/{HospitalId:int}/{LocationId:int}/{SubLocationId:int}/{Location}/{SubLocation}")]
        public IActionResult Submit(PatientBillingViewModel model)
        {
            return RedirectToAction("Capture", new
            {
                patRefNo = model.PatRefNo,
                hospitalId = model.HospitalId,
                locationId = model.LocationId,
                subLocationId = model.SubLocationId,
                location = model.Location,
                subLocation = model.SubLocation,
                patFirstName = model.PatFirstname,
                patSurname = model.PatSurname,
                attDoc = model.AttDoc,
                medaidName = model.MedAidName
            });
        }

        [Route("Billing/SearchResults")]
        public async Task<IActionResult> SearchResults(
            char letter,
            int HospitalId,
            int LocationId,
            int SubLocationId,
            string Location,
            string SubLocation,
            string PatRefNo = null)
        {
            string sql;
            NpgsqlParameter[] parameters;

            if (!string.IsNullOrEmpty(PatRefNo))
            {
                sql = @"
                SELECT sb.medaidname,sb.patfirstname,sb.attdoc,sb.sex,sb.id,sb.Active,sb.PatRefNo,sb.PatSurname,sb.HospitalId,
                       sl.id AS LocationId, sub.id AS SubLocationId
                FROM tblsignboards sb 
                INNER JOIN tblstocklocation sl on sb.hospitalid = sl.hospitalid 
                INNER JOIN tblstocksublocation sub ON CAST(sl.id AS VARCHAR(20)) = sub.locationid 
                INNER JOIN tblhospitals hid ON sl.hospitalid = hid.id 
                WHERE sb.active = 'Y' AND sb.hospitalid = @hospitalid AND sb.patrefno = @patrefno
                      AND sl.id = @locationid AND sub.id = @sublocationid 
                ORDER BY sb.PatSurname";

                parameters = new[]
                {
                    new NpgsqlParameter("@hospitalid", HospitalId),
                    new NpgsqlParameter("@patrefno", PatRefNo),
                    new NpgsqlParameter("@locationid", LocationId),
                    new NpgsqlParameter("@sublocationid", SubLocationId)
                };
            }
            else
            {
                sql = @"
                SELECT sb.medaidname,sb.patfirstname,sb.attdoc,sb.sex,sb.id,sb.Active,sb.PatRefNo,sb.PatSurname,sb.HospitalId,
                       sl.id AS LocationId, sub.id AS SubLocationId
                FROM tblsignboards sb 
                INNER JOIN tblstocklocation sl on sb.hospitalid = sl.hospitalid 
                INNER JOIN tblstocksublocation sub ON CAST(sl.id AS VARCHAR(20)) = sub.locationid 
                INNER JOIN tblhospitals hid ON sl.hospitalid = hid.id 
                WHERE sb.active = 'Y' AND sb.hospitalid = @hospitalid AND sb.PatSurname iLIKE @letters
                      AND sl.id = @locationid AND sub.id = @sublocationid 
                ORDER BY sb.PatSurname";

                parameters = new[]
                {
                    new NpgsqlParameter("@hospitalid", HospitalId),
                    new NpgsqlParameter("@letters", $"{letter}%"),
                    new NpgsqlParameter("@locationid", LocationId),
                    new NpgsqlParameter("@sublocationid", SubLocationId)
                };
            }

            //var results = await _context.Patients.FromSqlRaw(sql, parameters).ToListAsync();
            var results = await _context.Database.SqlQueryRaw<Patient>(sql, parameters).ToListAsync();

            var viewModels = results.Select(r => new PatientBillingViewModel
            {
                PatRefNo = r.PatRefNo,
                PatSurname = r.PatSurname,
                PatFirstname = r.PatFirstname,
                AttDoc = r.AttDoc,
                MedAidName = r.MedAidName,
                Sex = r.Sex,
                HospitalId = r.HospitalId,
                LocationId = r.LocationId,
                SubLocationId = r.SubLocationId,
                Location = Location,
                SubLocation = SubLocation
            }).ToList();

            return View(new SearchResultsViewModel
            {
                SearchParameters = new PatientBillingViewModel
                {
                    Letter = letter.ToString(),
                    HospitalId = HospitalId,
                    LocationId = LocationId,
                    SubLocationId = SubLocationId,
                    Location = Location,
                    SubLocation = SubLocation
                },
                Results = viewModels
            });
        }

        public async Task<IActionResult> Capture(string patRefNo, int hospitalId, int locationId, int subLocationId, string location, string subLocation, string patFirstName, string patSurname, string attDoc, string medaidName)
        {
            var model = new PatientBillingViewModel
            {
                PatRefNo = patRefNo,
                PatFirstname = patFirstName,
                PatSurname = patSurname,
                MedAidName = medaidName,
                HospitalId = hospitalId,
                LocationId = locationId,
                SubLocationId = subLocationId,
                Location = location,
                AttDoc = attDoc,
                SubLocation = subLocation
            };

            if (TempData["StockResults"] is string stockResultsJson)
            {
                ViewBag.StockResults = JsonSerializer.Deserialize<List<StockResultViewModel>>(stockResultsJson);
            }
            if (TempData["Scan"] != null)
            {
                ViewBag.Scan = TempData["Scan"].ToString();
            }

            string billingQuery = @"
                SELECT * FROM tblstockpatbilling 
                WHERE patrefno = @patrefno AND hospitalid = @hospitalid 
                      AND status = 'Pending' AND active = 'Y' 
                      AND locationid = @locationid AND sublocationid = @sublocationid 
                ORDER BY Id DESC";

            var billingParams = new[]
            {
                new NpgsqlParameter("@patrefno", patRefNo),
                new NpgsqlParameter("@hospitalid", hospitalId),
                new NpgsqlParameter("@locationid", locationId),
                new NpgsqlParameter("@sublocationid", subLocationId)
            };

            var billingResults = await _context.tblstockpatbilling.FromSqlRaw(billingQuery, billingParams).ToListAsync();

            ViewBag.BillingResults = billingResults;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitBarcode(PatientBillingViewModel model)
        {
            var connStr = _connectionFactory.GetConnectionString(model.HospitalId);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseMySQL(connStr)
                .Options;

            await using var context = new ApplicationDbContext(options);

            if (string.IsNullOrEmpty(model.Barcode))
            {
                TempData["Error"] = "Please enter a barcode.";
                return RedirectToAction("Capture", new
                {
                    model.PatRefNo,
                    hospitalId = model.HospitalId,
                    locationId = model.LocationId,
                    subLocationId = model.SubLocationId,
                    location = model.Location,
                    subLocation = model.SubLocation,
                    patFirstName = model.PatFirstname,
                    patSurname = model.PatSurname,
                    attDoc = model.AttDoc,
                    medaidName = model.MedAidName
                });
            }

            string sql = @"SELECT sp.STOCKCODE, sp.BARCODE AS Barcode, sp.PACKSIZE, s.DESCRIPTION FROM stockprices sp INNER JOIN stock s ON sp.stockcode = s.STOCKCODE WHERE(sp.barcode = @barcode OR s.description = @barcode)";

            var parameters = new[] { new MySqlParameter("@barcode", model.Barcode) };

            var stockResults = await context.Set<StockResultViewModel>().FromSqlRaw(sql, parameters).ToListAsync();

            TempData["StockResults"] = JsonSerializer.Serialize(stockResults);
            TempData["Scan"] = model.Scan;

            if (stockResults.Any())
            {
                var selectedStock = stockResults.First();

                var newEntry = new StockPatientBillingModel
                {
                    patrefno = model.PatRefNo,
                    locationid = model.LocationId,
                    patfirstname = model.PatFirstname,
                    patsurname = model.PatSurname,
                    description = selectedStock.Description,
                    stockcode = selectedStock.StockCode,
                    packsize = selectedStock.PackSize.ToString(),
                    hospitalid = model.HospitalId,
                    sublocationid = model.SubLocationId,
                    stockquantity = 1,
                    tblday = DateTime.Now.Day.ToString(),
                    tblmonth = DateTime.Now.Month.ToString(),
                    tblyear = DateTime.Now.Year.ToString(),
                    tbltime = DateTime.Now.ToString("HH:mm"),
                    type = "I",
                    active = "Y",
                    status = "Pending"
                };

                connStr = _connectionFactory.GetConnectionString(0);
                options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseNpgsql(connStr)
                    .Options;
                await using var context1 = new ApplicationDbContext(options);

                context1.tblstockpatbilling.Add(newEntry);
                await context1.SaveChangesAsync();
            }

            return RedirectToAction("Capture", new
            {
                model.PatRefNo,
                hospitalId = model.HospitalId,
                locationId = model.LocationId,
                subLocationId = model.SubLocationId,
                location = model.Location,
                subLocation = model.SubLocation,
                patFirstName = model.PatFirstname,
                patSurname = model.PatSurname,
                attDoc = model.AttDoc,
                medaidName = model.MedAidName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStockQuantity([FromBody] StockQuantityUpdateModel model)
        {
            if (model == null || model.Id <= 0)
            {
                return BadRequest("Invalid data.");
            }

            var record = await _context.tblstockpatbilling.FindAsync(model.Id);
            if (record == null)
            {
                return NotFound("Record not found.");
            }

            // Update stock quantity only if provided and > 0
            if (model.StockQuantity > 0)
            {
                record.stockquantity = model.StockQuantity;
            }

            // Update active if provided
            if (!string.IsNullOrEmpty(model.Active))
            {
                record.active = model.Active;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                newQuantity = record.stockquantity,
                active = record.active
            });
        }
    }
}