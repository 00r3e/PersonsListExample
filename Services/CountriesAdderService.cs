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
    public class CountriesAdderService : ICountriesAdderService
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesAdderService> _logger;

        public CountriesAdderService(ICountriesRepository countriesRepository, ILogger<CountriesAdderService> logger)
        {
            _countriesRepository = countriesRepository;
            _logger = logger;
        }

        async public Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            _logger.LogInformation("{MethodName} of {ServiceName}", nameof(AddCountry), nameof(CountriesAdderService));

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

    }
}
