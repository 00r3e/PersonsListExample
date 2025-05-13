using System;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Services.Helpers;
using System.Reflection;
using ServiceContracts.Enums;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        //private field
        private readonly List<Person> _persons;
        private readonly ICountriesService _countriesService;

        public PersonsService(bool initialize = true)
        {
            _persons = new List<Person>();
            _countriesService = new CountriesService();

            if (initialize)
            {
                _persons.AddRange(new List<Person>() 
                { 
                    new Person() { PersonID = Guid.Parse("2C2BB064-50F9-4548-B702-939E6AF18BCF"),
                    PersonName = "Hansiain", Email = "hstaddart0@scientificamerican.com",
                    DateOfBirth = DateTime.Parse("1999-01-10"), Address = "3809 Crownhardt Terrace",
                    Gender = "Male" , ReceiveNewsLetters = true, CountryID = Guid.Parse("7CA6E811-9879-4F5D-90DF-051D1200D111")},
                    new Person() { PersonID = Guid.Parse("1F323BD6-0D75-46AB-B63E-4429DDA5F667"),
                    PersonName = "Paquito", Email = "pduckerin1@php.net",
                    DateOfBirth = DateTime.Parse("1998-12-04"), Address = "1668 Badeau Terrace",
                    Gender = "Male" , ReceiveNewsLetters = true,  CountryID = Guid.Parse("129DDFFE-D967-45B6-943E-4536384F2731") },
                    new Person() { PersonID = Guid.Parse("D8A4A97A-BDB5-42D4-8633-EF20848BC3F5"),
                    PersonName = "Dannye", Email = "dekless2@cdc.gov",
                    DateOfBirth = DateTime.Parse("1994-07-23"), Address = "0684 Eggendart Plaza",
                    Gender = "Female" , ReceiveNewsLetters = true,  CountryID = Guid.Parse("25EE8371-43DD-41A9-A9BC-011D3A6FDB06") },
                    new Person() { PersonID = Guid.Parse("5DA3B7EC-58C9-4390-920E-1D9DD88B758D"),
                    PersonName = "Zacherie", Email = "zguye3@1688.com",
                    DateOfBirth = DateTime.Parse("2000-05-27"), Address = "55786 Dapin Hil",
                    Gender = "Male" , ReceiveNewsLetters = false,  CountryID = Guid.Parse("5CF549CE-FF0C-467D-84BF-FC541EB82CBD") },
                    new Person() { PersonID = Guid.Parse("BF2B3192-B68F-4ED8-B10A-54D354A154C6"),
                    PersonName = "Radcliffe", Email = "rcockley4@mayoclinic.com",
                    DateOfBirth = DateTime.Parse("1998-11-20"), Address = "27 Gina Avenue",
                    Gender = "Male" , ReceiveNewsLetters = false,  CountryID = Guid.Parse("7F8FCF18-BBD7-416D-BDB1-908BA33FB1E2") },
                    new Person() { PersonID = Guid.Parse("84F626EF-E1FF-4720-9B8E-2E0EBBF98DA1"),
                    PersonName = "Yuma", Email = "ysammes5@netlog.com",
                    DateOfBirth = DateTime.Parse("1993-10-10"), Address = "790 Cody Parkway",
                    Gender = "Male" , ReceiveNewsLetters = false,  CountryID = Guid.Parse("129DDFFE-D967-45B6-943E-4536384F2731") }
                });

            }
        }

        private PersonResponse ConvertPersonToPersoneResponse(Person person)
        {
            //Convert the Person Object into PersonResponse type
            PersonResponse personResponse = person.ToPersonResponse();

            personResponse.Country = _countriesService.GetCountryByCountryID(person.CountryID)?.CountryName;

            return personResponse;
        }

        public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
        {
            //Check if PersonAddRequest is not null
            if (personAddRequest == null) 
            { 
                throw new ArgumentNullException(nameof(personAddRequest)); 
            }

            //Model Validation
            ValidationHelper.ModelValidation(personAddRequest);

            //Convert personAddRequest into Person type
            Person person = personAddRequest.ToPerson();

            //generate PersonId
            person.PersonID = Guid.NewGuid();

            //Add Person to list
            _persons.Add(person);

            //Convert the Person Object into PersonResponse type
            return ConvertPersonToPersoneResponse(person);

        }

        public List<PersonResponse> GetAllPersons()
        {
            return _persons.Select(p => ConvertPersonToPersoneResponse(p)).ToList();
        }

        public PersonResponse? GetPersonByPersonID(Guid? personID)
        {
            if(!personID.HasValue)
            {
                return null;
            }

            Person? person = _persons.FirstOrDefault(p => p.PersonID == personID);

            if (person == null) 
            {
                return null;
            }

            return ConvertPersonToPersoneResponse(person);
        }

        public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<Person> persons = new List<Person>();

            List<PersonResponse> allPersons = GetAllPersons();

            List<PersonResponse> matchingPersons = allPersons;

            if (string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(searchBy)) return matchingPersons;

            var propertyInfo = typeof(Person).GetProperty(searchBy);
            if (propertyInfo != null)
            {
                persons = _persons.Where(person =>
                {
                    var value = propertyInfo.GetValue(person);

                    if (value == null)
                        return true;
           
                    if (value is DateTime dateTimeValue)
                    {
                        string formattedDate = dateTimeValue.ToString("dd MMMM yyyy");
                        return formattedDate.Contains(searchString, StringComparison.OrdinalIgnoreCase);
                    }

                    if (searchBy == "Gender" )
                    {
                        string? stringValue = value?.ToString()?.ToLower();
                        return Equals(searchString.ToLower(), stringValue);
                    }

                    // Handle other types (string, int, etc.)
                    string? valueAsString = value.ToString();
                    return valueAsString?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? true;

                }).ToList();
            }

            return persons.Select(p =>  ConvertPersonToPersoneResponse(p)).ToList();

        }

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return allPersons;
            }

            List<PersonResponse> sortedPersons = (sortBy, sortOrder)
            switch
            {
                (nameof(PersonResponse.PersonName), SortOrderOptions.ASCENDING) =>
                allPersons.OrderBy(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.PersonName), SortOrderOptions.DESCENDING) =>
                allPersons.OrderByDescending(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.ASCENDING) =>
                allPersons.OrderBy(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Email), SortOrderOptions.DESCENDING) =>
                allPersons.OrderByDescending(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.ASCENDING) =>
                allPersons.OrderBy(p => p.DateOfBirth).ToList(),

                (nameof(PersonResponse.DateOfBirth), SortOrderOptions.DESCENDING) =>
                allPersons.OrderByDescending(p => p.DateOfBirth).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.ASCENDING) =>
                allPersons.OrderBy(p => p.Age).ToList(),

                (nameof(PersonResponse.Age), SortOrderOptions.DESCENDING) =>
                allPersons.OrderByDescending(p => p.Age).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.ASCENDING) =>
                allPersons.OrderBy(p => p.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Gender), SortOrderOptions.DESCENDING) =>
                allPersons.OrderByDescending(p => p.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.ASCENDING) =>
                allPersons.OrderBy(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Country), SortOrderOptions.DESCENDING) =>
                allPersons.OrderByDescending(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.ASCENDING) =>
                allPersons.OrderBy(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.Address), SortOrderOptions.DESCENDING) =>
                allPersons.OrderByDescending(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.ASCENDING) =>
                allPersons.OrderBy(p => p.ReceiveNewsLetters).ToList(),

                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderOptions.DESCENDING) =>
                allPersons.OrderByDescending(p => p.ReceiveNewsLetters).ToList(),

                _ => allPersons
            };

            return sortedPersons;
        }

        public PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest == null)
            {
                throw new ArgumentNullException(nameof(Person));
            }

            //Validation
            ValidationHelper.ModelValidation(personUpdateRequest);

            //get matching person object to update
            Person? matchingPerson = _persons.FirstOrDefault(p => p.PersonID == personUpdateRequest.PersonID);

            if (matchingPerson == null) 
            {
                throw new ArgumentException("Given person id doesn't exist");
            }

            //update all details
            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryID = personUpdateRequest.CountryID;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            return ConvertPersonToPersoneResponse(matchingPerson);
        }

        public bool DeletePerson(Guid? personID)
        {
            if(personID == null)
            {
                throw new ArgumentNullException(nameof(personID));
            }

            Person? person = _persons.FirstOrDefault(p => p.PersonID == personID);

            if(person == null) { return false; }

            _persons.RemoveAll(p => p.PersonID == personID );

            return true;
        }
    }
}
