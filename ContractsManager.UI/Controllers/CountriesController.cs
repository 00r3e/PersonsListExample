using System.Drawing.Text;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace PersonsListExample.Controllers
{
    [Route("[controller]")]
    public class CountriesController : Controller
    {

        private readonly ICountriesUploaderFromExcelService _countriesUploadFromExcelService;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(ICountriesUploaderFromExcelService countriesUploadFromExcelService, ILogger<CountriesController> logger)
        {
            _countriesUploadFromExcelService = countriesUploadFromExcelService;
            _logger = logger;
        }

        [Route("[action]")]
        public IActionResult UploadFromExcel()
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(UploadFromExcel), nameof(CountriesController));

            return View();
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}",  nameof(UploadFromExcel), nameof(CountriesController));

            if (excelFile == null || excelFile.Length == 0)
            {
                ViewBag.ErrorMessage = "Select an xlsx file";
            }

            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Unsupported file. Select an xlsx file";
                return View();
            }
            int countriesCountInserted = await _countriesUploadFromExcelService.UploadCountriesFromExcelFile(excelFile);

            ViewBag.Message = $"{countriesCountInserted} Countries Uploaded";

            return View();
        }
    }
}
