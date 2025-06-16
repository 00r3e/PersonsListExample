using System;

using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using Entities;
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCoreMock;
using AutoFixture;
using FluentAssertions;
using AutoFixture.Kernel;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using RepositoryContracts;
using Moq;
using FluentAssertions.Execution;
using System.Linq.Expressions;

namespace CRUDTests
{
    public class PersonServiceTest
    {

        private readonly IPersonsService _personsService;


        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly IPersonsRepository _personsRepository;

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();

            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;

            _personsService = new PersonsService(_personsRepository);

            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson


        //When we supply null value as PersonAddRequest,it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(new Person());

            //Act
            Func<Task> action = async () =>
            {
                await _personsService.AddPerson(personAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();


            //await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            ////Act
            //    await _personsService.AddPerson(personAddRequest));
        }

        //When we supply null value as PersonName,it should throw ArgumentException

        [Fact]
        public async Task AddPerson_NullPersonName_ToBeArgumentException()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "example@example.com").With(temp => temp.PersonName, null as string).Create();

            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(new Person());

            //Act
            Func<Task> action = async () =>
            {
                await _personsService.AddPerson(personAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();

            ////Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () =>
            ////Act
            //    await _personsService.AddPerson(personAddRequest));
        }

        //When we supply proper person details, it should insert the person in to the persons list
        //and it should return an object of PersonRespons, witch includes the newly generated person id

        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "example@example.com").Create();

            Person person = personAddRequest.ToPerson();

            PersonResponse personResponseExpected = person.ToPersonResponse();

            //If we supply any argument value to the AddPerson method, it should return the same return value
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            //Act
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);
            personResponseExpected.PersonID = personResponseFromAdd.PersonID;

            //Assert
            personResponseFromAdd.PersonID.Should().NotBe(Guid.Empty);
            personResponseFromAdd.Should().Be(personResponseExpected);

            ////Assert
            //Assert.True(personResponse.PersonID != Guid.Empty);

