using System;

using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using Entities;

namespace CRUDTests
{
    public class PersonServiceTest
    {

        private readonly IPersonsService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;

        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {
            _personService = new PersonsService();
            _countriesService = new CountriesService(false);
            _testOutputHelper = testOutputHelper;
        }

        #region AddPerson


        //When we supply null value as PersonAddRequest,it should throw ArgumentNullException
        [Fact]
        public void AddPerson_NullPerson()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;


            Assert.Throws<ArgumentNullException>(() =>
            //Act
                _personService.AddPerson(personAddRequest));
        }

        //When we supply null value as PersonName,it should throw ArgumentException

        [Fact]
        public void AddPerson_NullPersonName()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest { PersonName = null };


            Assert.Throws<ArgumentException>(() =>
            //Act
                _personService.AddPerson(personAddRequest));
        }

        //When we supply proper person details, it should insert the person in to the persons list
        //and it should return an object of PersonRespons, witch includes the newly generated person id

        [Fact]
        public void AddPerson_ProperPersonDetails()
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
            PersonResponse personResponse = _personService.AddPerson(personAddRequest);

            //Assert
            Assert.True(personResponse.PersonID != Guid.Empty);

            Assert.Contains(personResponse, _personService.GetAllPersons());
        }


        #endregion


        #region GetPersonByPersonID

        //If suplly null value as argument it should return null
        [Fact]
        public void GetPersonByPersonID_NullPersonID()
        {
            //Arrange
            Guid? personID = null;

            //act
            PersonResponse? personResponse = _personService.GetPersonByPersonID(personID);

            //Assert
            Assert.Null(personResponse);
        }

        //if supply a valid person id, should return a Person response object
        [Fact]

        public void GetPersonByPersonID_ProperPersonID()
        {
            //Arange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "China" };

            CountryResponse countryResponse = _countriesService.AddCountry(countryAddRequest);

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
            PersonResponse? personResponseFromAddPerson = _personService.AddPerson(person);

            PersonResponse? personResponseFromGetPersonByID = _personService.GetPersonByPersonID(personResponseFromAddPerson.PersonID);

            //Assert
            Assert.Equal(personResponseFromAddPerson, personResponseFromGetPersonByID);

        }


        #endregion

        #region GetAllPersons

        //The GetAllPersons should return an ampty list by default
        [Fact]
        public void GetAllPersons_EmptyList()
        {
            //Act
            List<PersonResponse> persons = _personService.GetAllPersons();

            //Assert
            Assert.Empty(persons);
        }

        //first, we add few persons; and then when we call GetallPersons,
        //it should return the same persons that were added
        [Fact]
        public void GetAllPersons_AddProperPersons()
        {
            //Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse countryResponse1 = _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = countryResponse1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest personrequest2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = countryResponse2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest personRequest3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = countryResponse2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personRequests = new List<PersonAddRequest>() { personRequest1, personrequest2, personRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personRequest in personRequests)
            {
                PersonResponse personResponse = _personService.AddPerson(personRequest);
                personResponseListFromAdd.Add(personResponse);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {

                CountryResponse? countryResponse = _countriesService.GetCountryByCountryID(personResponseFromAdd.CountryID);

                _testOutputHelper.WriteLine(personResponseFromAdd.ToString(countryResponse.CountryName));
            }

            //Act
            List<PersonResponse> personsListFromGet = _personService.GetAllPersons();

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsListFromGet)
            {
                CountryResponse? countryResponse = _countriesService.GetCountryByCountryID(personResponseFromGet.CountryID);

                _testOutputHelper.WriteLine(personResponseFromGet.ToString(countryResponse.CountryName));
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
        public void GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse countryResponse1 = _countriesService.AddCountry(countryRequest1);
            CountryResponse countryResponse2 = _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = countryResponse1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest personRequest2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = countryResponse2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest personRequest3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = countryResponse2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personRequests = new List<PersonAddRequest>() { personRequest1, personRequest2, personRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personRequest in personRequests)
            {
                PersonResponse personResponse = _personService.AddPerson(personRequest);
                personResponseListFromAdd.Add(personResponse);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                CountryResponse? countryResponse = _countriesService.GetCountryByCountryID(personResponseFromAdd.CountryID);
                _testOutputHelper.WriteLine(personResponseFromAdd.ToString(countryResponse?.CountryName));
            }

            //Act
            List<PersonResponse> personsListFromSearch = _personService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsListFromSearch)
            {
                CountryResponse? countryResponse = _countriesService.GetCountryByCountryID(personResponseFromGet.CountryID);
                _testOutputHelper.WriteLine(personResponseFromGet.ToString(countryResponse?.CountryName));
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
        public void GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse countryResponse1 = _countriesService.AddCountry(countryRequest1);
            CountryResponse countryresponse2 = _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = countryResponse1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest personRequest2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = countryresponse2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest personRequest3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = countryresponse2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personRequests = new List<PersonAddRequest>() { personRequest1, personRequest2, personRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personRequest in personRequests)
            {
                PersonResponse personResponse = _personService.AddPerson(personRequest);
                personResponseListFromAdd.Add(personResponse);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                if (personResponseFromAdd.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                {
                    CountryResponse? countryResponse = _countriesService.GetCountryByCountryID(personResponseFromAdd.CountryID);
                    _testOutputHelper.WriteLine(personResponseFromAdd.ToString(countryResponse.CountryName));
                }
            }

            //Act
            List<PersonResponse> personsListFromSearch = _personService.GetFilteredPersons(nameof(Person.PersonName), "ma");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromGet in personsListFromSearch)
            {
                CountryResponse? countryResponse = _countriesService.GetCountryByCountryID(personResponseFromGet.CountryID);
                _testOutputHelper.WriteLine(personResponseFromGet.ToString(countryResponse.CountryName));
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
        public void GetSortedPersons()
        {
            //Arrange
            CountryAddRequest countryRequest1 = new CountryAddRequest() { CountryName = "USA" };
            CountryAddRequest countryRequest2 = new CountryAddRequest() { CountryName = "India" };

            CountryResponse countryResponse1 = _countriesService.AddCountry(countryRequest1);
            CountryResponse countryresponse2 = _countriesService.AddCountry(countryRequest2);

            PersonAddRequest personRequest1 = new PersonAddRequest() { PersonName = "Smith", Email = "smith@example.com", Gender = GenderOptions.Male, Address = "address of smith", CountryID = countryResponse1.CountryID, DateOfBirth = DateTime.Parse("2002-05-06"), ReceiveNewsLetters = true };

            PersonAddRequest personRequest2 = new PersonAddRequest() { PersonName = "Mary", Email = "mary@example.com", Gender = GenderOptions.Female, Address = "address of mary", CountryID = countryresponse2.CountryID, DateOfBirth = DateTime.Parse("2000-02-02"), ReceiveNewsLetters = false };

            PersonAddRequest personRequest3 = new PersonAddRequest() { PersonName = "Rahman", Email = "rahman@example.com", Gender = GenderOptions.Male, Address = "address of rahman", CountryID = countryresponse2.CountryID, DateOfBirth = DateTime.Parse("1999-03-03"), ReceiveNewsLetters = true };

            List<PersonAddRequest> personRequests = new List<PersonAddRequest>() { personRequest1, personRequest2, personRequest3 };

            List<PersonResponse> personResponseListFromAdd = new List<PersonResponse>();

            foreach (PersonAddRequest personRequest in personRequests)
            {
                PersonResponse personResponse = _personService.AddPerson(personRequest);
                personResponseListFromAdd.Add(personResponse);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse personResponseFromAdd in personResponseListFromAdd)
            {
                if (personResponseFromAdd.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase))
                {
                    CountryResponse? countryResponse = _countriesService.GetCountryByCountryID(personResponseFromAdd.CountryID);
                    _testOutputHelper.WriteLine(personResponseFromAdd.ToString(countryResponse.CountryName));
                }
            }

            List<PersonResponse> allPersons = _personService.GetAllPersons();

            //Act
            List<PersonResponse> personsListFromSort = _personService.GetSortedPersons(allPersons, nameof(Person.PersonName),SortOrderOptions.DESCENDING);

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse personResponseFromSort in personsListFromSort)
            {
                CountryResponse? countryResponse = _countriesService.GetCountryByCountryID(personResponseFromSort.CountryID);
                _testOutputHelper.WriteLine(personResponseFromSort.ToString(countryResponse.CountryName));
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
        public void UpdatePerson_NullPerson()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
                //Act
                _personService.UpdatePerson(personUpdateRequest)
            );
        }

        //When we supply invalid person id, it should throw ArgumentException
        [Fact]

        public void UpdatePerson_InvalidPersonID()
        {
            //Arrange
            PersonUpdateRequest? personUpdateRequest = 
                new PersonUpdateRequest() { PersonID = Guid.NewGuid() };

            //Assert
            Assert.Throws<ArgumentException>(() =>
                //Act
                _personService.UpdatePerson(personUpdateRequest)
            );
        }

        //When the PersoneName is null, it should trow ArgumentException
        [Fact]
        public void UpdatePerson_PersonNameIsNull()
        {
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse countryResponse = _countriesService.AddCountry(countryAddRequest);


            PersonAddRequest personAddRequest =
                new PersonAddRequest() { PersonName = "John", CountryID = 
                countryResponse.CountryID, Email = "name@example.com", Address = "Sample Address",
                Gender = GenderOptions.Other};
            PersonResponse personResponseFromAdd = _personService.AddPerson(personAddRequest);

            PersonUpdateRequest personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.PersonName = null;

            //Assert
            Assert.Throws<ArgumentException>( () => 
                //Act
                _personService.UpdatePerson(personUpdateRequest));

        }

        //First, add a new person and try to update the person name and email
        [Fact]
        public void UpdatePerson_PersonNameDetailsUpdation()
        {
            //Arrange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse countryResponse = _countriesService.AddCountry(countryAddRequest);

            PersonAddRequest personAddRequest =
                new PersonAddRequest() { PersonName = "John", CountryID = countryResponse.CountryID, 
                    Address = "Sample address", DateOfBirth = DateTime.Parse("2000-01-01"), 
                    Email = "sample@email.com", Gender = GenderOptions.Other, ReceiveNewsLetters = true };
            PersonResponse personResponseFromAdd = _personService.AddPerson(personAddRequest);

            PersonUpdateRequest personUpdateRequest = personResponseFromAdd.ToPersonUpdateRequest();
            personUpdateRequest.PersonName = "William";
            personUpdateRequest.Email = "sample2@email.com";

            PersonResponse personResponseFromUpdate = _personService.UpdatePerson(personUpdateRequest);

            PersonResponse personResponseFromGet= _personService.GetPersonByPersonID(personResponseFromUpdate.PersonID);

            //Assert
            Assert.Equal(personResponseFromGet, personResponseFromUpdate);
            
        }

        #endregion

        #region DeletePerson 

        //if you supply a valid PersonID, it should return true
        [Fact]
        public void DeletePerson_ValidPersonID()
        {
            //Arange
            CountryAddRequest countryAddRequest = new CountryAddRequest() { CountryName = "USA" };
            CountryResponse countryResponseFromAdd = _countriesService.AddCountry(countryAddRequest);

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

            PersonResponse personResponseFromAdd = _personService.AddPerson(personAddRequest); 

            //Act
            bool isDeleted = _personService.DeletePerson(personResponseFromAdd.PersonID);

            //Assert
            Assert.True(isDeleted);
        }

        //if you supply an invalid PersonID, it should return false
        [Fact]
        public void DeletePerson_InvalidPersonID()
        {

            //Act
            bool isDeleted = _personService.DeletePerson(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
        }

        #endregion
    }
}
