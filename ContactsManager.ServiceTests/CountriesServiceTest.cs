using System;
using ServiceContracts.DTO;
using ServiceContracts;
using ContactsManager.Core.Domain.Entities;
using Services;
using AutoFixture;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using Microsoft.Extensions.Logging;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly ICountriesRepository _countriesRepository;

        private readonly ICountriesGetterService _countriesGetterService;
        private readonly ICountriesAdderService _countriesAdderService;

        private readonly IFixture _fixture;

        public CountriesServiceTest()
        {
            _fixture = new Fixture();

            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;

            var loggerGetterMock = new Mock<ILogger<CountriesGetterService>>();
            var loggerAdderMock = new Mock<ILogger<CountriesAdderService>>();


            _countriesGetterService = new CountriesGetterService(_countriesRepository, loggerGetterMock.Object);
            _countriesAdderService = new CountriesAdderService(_countriesRepository, loggerAdderMock.Object);

        }

        #region AddCountry
        //When CountryAddRequest is null, it should throw ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry_ToBeArgumentNullException()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Act
            Func<Task> action = async () =>
            {
                await _countriesAdderService.AddCountry(request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();

        }

        //When the CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_NullCountryName_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest request = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, null as string).Create();

            //Act
            Func<Task> action = async () =>
            {
                await _countriesAdderService.AddCountry(request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();

        }

        //When the CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
        {
            //Arrange
            CountryAddRequest countryRequest1 = _fixture.Build<CountryAddRequest>()
                 .With(temp => temp.CountryName, "name").Create();
            CountryAddRequest countryRequest2 = _fixture.Build<CountryAddRequest>()
              .With(temp => temp.CountryName, "name").Create();

            Country country1 = countryRequest1.ToCountry();
            Country country2 = countryRequest2.ToCountry();

            _countriesRepositoryMock
             .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
             .ReturnsAsync(country1);

            //Return null when GetCountryByCountryName is called
            _countriesRepositoryMock
             .Setup(temp => temp.GetCountryByName(It.IsAny<string>()))
             .ReturnsAsync(null as Country);

            await _countriesAdderService.AddCountry(countryRequest1);

            //Act
            var action = async () =>
            {
                //Return first country when GetCountryByCountryName is called
                _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country1);

                _countriesRepositoryMock.Setup(temp => temp.GetCountryByName(It.IsAny<string>())).ReturnsAsync(country1);

                await _countriesAdderService.AddCountry(countryRequest2);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();

        }


        //When you supply proper country name, it should insert (add) the country to the existing list of countries

        [Fact]
        public async Task AddCountry_ProperCountryDetails_ToBeSuccessful()
        {
            //Arrange
            CountryAddRequest countryRequest = _fixture.Build<CountryAddRequest>().With(temp => temp.CountryName, "name").Create();

            Country country = countryRequest.ToCountry();

            CountryResponse countryResponseExpected = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(temp => temp.AddCountry(It.IsAny<Country>())).ReturnsAsync(country);

            //Act
            CountryResponse countryResponseFromAdd = await _countriesAdderService.AddCountry(countryRequest);
            countryResponseExpected.CountryID = countryResponseFromAdd.CountryID;

            countryResponseFromAdd.CountryID.Should().NotBeEmpty();
            countryResponseExpected.Should().Be(countryResponseFromAdd);

        }

        #endregion


        #region GetAllCountries

        [Fact]
        public async Task GetAllCountries_AddFewCountries_ToBeSuccessful()
        {
            //Arrange
            List<Country> countries = new List<Country>()
            {
                _fixture.Build<Country>().With(temp => temp.Persons, null as List<Person>).Create(),
                _fixture.Build<Country>().With(temp => temp.Persons, null as List<Person>).Create(),
                _fixture.Build < Country >().With(temp => temp.Persons, null as List<Person>).Create()
            };
            //Act
            List<CountryResponse> countryResponses = countries.Select(temp => temp.ToCountryResponse()).ToList();

            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countries);

            List<CountryResponse> responseListFromGet = await _countriesGetterService.GetAllCountries();

            responseListFromGet.Should().BeEquivalentTo(countryResponses);

        }

        
        [Fact]
        //The list of countries should be empty by default (before adding any countries)
        public async Task GetAllCountries_EmptyList_ToBeEmpty()
        {
            //Arrange


            //Act
            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(new List<Country>());
            List<CountryResponse> actualCountryResponseList = await _countriesGetterService.GetAllCountries();

            //Assert
            actualCountryResponseList.Should().BeEmpty();


        }
        #endregion

        #region GetCountryByCountryID

        [Fact]
        public async Task GetCountryByCountryID_CountryNotFound_ToBeNull() 
        {
            //Act
            _countriesRepositoryMock.Setup(temp => temp.GetCountryById(It.IsAny<Guid>())).ReturnsAsync(null as Country);

            var result = await _countriesGetterService.GetCountryByCountryID(new Guid());

            //Assert
            result.Should().BeNull(); 

        }



        [Fact]
        public async Task GetCountryByCountryID_GetCountry_ToBeSuccesful()
        {
            //Arrange
            Country country = _fixture.Build<Country>().With(temp => temp.Persons, null as List<Person>).Create();

            CountryResponse countryResponseExpected = country.ToCountryResponse();

            _countriesRepositoryMock.Setup(temp => temp.GetCountryById(It.IsAny<Guid>())).ReturnsAsync(country);

            CountryResponse? countryResponseFromGet = await _countriesGetterService.GetCountryByCountryID(new Guid());

            //Assert
            countryResponseFromGet.Should().BeEquivalentTo(countryResponseExpected);

        }
        #endregion
    }
}
