using System.Collections.Generic;
using System.Data;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly PersonsDbContext _personsDbContext;

        public CountriesService(PersonsDbContext personsDbContext )
        {
            _personsDbContext = personsDbContext;

        }

        async public Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
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
            if (await _personsDbContext.Countries.CountAsync(temp => temp.CountryName == countryAddRequest.CountryName) > 0)
            {
                throw new ArgumentException("Given country name already exists");
            }

            //Convert object from CountryAddRequest to Country type
            Country country = countryAddRequest.ToCountry();

            //generate CountryID
            country.CountryID = Guid.NewGuid();

            //Add country object into _countries
            _personsDbContext.Countries.Add(country);
            await _personsDbContext.SaveChangesAsync();

            return country.ToCountryResponse();

        }

        public async  Task<List<CountryResponse>> GetAllCountries()
        {
            return await _personsDbContext.Countries.Select(country => country.ToCountryResponse()).ToListAsync();
        }

        public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
        {
            if(countryID == null) return null;

            Country? country = await _personsDbContext.Countries.FirstOrDefaultAsync(x => x.CountryID == countryID);

            if (country == null) return null;

            return country.ToCountryResponse();
        }
    }
}
