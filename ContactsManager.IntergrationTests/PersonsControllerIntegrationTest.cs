using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using CsvHelper;
using Entities;
using Fizzler.Systems.HtmlAgilityPack;
using FluentAssertions;
using FluentAssertions.Web;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;

namespace PersonsListTests
{
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
        private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
        private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;
        private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
        private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;


        private readonly IFixture _fixture;

        public PersonsControllerIntegrationTest(CustomWebApplicationFactory webAppFactory)
        {
            _personsGetterServiceMock = new Mock<IPersonsGetterService>();
            _personsAdderServiceMock = new Mock<IPersonsAdderService>();
            _personsDeleterServiceMock = new Mock<IPersonsDeleterService>();
            _personsUpdaterServiceMock = new Mock<IPersonsUpdaterService>();
            _personsSorterServiceMock = new Mock<IPersonsSorterService>();


            webAppFactory.ConfigureTestServicesAction = services =>
            {
                services.AddSingleton(_personsGetterServiceMock.Object);
                services.AddSingleton(_personsAdderServiceMock.Object);
                services.AddSingleton(_personsDeleterServiceMock.Object);
                services.AddSingleton(_personsUpdaterServiceMock.Object);
                services.AddSingleton(_personsSorterServiceMock.Object);

            };

            _client = webAppFactory.CreateClient();

            _fixture = new Fixture();
        }

        #region Index

        [Fact]
        public async Task Index_ToReturnView()
        {
            //Arange
            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<PersonResponse>());
            _personsSorterServiceMock.Setup(s => s.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>()))
                           .ReturnsAsync(new List<PersonResponse>());
            //Act
            HttpResponseMessage response = await _client.GetAsync("Persons/Index");

            //Assert
            response.Should().BeSuccessful();

            string responseBody = await response.Content.ReadAsStringAsync();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseBody);

            var document = htmlDocument.DocumentNode;

            var table = document.QuerySelector("table.persons");
            table.Should().NotBeNull();
        }

        [Fact]
        public async Task Index_WithSearchParameters_ReturnsFilteredView()
        {
            // Arrange
            string searchBy = "PersonName";
            string searchString = "a";

            List<PersonResponse> personsResponseList = new List<PersonResponse>()
            {
                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.PersonName, "ExampleName").Create(),
            };

            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personsResponseList);
            _personsSorterServiceMock.Setup(s => s.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>()))
                           .ReturnsAsync(personsResponseList);

            // Act
            HttpResponseMessage response = await _client.GetAsync($"Persons/Index?searchBy={searchBy}&searchString={searchString}");

            // Assert
            response.Should().BeSuccessful();

            string responseBody = await response.Content.ReadAsStringAsync();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseBody);

            var document = htmlDocument.DocumentNode;

            var table = document.QuerySelector("table.persons");
            table.Should().NotBeNull();

            var tr = document.QuerySelector("tr.persons-tr");
            tr.Should().NotBeNull();
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ToReturnView()
        {
            //Arange

            //Act
            HttpResponseMessage response = await _client.GetAsync("Persons/Create");

            //Assert
            response.Should().BeSuccessful();

            string responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(responseBody);

            var document = htmlDocument.DocumentNode;

            var table = document.QuerySelector("div.create");
            table.Should().NotBeNull();
        }

        [Fact]
        public async Task Create_WithParameter_ReturnsIndexView()
        {
            // Arrange    

            PersonAddRequest personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Gender, GenderOptions.Other)
                .Create();

            List<PersonResponse> personsResponseList = new List<PersonResponse>()
            {
                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.PersonName, "ExampleName1").Create(),

                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "example2@example.com")
                .With(temp => temp.PersonName, "ExampleName2").Create(),

                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "example3@example.com")
                .With(temp => temp.PersonName, "ExampleName3").Create(),
            };

            _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(It.IsAny<PersonResponse>());
            
            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personsResponseList);

            _personsSorterServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>()))
                .ReturnsAsync(personsResponseList);

            // Act
            var formData = new Dictionary<string, string>{
                    { "PersonName", personAddRequest.PersonName },
                    { "Email", personAddRequest.Email },
                    { "Gender", personAddRequest.Gender.ToString() },
                    { "DateOfBirth", personAddRequest.DateOfBirth.ToString() },
                    { "CountryID", personAddRequest.CountryID.ToString() },
                    { "Address", personAddRequest.Address },
                    { "ReceiveNewsLetters", personAddRequest.ReceiveNewsLetters.ToString() }
            };

            var content = new FormUrlEncodedContent(formData);

            var response = await _client.PostAsync("Persons/Create", content);

            // Assert
            response.Should().BeSuccessful();

            string responseBody = await response.Content.ReadAsStringAsync();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseBody);

            var document = htmlDocument.DocumentNode;

            var table = document.QuerySelector("table.persons");
            table.Should().NotBeNull();
        }
        #endregion

        #region Delete
        [Fact]
        public async Task Delete_ToReturnView()
        {
            // Arrange
            var personsServiceMock = new Mock<IPersonsGetterService>();
            var factory = new CustomWebApplicationFactory();

            factory.ConfigureTestServicesAction = services =>
            {
                services.AddSingleton(personsServiceMock.Object);
            };

            var client = factory.CreateClient();

            var fixture = new Fixture();

            PersonResponse personResponse = fixture.Build<PersonResponse>() // ✅ use local fixture
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.PersonName, "ExampleName").Create();

            personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())) // ✅ use local mock
                .ReturnsAsync(personResponse);

            // Act
            HttpResponseMessage response = await client.GetAsync($"Persons/Delete/{personResponse.PersonID}"); // ✅ use local client

            // Assert
            response.Should().BeSuccessful();

            string responseBody = await response.Content.ReadAsStringAsync();

            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseBody);

            var document = htmlDocument.DocumentNode;

            var div = document.QuerySelector("div.delete");
            div.Should().NotBeNull();
        }

        [Fact]
        public async Task Delete_WithParameter_ReturnsIndexView()
        {
            // Arrange    

            PersonResponse personResponse = _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Gender, "Other")
                .Create();

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);

            _personsDeleterServiceMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            // Act
            var formData = new Dictionary<string, string>{
                    { "PersonName", personResponse.PersonName },
                    { "PersonID", personResponse.PersonID.ToString() },
                    { "Email", personResponse.Email },
                    { "Gender", personResponse.Gender.ToString() },
                    { "DateOfBirth", personResponse.DateOfBirth.ToString() },
                    { "CountryID", personResponse.CountryID.ToString() },
                    { "Address", personResponse.Address },
                    { "ReceiveNewsLetters", personResponse.ReceiveNewsLetters.ToString() }
            };

            List<PersonResponse> personsResponseList = new List<PersonResponse>()
            {
                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.PersonName, "ExampleName1").Create(),

                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "example2@example.com")
                .With(temp => temp.PersonName, "ExampleName2").Create(),

                _fixture.Build<PersonResponse>()
                .With(temp => temp.Email, "example3@example.com")
                .With(temp => temp.PersonName, "ExampleName3").Create(),
            };

            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personsResponseList);

            _personsSorterServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOptions>()))
                .ReturnsAsync(personsResponseList);

            var content = new FormUrlEncodedContent(formData);

            var response = await _client.PostAsync($"Persons/Delete/{personResponse.PersonID}", content);

            // Assert
            response.Should().BeSuccessful();

            string responseBody = await response.Content.ReadAsStringAsync();
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(responseBody);

            var document = htmlDocument.DocumentNode;

            var table = document.QuerySelector("table.persons");
            table.Should().NotBeNull();
        }
        #endregion
    }

}
