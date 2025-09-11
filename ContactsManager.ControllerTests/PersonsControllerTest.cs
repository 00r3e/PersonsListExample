using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Castle.Core.Logging;
using CRUDExample.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace PersonsListTests
{
    public class PersonsControllerTest
    {
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsSorterService _personsSorterService;

        private readonly ICountriesGetterService _countriesService;
        private readonly ILogger<PersonsController> _logger;

        private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
        private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
        private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;
        private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
        private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;

        private readonly Mock<ICountriesGetterService> _countriesServiceMock;
        private readonly Mock<ILogger<PersonsController>> _loggerMock;

        private readonly Fixture _fixure;

        public PersonsControllerTest()
        {
            _fixure = new Fixture();

            _personsGetterServiceMock = new Mock<IPersonsGetterService>();
            _personsAdderServiceMock = new Mock<IPersonsAdderService>();
            _personsDeleterServiceMock = new Mock<IPersonsDeleterService>();
            _personsUpdaterServiceMock = new Mock<IPersonsUpdaterService>();
            _personsSorterServiceMock = new Mock<IPersonsSorterService>();

            _countriesServiceMock = new Mock<ICountriesGetterService>();
            _loggerMock = new Mock<ILogger<PersonsController>>();

            _personsGetterService = _personsGetterServiceMock.Object;
            _personsAdderService = _personsAdderServiceMock.Object;
            _personsSorterService = _personsSorterServiceMock.Object;
            _personsUpdaterService = _personsUpdaterServiceMock.Object;
            _personsDeleterService = _personsDeleterServiceMock.Object;

            _countriesService = _countriesServiceMock.Object;
            _logger = _loggerMock.Object;
        }

        #region Index
        
        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonList()
        {
            //Arrange
            List<PersonResponse> personsResponseList = _fixure.Create<List<PersonResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsGetterService, _personsAdderService,
                _personsDeleterService, _personsSorterService, _personsUpdaterService, _logger);
            
            _personsGetterServiceMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personsResponseList);

            _personsSorterServiceMock.Setup(temp => temp.GetSortedPersons(It.IsAny<List<PersonResponse>>(),It.IsAny<string>(), It.IsAny<SortOrderOptions>()))
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

        //[Fact]
        //public async Task Create_IfModelErrors_ToReturnCreateView()
        //{
        //    //Arrange
        //    PersonAddRequest personAddRequest = _fixure.Create<PersonAddRequest>();
        //    PersonResponse personResponse = _fixure.Create<PersonResponse>();
        //    List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

        //    PersonsController personsController = new PersonsController(_countriesService, _personsGetterService, _personsAdderService,
        //        _personsDeleterService, _personsSorterService, _personsUpdaterService, _logger);

        //    _countriesServiceMock.Setup(temp => temp.GetAllCountries())
        //        .ReturnsAsync(countries);
        //    _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
        //        .ReturnsAsync(personResponse);

        //    //Act
        //    personsController.ModelState.AddModelError("TestError", "Test Error");

        //    IActionResult actionResult = await personsController.Create(personAddRequest);

        //    //Assert
        //    ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);

        //    viewResult.ViewData.Model.Should().BeAssignableTo<PersonAddRequest>();
        //    viewResult.ViewData.Model.Should().Be(personAddRequest);
        //}

        [Fact]
        public async Task Create_IfNoModelErrors_ToReturnRedirectToIndexView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixure.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixure.Create<PersonResponse>();
            List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsGetterService, _personsAdderService,
                _personsDeleterService, _personsSorterService, _personsUpdaterService, _logger);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
                .ReturnsAsync(personResponse);

            //Act
            IActionResult actionResult = await personsController.Create(personAddRequest);

            //Assert
            RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(actionResult);

            redirectResult.ActionName.Should().Be("Index");
        }

        #endregion

        #region Edit
        //[Fact]
        //public async Task Edit_IfModelErrors_ToReturnEditView()
        //{
        //    //Arrange
        //    PersonAddRequest personAddRequest = _fixure.Create<PersonAddRequest>();
        //    PersonResponse personResponse = _fixure.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();
        //    List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

        //    PersonsController personsController = new PersonsController(_countriesService, _personsGetterService, _personsAdderService,
        //        _personsDeleterService, _personsSorterService, _personsUpdaterService, _logger);

        //    _countriesServiceMock.Setup(temp => temp.GetAllCountries())
        //        .ReturnsAsync(countries);
        //    _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
        //        .ReturnsAsync(personResponse);
        //    _personsUpdaterServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>()))
        //        .ReturnsAsync(personResponse);

        //    //Act
        //    personsController.ModelState.AddModelError("TestError", "Test Error");

        //    PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

        //    IActionResult actionResult = await personsController.Edit(personUpdateRequest);

        //    //Assert
        //    ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);

        //    viewResult.ViewData.Model.Should().BeAssignableTo<PersonUpdateRequest>();
        //    viewResult.ViewData.Model.Should().BeEquivalentTo(personUpdateRequest);
        //}

        [Fact]
        public async Task Edit_IfNoModelErrors_ToReturnRedirectToIndexView()
        {
            //Arrange
            PersonAddRequest personAddRequest = _fixure.Create<PersonAddRequest>();
            PersonResponse personResponse = _fixure.Build<PersonResponse>().With(temp => temp.Gender, "Male").Create();
            List<CountryResponse> countries = _fixure.Create<List<CountryResponse>>();

            PersonsController personsController = new PersonsController(_countriesService, _personsGetterService, _personsAdderService,
                _personsDeleterService, _personsSorterService, _personsUpdaterService, _logger);

            _countriesServiceMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);
            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            _personsUpdaterServiceMock.Setup(temp => temp.UpdatePerson(It.IsAny<PersonUpdateRequest>()))
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

            PersonsController personsController = new PersonsController(_countriesService, _personsGetterService, _personsAdderService,
                _personsDeleterService, _personsSorterService, _personsUpdaterService, _logger);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            _personsDeleterServiceMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>()))
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

            PersonsController personsController = new PersonsController(_countriesService, _personsGetterService, _personsAdderService,
                _personsDeleterService, _personsSorterService, _personsUpdaterService, _logger);

            _personsGetterServiceMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(personResponse);
            _personsDeleterServiceMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>()))
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
