using System.Collections.Generic;
using System.Data;
using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesUploaderFromExcelService : ICountriesUploaderFromExcelService
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesUploaderFromExcelService> _logger;

        public CountriesUploaderFromExcelService(ICountriesRepository countriesRepository, ILogger<CountriesUploaderFromExcelService> logger)
        {
            _countriesRepository = countriesRepository;
            _logger = logger;
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(UploadCountriesFromExcelFile), nameof(CountriesUploaderFromExcelService));

            MemoryStream memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            int countriesInserted = 0;

            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets["Countries"];

                int rowCount = workSheet.Dimension.Rows;
                

                for (int row = 2; row <= rowCount; row++)
                {
                    string? cellValue = Convert.ToString(workSheet.Cells[row, 1].Value);

                    if (!cellValue.IsNullOrEmpty())
                    {
                        string? countryName = cellValue;

                        if(await _countriesRepository.GetCountryByName(countryName) == null)
                        {
                            Country country = new Country() { CountryName = countryName };
                            
                            await _countriesRepository.AddCountry(country);
                            countriesInserted++;
                        }
                    }
                }
            }
            return countriesInserted;
        }
    }
}
