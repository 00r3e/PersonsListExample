﻿using System.Drawing.Text;
using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace PersonsListExample.Controllers
{
    [Route("[controller]")]
    public class CountriesController : Controller
    {

        private readonly ICountriesService _countriesService;

        public CountriesController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        [Route("[action]")]
        public IActionResult UploadFromExcel()
        {
            return View();
        }


        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                ViewBag.ErrorMessage = "Select an xlsx file";
            }

            if (!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Unsupported file. Select an xlsx file";
                return View();
            }
            int countriesCountInserted = await _countriesService.UploadCountriesFromExcelFile(excelFile);

            ViewBag.Message = $"{countriesCountInserted} Countries Uploaded";

            return View();
        }
    }
}
