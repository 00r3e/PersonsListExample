using System;

using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace CRUDTests
{
    public class PersonServiceTest
    {

        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;

        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {

            _countriesService = new CountriesService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options));

            _personsService = new PersonsService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options), _countriesService);
                        _testOutputHelper = testOutputHelper;
        }

        #region AddPerson


        //When we supply null value as PersonAddRequest,it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;


            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            //Act
                await _personsService.AddPerson(personAddRequest));
        }

        //When we supply null value as PersonName,it should throw ArgumentException

        [Fact]
        public async Task AddPerson_NullPersonName()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest { PersonName = null };


            await Assert.ThrowsAsync<ArgumentException>(async () =>
            //Act
                await _personsService.AddPerson(personAddRequest));
        }

        //When we supply proper person details, it should insert the person in to the persons list
        //and it should return an object of PersonRespons, witch includes the newly generated person id

        [Fact]
        public async Task AddPerson_ProperPersonDetails()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest
            {
                PersonName = "Name",
                Email = "Name@qqq.com",
                Address = "sample address",
                CountryID = Guid.NewGuid(),
                Gender = GenderOptions.Female,
                DateOfBirth = DateTime.Parse("2000-01-01"),
                ReceiveNewsLetters = true
            };

            //Act
            PersonResponse personResponse = await _personsService.AddPerson(personAddRequest);

            //Assert
            Assert.True(personResponse.PersonID != Guid.Empty);

            Assert.Contains(personResponse, await _personsService.GetAllPersons());
        }


        #endregion


        #region GetPersonByPersonID

        //If suplly null value as argument it should return null
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID()
        {
            //Arrange
            Guid? personID = null;

            //act
            PersonResponse? personResponse = await _personsService.GetPersonByPersonID(personID);

            //Assert
            Assert.Null(personResponse);
        }

        //if supply a valid person id, should return a Person response object
        [Fact]

        public async Task GetPersonByPersonID_ProperPersonID()
        {
            //Arange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "China" };

            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest person = new PersonAddRequest
            {
                PersonName = "Name",
                Email = "sample@mail.com",
                CountryID = countryResponse.CountryID,
                Address = "Address",
                DateOfBirth = DateTime.Parse("2000-02-02"),
                Gender = GenderOptions.Other,
                ReceiveNewsLetters = true
            };

            //Act
            PersonResponse? personResponseFromAddPerson = await _personsService.AddPerson(person);

            PersonResponse? personResponseFromGetPersonByID = await _personsService.GetPersonByPersonID(personResponseFromAddPerson.PersonID);

            //Assert
            Assert.Equal(personResponseFromAddPerson, personResponseFromGetPersonByID);

        }


        #endregion

        #region GetAllPersons

        //The GetAllPersons should return an ampty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            //Act
            List<PersonResponse> persons = await _personsService.GetAllPersons();

            //Assert
            Assert.Empty(persons);
        }

        //first, we add few persons; and then when we call GetallPersons,
        //it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_AddProperPersons()
        {
            //Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = countryResponse1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest personrequest2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = countryResponse2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest personRequest3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = countryResponse2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personRequests = new List<PersonAddRequest>() { personRequest1, personrequest2, personRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personRequest in personRequests)
            {
                PersonResponse personResponse = await _personsService.AddPerson(personRequest);
                personResponseListFromAdd.Add(personResponse);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {

                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            //Act
            List<PersonResponse> personsListFromGet = await _personsService.GetAllPersons();

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsListFromGet)
            {
                _testOutputHelper.WriteLine(personResponseFromGet.ToString());
            }

            //Assert
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                Assert.Contains(personResponseFromAdd, personsListFromGet);
            }


        }

        #endregion

        #region GetFilteredPersons


        //If search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = await _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = countryResponse1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest personRequest2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = countryResponse2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest personRequest3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = countryResponse2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personRequests = new List<PersonAddRequest>() { personRequest1, personRequest2, personRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personRequest in personRequests)
            {
                PersonResponse personResponse = await _personsService.AddPerson(personRequest);
                personResponseListFromAdd.Add(personResponse);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                CountryResponse? countryResponse = await _countriesService.GetCountryByCountryID(personResponseFromAdd.CountryID);
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
            }

            //Act
            List<PersonResponse> personsListFromSearch = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsListFromSearch)
            {
                _testOutputHelper.WriteLine(personResponseFromGet.ToString());
            }

            //Assert
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                Assert.Contains(personResponseFromAdd, personsListFromSearch);
            }

        }


        //First we will add few persons; and then we will search based on person name
        //with some search string.It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryresponse2 = await _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = countryResponse1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest personRequest2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = countryresponse2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest personRequest3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = countryresponse2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personRequests = new List<PersonAddRequest>() { personRequest1, personRequest2, personRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personRequest in personRequests)
            {
                PersonResponse personResponse = await _personsService.AddPerson(personRequest);
                personResponseListFromAdd.Add(personResponse);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                if (personResponseFromAdd.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                {
                    _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
                }
            }

            //Act
            List<PersonResponse> personsListFromSearch = await _personsService.GetFilteredPersons(nameof(Person.PersonName), "ma");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsListFromSearch)
            {
                _testOutputHelper.WriteLine(personResponseFromGet.ToString());
            }

            //Assert
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                if (personResponseFromAdd.PersonName != null)
                {
                    if (personResponseFromAdd.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                    {
                        Assert.Contains(personResponseFromAdd, personsListFromSearch);
                    }
                }
            }
        }

        #endregion

        #region GetSortedPersons

        //When we sort based on PersonName in DESCENDING,
        //it should return persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons()
        {
            //Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse countryResponse1 = await _countriesService.AddCountry(countryRequest1);
            CountryResponse countryresponse2 = await _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = countryResponse1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest personRequest2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = countryresponse2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest personRequest3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = countryresponse2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personRequests = new List<PersonAddRequest>() { personRequest1, personRequest2, personRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personRequest in personRequests)
            {
                PersonResponse personResponse = await _personsService.AddPerson(personRequest);
                personResponseListFromAdd.Add(personResponse);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                if (personResponseFromAdd.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                {
                    _testOutputHelper.WriteLine(personResponseFromAdd.ToString());
                }
            }

            List<PersonResponse> allPersons = await _personsService.GetAllPersons();

            //Act
            List<PersonResponse> personsListFromSort = await _personsService.GetSortedPersons(allPersons, nameof(Person.PersonName),SortOrderOptions.DESCENDING);

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromSort in personsListFromSort)
            {
                _testOutputHelper.WriteLine(personResponseFromSort.ToString());
            }
            personResponseListFromAdd = personResponseListFromAdd.OrderByDescending(p => p.PersonName).ToList();

            //Assert

            for (int i = 0; i < personResponseListFromAdd.Count; i++)
            {
                Assert.Equal(personResponseListFromAdd[i], personsListFromSort[i]);
            }
            
        }

        #endregion

        #region UpdatePerson 

        //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                //Act
                await _personsService.UpdatePerson(personUpdateRequest)
            );
        }

        //When we supply invalid person id, it should throw ArgumentException
        [Fact]

        public async Task UpdatePerson_InvalidPersonID()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = 
                new PersonUpdateRequest() { PersonID = Guid.NewGuid() };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
                //Act
                await _personsService.UpdatePerson(personUpdateRequest)
            );
        }

        //When the PersoneName is null, it should trow ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull()
        {
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);


            PersonAddRequest personAddRequest =
                new PersonAddRequest() { PersonName = "John", CountryID = 
                countryResponse.CountryID, Email = "name@example.com", Address = "Sample Address",
                Gender = GenderOptions.Other};
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            PersonUpdateRequest personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.PersonName = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentException>( async () => 
                //Act
                await _personsService.UpdatePerson(personUpdateRequest));

        }

        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonNameDetailsUpdation()
        {
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse countryResponse = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest =
                new PersonAddRequest() { PersonName = "John", CountryID = countryResponse.CountryID, 
                    Address = "Sample address", DateOfBirth = DateTime.Parse("2000-01-01"), 
                    Email = "sample@email.com", Gender = GenderOptions.Other, ReceiveNewsLetters = true };
            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest);

            PersonUpdateRequest personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.PersonName = "William";
            personUpdateRequest.Email = "sample2@email.com";

            PersonResponse personResponseFromUpdate = await _personsService.UpdatePerson(personUpdateRequest);

            PersonResponse? personResponseFromGet = await _personsService.GetPersonByPersonID(personResponseFromUpdate.PersonID);

            //Assert
            Assert.Equal(personResponseFromGet, personResponseFromUpdate);
            
        }

        #endregion

        #region DeletePerson 

        //if you supply a valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            //Arange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "USA" };
            CountryResponse countryResponseFromAdd = await _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest = new PersonAddRequest()
            {
                PersonName = "Max",
                Address = "Sample Address",
                CountryID = countryResponseFromAdd.CountryID,
                DateOfBirth = Convert.ToDateTime("2000-02-02"),
                Email = "sample@example.com",
                Gender = GenderOptions.Other,
                ReceiveNewsLetters = true
            };

            PersonResponse personResponseFromAdd = await _personsService.AddPerson(personAddRequest); 

            //Act
            bool isDeleted = await _personsService.DeletePerson(personResponseFromAdd.PersonID);

            //Assert
            Assert.True(isDeleted);
        }

        //if you supply an invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {

            //Act
            bool isDeleted = await _personsService.DeletePerson(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
        }

        #endregion
    }
}
