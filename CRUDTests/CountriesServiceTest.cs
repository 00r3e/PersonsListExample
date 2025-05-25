using System;
using ServiceContracts.DTO;
using ServiceContracts;
using Entities;
using Services;


namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        public CountriesServiceTest()
        {
            _countriesService = new CountriesService(false);
        }


        #region AddCountry
        //When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public void AddCountry_NullCountry()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                _countriesService.AddCountry(request);
            });
        }

        //When the CountryName is null, it should throw ArgumentException

        [Fact]
        public void AddCountry_NullCountryName()
        {
            //Arrange
            CountryAddRequest request = new CountryAddRequest() { CountryName = null };

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _countriesService.AddCountry(request);
            });
        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public void AddCountry_DuplicateCountryName()
        {
            //Arrange
            CountryAddRequest request1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest request2 = new CountryAddRequest() { CountryName = "USA" };


            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _countriesService.AddCountry(request1);
                _countriesService.AddCountry(request2);
            });
        }


        //When you supply proper country name, it should insert (add) the country to the existing list of countries

        [Fact]
        public void AddCountry_ProperCountryDetails()
        {
            //Arrange
            CountryAddRequest request = new CountryAddRequest() { CountryName = "Japan" };

            //Act
            CountryResponse response = _countriesService.AddCountry(request);

            List<CountryResponse> countriesFromGetAllCountries =_countriesService.GetAllCountries();

            //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains<CountryResponse>(response, countriesFromGetAllCountries);

        }

        #endregion


        #region GetAllCountries

        [Fact]
        public void GetAllCountries_AddFewCountries()
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
                countryResponses.Add(_countriesService.AddCountry(countryRequest));
            }


            List<CountryResponse> response = _countriesService.GetAllCountries();

            //Assert
            foreach (CountryResponse countryResponse in countryResponses)
            {
                Assert.Contains<CountryResponse>(countryResponse, response);
            }
        }

        [Fact]
        public void GetAllCountries_WrongCountriesDetails()
        {
            //Arrange
            CountryAddRequest request1 = new CountryAddRequest() { CountryName = "Japan" };
            CountryAddRequest request2 = new CountryAddRequest() {  };

            //Act
            

            List<CountryResponse> response = _countriesService.GetAllCountries();

            Assert.Throws<ArgumentException>(() =>
            {
                _countriesService.AddCountry(request1);
                _countriesService.AddCountry(request2);
                List<CountryResponse> response = _countriesService.GetAllCountries();
            });
        }

        [Fact]
        //The list of countries should be empty by default (before adding any countries)
        public void GetAllCountries_EmptyList()
        {
            //Arrange

            //Act
            List<CountryResponse> actualCountryResponseList = _countriesService.GetAllCountries();

            //Assert
            Assert.Empty(actualCountryResponseList);

        }
        #endregion

        #region GetCountryByCountryID

        [Fact]
        public void GetCountryByCountryID_CountryNotFound() 
        {

            //Assert

            Assert.Null(
                //Act
                _countriesService.GetCountryByCountryID(new Guid()));
        }



        [Fact]
        public void GetCountryByCountryID_GetCountry()
        {
            //Arrange

            CountryResponse countryResponse = _countriesService.AddCountry(new CountryAddRequest() { CountryName = "USA" });

            //Assert
            Assert.Equal(countryResponse,
            //Act
            _countriesService.GetCountryByCountryID(countryResponse.CountryID));
        }

        #endregion
    }
}
