using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace PersonsListTests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        private readonly Mock<IPersonsService> _personsServiceMock;
        private readonly Mock<ICountriesService> _countriesServiceMock;

        private readonly Fixture _fixure;

        public PersonsControllerTest()
        {
            _fixure = new Fixture();

            _personsServiceMock = new Mock<IPersonsService>();
            _countriesServiceMock = new Mock<ICountriesService>();

            _personsService = _personsServiceMock.Object;
            _countriesService = _countriesServiceMock.Object;
        }

        #region Index
        
        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonList()
        {
            //Arrange
            List<PersonResponse> personsResponseList = _fixure.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsService);
            
            _personsServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personsResponseList);

            _personsServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(),It.IsAny<string>(), It.IsAny<SortOrderOptions>()))
                .ReturnsAsync(personsResponseList);

            //Act
            IActionResult actionResult = await personsController.Index(_fixure.Create<string>(), _fixure.Create<string>());

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);

            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonResponse>>();

            viewResult.ViewData.Model.Should().Be(personsResponseList);
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_IfModelErrors_ToReturnCreateView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixure.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixure.Create<PersonResponse>();
            List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            //Act
            personsController.ModelState.AddModelError("TestError", "Test Error");

            IActionResult actionResult = await personsController.Create(personAddRequest);

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);

            viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
            viewResult.ViewData.Model.Should().Be(personAddRequest);
        }

        [Fact]
        public async Task Create_IfNoModelErrors_ToReturnRedirectToIndexView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixure.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixure.Create<PersonResponse>();
            List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            //Act
            IActionResult actionResult = await personsController.Create(personAddRequest);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(actionResult);

            redirectResult.ActionName.Should().Be("Index");
        }

        #endregion

        #region Edit
        [Fact]
        public async Task Edit_IfModelErrors_ToReturnEditView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixure.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixure.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();
            List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            _personsServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>()))
                .ReturnsAsync(personResponse);

            //Act
            personsController.ModelState.AddModelError("TestError", "Test Error");

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            IActionResult actionResult = await personsController.Edit(personUpdateRequest);

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);

            viewResult.ViewData.Model.Should().BeAssignableTo<PersonUpdateRequest>();
            viewResult.ViewData.Model.Should().BeEquivalentTo(personUpdateRequest);
        }

        [Fact]
        public async Task Edit_IfNoModelErrors_ToReturnRedirectToIndexView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixure.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixure.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();
            List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsService);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            _personsServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>()))
                .ReturnsAsync(personResponse);

            //Act
            IActionResult actionResult = await personsController.Create(personAddRequest);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(actionResult);

            redirectResult.ActionName.Should().Be("Index");
        }

        #endregion

        #region Delete
        [Fact]
        public async Task Delete_IfModelErrors_ToRedirectToIndexView()
        {
            //Arrange
            PersonResponse personResponse = _fixure.Create<PersonResponse>();
            List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsService);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            _personsServiceMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            //Act
            personsController.ModelState.AddModelError("TestError", "Test Error");

            IActionResult actionResult = await personsController.Delete(personResponse);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(actionResult);

            redirectResult.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Delete_IfNoModelErrors_ToRedirectToIndexView()
        {
            //Arrange
            PersonResponse personResponse = _fixure.Create<PersonResponse>();
            List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsService);

            _personsServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            _personsServiceMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            //Act
            IActionResult actionResult = await personsController.Delete(personResponse);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(actionResult);

            redirectResult.ActionName.Should().Be("Index");
        }




        #endregion
    }
}