            //Assert.Contains(personResponse, personResponseList);
        }


        #endregion

        #region GetPersonByPersonID

        //If suplly null value as argument it should return null
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            //Arrange
            Guid? personID = null;

            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(new Person());

            //act
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

            //Assert
            personResponse.Should().BeNull();

            ////Assert
            //Assert.Null(personResponse);
        }

        //if supply a valid person id, should return a Person response object
        [Fact]
        public async Task GetPersonByPersonID_ProperPersonID_ToBeSuccessful()
        {
            //Arange

            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "example@example.com")
                .With(temp => temp.Country, null as Country).Create();

            PersonResponse personResponseExpected =  person.ToPersonResponse();

            //Act
            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            PersonResponse? personResponseFromGetPersonByID = await _personsService.GetPersonByPersonID(person.PersonID);

            personResponseFromGetPersonByID.Should().Be(personResponseExpected);

            ////Assert
            //Assert.Equal(personResponseFromAddPerson, personResponseFromGetPersonByID);

        }


        #endregion

        #region GetAllPersons

        //The GetAllPersons should return an ampty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList_ToBeEmpty()
        {

            //Act
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(new List<Person>());

            List<PersonResponse> persons = await _personsService.GetAllPersons();

            persons.Should().BeEmpty();

            ////Assert
            //Assert.Empty(persons);
        }

        //first, we add few persons; and then when we call GetallPersons,
        //it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "example2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "example3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            List<PersonResponse> personResponseListExpected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListExpected)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            //Act
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);
            List<PersonResponse> personsResponseListFromGet = await _personsService.GetAllPersons();

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsResponseListFromGet)
            {
                _testOutputHelper.WriteLine(personResponseFromGet.ToString());
            }

            personsResponseListFromGet.Should().BeEquivalentTo(personResponseListExpected);

            ////Assert
            //foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            //{
            //    Assert.Contains(personResponseFromAdd, personsListFromGet);
            //}
        }

        #endregion

        #region GetFilteredPersons


        //If search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create()
            };

            List<PersonResponse> personResponseListExpected = persons.Select(temp => temp.ToPersonResponse()).ToList();


            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListExpected)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            //Act
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);
            List<PersonResponse> personsListFromSearch = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsListFromSearch)
            {
                _testOutputHelper.WriteLine(personResponseFromGet.ToString());
            }

            //Assert
            personsListFromSearch.Should().BeEquivalentTo(personResponseListExpected);

            ////Assert
            //foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            //{
            //    Assert.Contains(personResponseFromAdd, personsListFromSearch);
            //}

        }


        //search based on person name with some search string.It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create()
            };

            List<PersonResponse> personResponseListExpected = persons.Select(temp => temp.ToPersonResponse()).ToList();


            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListExpected)
            {
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            //Act
            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);
            List<PersonResponse> personsListFromSearch = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "a");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsListFromSearch)
            {
                _testOutputHelper.WriteLine(personResponseFromGet.ToString());
            }

            //Assert
            personsListFromSearch.Should().BeEquivalentTo(personResponseListExpected);

        }

        #endregion

        #region GetSortedPersons

        //When we sort based on PersonName in DESCENDING,
        //it should return persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful
            ()
        {

            //Arrange
            List<Person> persons = new List<Person>()
            {
                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create(),

                _fixture.Build<Person>()
                .With(temp => temp.Email, "example1@example.com")
                .With(temp => temp.Country, null as Country).Create()
            };

            List<PersonResponse> personResponseListExpected = persons.Select(temp => temp.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);

            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseExpected in personResponseListExpected)
            {
                _testOutputHelper.WriteLine(personResponseExpected.ToString());
            }

            List<PersonResponse> allPersons = await _personsService.GetAllPersons();

            //Act
            List<PersonResponse> personsResponseListFromSort = await _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderOptions.DESCENDING);

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromSort in personsResponseListFromSort)
            {
                _testOutputHelper.WriteLine(personResponseFromSort.ToString());
            }

            //Assert
            personsResponseListFromSort.Should().BeInDescendingOrder(temp => temp.PersonName);

            //personResponseListFromAdd = personResponseListFromAdd.OrderByDescending(p => p.PersonName).ToList();

            ////Assert
            //personsResponseListFromSort.Should().BeEquivalentTo(personResponseListFromAdd);

            ////Assert
            //for (int i = 0; i < personResponseListFromAdd.Count; i++)
            //{
            //    Assert.Equal(personResponseListFromAdd[i], personsResponseListFromSort[i]);
            //}

        }

        #endregion

        #region UpdatePerson 

        //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            //Act
            Func<Task> action = async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();

            ////Assert
            //await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            //    //Act
            //    await _personsService.UpdatePerson(personUpdateRequest)
            //);
        }

        //When we supply invalid person id, it should throw ArgumentException
        [Fact]

        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = _fixture.Build<PersonUpdateRequest>()
                        .With(temp => temp.Email, "example1@example.com").Create();

            //Act
            Func<Task> action = async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();

            ////Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () =>
            //    //Act
            //    await _personsService.UpdatePerson(personUpdateRequest)
            //);
        }

        //When the PersoneName is null, it should trow ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
            
            Person person = _fixture.Build<Person>()
                        .With(temp => temp.PersonName, null as string)
                        .With(temp => temp.Email, "example1@example.com")
                        .With(temp => temp.Country, null as Country)
                        .With(temp => temp.Gender, "Male").Create();

            PersonResponse personResponse = person.ToPersonResponse();

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            Func<Task> action = async () =>
            {
                await _personsService.UpdatePerson(personUpdateRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();

            ////Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () =>
            //    //Act
            //    await _personsService.UpdatePerson(personUpdateRequest));

        }

        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonNameDetailsUpdation_ToBeSuccessful()
        {
            //Arrange
            
            Person person = _fixture.Build<Person>()
                        .With(temp => temp.Email, "example1@example.com")
                        .With(temp => temp.Country, null as Country)
                        .With(temp => temp.Gender, "Male").Create();
            PersonResponse personResponseExpected = person.ToPersonResponse();

            PersonUpdateRequest personUpdateRequest = personResponseExpected.ToPersonUpdateRequest();

            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);
            //Act
            PersonResponse personResponseFromUpdate = await _personsService.UpdatePerson(personUpdateRequest);

            //Assert
            personResponseFromUpdate.Should().Be(personResponseExpected);

            ////Assert
            //Assert.Equal(personResponseFromGet, personResponseFromUpdate);

        }

        #endregion

        #region DeletePerson 

        //if you supply a valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            //Arange

            Person person = _fixture.Build<Person>()
                        .With(temp => temp.Email, "example1@example.com")
                        .With(temp => temp.Country, null as Country).Create();

            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(true);
            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            //Act
            bool isDeleted = await _personsService.DeletePerson(person.PersonID);

            isDeleted.Should().BeTrue();

            ////Assert
            //Assert.True(isDeleted);
        }

        //if you supply an invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID_ToBeFalse()
        {

            //Act
            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            //Assert
            isDeleted.Should().BeFalse();
            
            ////Assert
            //Assert.False(isDeleted);
        }

        #endregion
    }
}
