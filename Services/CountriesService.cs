using System.Collections.Generic;
using System.Data;
using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly List<Country> _countries;

        public CountriesService(bool initialize = true)
        {
            _countries = new List<Country>();
            if (initialize)
            {
                _countries.AddRange(new List<Country> {
                    new Country()
                    {
                        CountryID = Guid.Parse("7CA6E811-9879-4F5D-90DF-051D1200D111"),
                        CountryName = "Mexico"
                    },
                    new Country()
                    {
                        CountryID = Guid.Parse("129DDFFE-D967-45B6-943E-4536384F2731"),
                        CountryName = "USA"
                    },
                    new Country()
                    {
                        CountryID = Guid.Parse("25EE8371-43DD-41A9-A9BC-011D3A6FDB06"),
                        CountryName = "UK"
                    },
                    new Country()
                    {
                        CountryID = Guid.Parse("5CF549CE-FF0C-467D-84BF-FC541EB82CBD"),
                        CountryName = "India"
                    },
                    new Country()
                    {
                        CountryID = Guid.Parse("7F8FCF18-BBD7-416D-BDB1-908BA33FB1E2"),
                        CountryName = "Japan"
                    }}
                );

            }
        }

        public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
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
            if (_countries.Where(temp => temp.CountryName == countryAddRequest.CountryName).Count() > 0)
            {
                throw new ArgumentException("Given country name already exists");
            }

            //Convert object from CountryAddRequest to Country type
            Country country = countryAddRequest.ToCountry();

            //generate CountryID
            country.CountryID = Guid.NewGuid();

            //Add country object into _countries
            _countries.Add(country);

            return country.ToCountryResponse();

        }

        public List<CountryResponse> GetAllCountries()
        {
            return _countries.Select(country => country.ToCountryResponse()).ToList();
        }

        public CountryResponse? GetCountryByCountryID(Guid? countryID)
        {
            if(countryID == null) return null;

            Country? country = _countries.FirstOrDefault(x => x.CountryID == countryID);

            if (country == null) return null;

            return country.ToCountryResponse();
        }
    }
}
