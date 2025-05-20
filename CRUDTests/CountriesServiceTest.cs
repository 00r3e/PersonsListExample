using System;
using ServiceContracts.DTO;
using ServiceContracts;
using Entities;
using Services;
using Microsoft.EntityFrameworkCore;


namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        public CountriesServiceTest()
        {
            _countriesService = new CountriesService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options));
        }


        #region AddCountry
        //When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            });
        }

        //When the CountryName is null, it should throw ArgumentException

        [Fact]
        public async Task AddCountry_NullCountryName()
        {
            //Arrange
            CountryAddRequest request = new CountryAddRequest() { CountryName = null };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            });
        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            //Arrange
            CountryAddRequest request1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest request2 = new CountryAddRequest() { CountryName = "USA" };


            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }


        //When you supply proper country name, it should insert (add) the country to the existing list of countries

        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            //Arrange
            CountryAddRequest request = new CountryAddRequest() { CountryName = "Japan" };

            //Act
            CountryResponse response = await _countriesService.AddCountry(request);

            List<CountryResponse> countriesFromGetAllCountries =await _countriesService.GetAllCountries();

            //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains<CountryResponse>(response, countriesFromGetAllCountries);

        }

        #endregion


        #region GetAllCountries

        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            //Arrange
            List<CountryAddRequest> countryRequests = new List<CountryAddRequest>()
            {
                new CountryAddRequest(){CountryName = "USA"},
                new CountryAddRequest(){CountryName = "Japan"},
                new CountryAddRequest(){CountryName = "Mexico"}
            };
            //Act
            List<CountryResponse> countryResponses = new List<CountryResponse>();

            foreach (CountryAddRequest countryRequest in countryRequests)
            {
                countryResponses.Add(await _countriesService.AddCountry(countryRequest));
            }


            List<CountryResponse> response = await _countriesService.GetAllCountries();

            //Assert
            foreach (CountryResponse countryResponse in countryResponses)
            {
                Assert.Contains<CountryResponse>(countryResponse, response);
            }
        }

        [Fact]
        public async Task GetAllCountries_WrongCountriesDetails()
        {
            //Arrange
            CountryAddRequest request1 = new CountryAddRequest() { CountryName = "Japan" };
            CountryAddRequest request2 = new CountryAddRequest() {  };

            //Act
            

            List<CountryResponse> response = await _countriesService.GetAllCountries();

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
                List<CountryResponse> response = await _countriesService.GetAllCountries();
            });
        }

        [Fact]
        //The list of countries should be empty by default (before adding any countries)
        public async Task GetAllCountries_EmptyList()
        {
            //Arrange

            //Act
            List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            //Assert
            Assert.Empty(actualCountryResponseList);

        }
        #endregion

        #region GetCountryByCountryID

        [Fact]
        public async Task GetCountryByCountryID_CountryNotFound() 
        {

            //Assert

            Assert.Null(
                //Act
                await _countriesService.GetCountryByCountryID(new Guid()));
        }



        [Fact]
        public async Task GetCountryByCountryID_GetCountry()
        {
            //Arrange

            CountryResponse countryResponse = await _countriesService.AddCountry(new CountryAddRequest() { CountryName = "USA" });

            //Assert
            Assert.Equal(countryResponse,
            //Act
            await _countriesService.GetCountryByCountryID(countryResponse.CountryID));
        }

        #endregion
    }
}
