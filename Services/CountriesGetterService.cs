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
    public class CountriesGetterService : ICountriesGetterService
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesGetterService> _logger;

        public CountriesGetterService(ICountriesRepository countriesRepository, ILogger<CountriesGetterService> logger)
        {
            _countriesRepository = countriesRepository;
            _logger = logger;
        }

        async public Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(AddCountry), nameof(CountriesGetterService));

            //Validation: countryAddRequest parameter can't be null
            if (countryAddRequest == null)
            {
                throw new ArgumentNullException(nameof(countryAddRequest));
            }

            //Validation: CountryName can't be null
            if (countryAddRequest.CountryName == null)
            {
                throw new ArgumentException(nameof(countryAddRequest.CountryName));
            }

            //Validation: CountryName can't be duplicate
            if (await _countriesRepository.GetCountryByName(countryAddRequest.CountryName) != null)
            {
                throw new ArgumentException("Given country name already exists");
            }

            //Convert object from CountryAddRequest to Country type
            Country country = countryAddRequest.ToCountry();

            //generate CountryID
            country.CountryID = Guid.NewGuid();

            //Add country object into _countries
            await _countriesRepository.AddCountry(country);

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(GetAllCountries), nameof(CountriesGetterService));


            return (await _countriesRepository.GetAllCountries()).Select(temp => temp.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(GetCountryByCountryID), nameof(CountriesGetterService));

            if (countryID == null) return null;

            Country? country = await _countriesRepository.GetCountryById(countryID.Value);

            if (country == null) return null;

            return country.ToCountryResponse();
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(UploadCountriesFromExcelFile), nameof(CountriesGetterService));

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
