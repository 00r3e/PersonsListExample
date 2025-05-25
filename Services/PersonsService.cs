using System;
using Entities;
using ServiceContracts.DTO;
using ServiceContracts;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Services.Helpers;
using System.Reflection;
using ServiceContracts.Enums;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        //private field
        private readonly PersonsDbContext _personsDbContext;
        private readonly ICountriesService _countriesService;

        public PersonsService(PersonsDbContext personsDbContext, ICountriesService countriesService)
        {
            _personsDbContext= personsDbContext;
            _countriesService = countriesService;

        }

        //private PersonResponse ConvertPersonToPersoneResponse(Person person)
        //{
        //    //Convert the Person Object into PersonResponse type
        //    PersonResponse personResponse = person.ToPersonResponse();

        //    personResponse.Country = person.Country?.CountryName;

        //    return personResponse;
        //}

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
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
            _personsDbContext.Persons.Add(person);

            await _personsDbContext.SaveChangesAsync();
            //_personsDbContext.sp_InsertPerson(person);

            //Convert the Person Object into PersonResponse type
            return person.ToPersonResponse();

        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            var persons = await _personsDbContext.Persons.Include("Country").ToListAsync();

            return persons.Select(p => p.ToPersonResponse()).ToList();
            //return _personsDbContext.sp_GetAllPersons().Select(p => ConvertPersonToPersoneResponse(p)).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            if(!personID.HasValue)
            {
                return null;
            }

            Person? person = await _personsDbContext.Persons.Include("Country").FirstOrDefaultAsync(p => p.PersonID == personID);

            if (person == null) 
            {
                return null;
            }

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {

            List<Person> persons = await _personsDbContext.Persons.ToListAsync();

            if (string.IsNullOrEmpty(searchString) || string.IsNullOrEmpty(searchBy)) return await GetAllPersons();

            var propertyInfo = typeof(Person).GetProperty(searchBy);
            if (propertyInfo != null)
            {
                persons = persons.Where(person =>
                {
                    var value = propertyInfo.GetValue(person);

                    if (value == null)
                        return true;

                    if (value is DateTime dateTimeValue)
                    {
                        string formattedDate = dateTimeValue.ToString("dd MMMM yyyy");
                        return formattedDate.Contains(searchString, StringComparison.OrdinalIgnoreCase);
                    }

                    if (searchBy == "Gender")
                    {
                        string? stringValue = value?.ToString()?.ToLower();
                        return Equals(searchString.ToLower(), stringValue);
                    }

                    // Handle other types (string, int, etc.)
                    string? valueAsString = value.ToString();
                    return valueAsString?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? true;

                }).ToList();
            
            }
                return persons.Select(p => p.ToPersonResponse()).ToList();
        }

        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderOptions sortOrder)
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

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest == null)
            {
                throw new ArgumentNullException(nameof(Person));
            }

            //Validation
            ValidationHelper.ModelValidation(personUpdateRequest);

            //get matching person object to update
            Person? matchingPerson = await _personsDbContext.Persons.FirstOrDefaultAsync(p => p.PersonID == personUpdateRequest.PersonID);

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

            await _personsDbContext.SaveChangesAsync(); //Update

            return matchingPerson.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            if(personID == null)
            {
                throw new ArgumentNullException(nameof(personID));
            }

            Person? person = await _personsDbContext.Persons.FirstOrDefaultAsync(p => p.PersonID == personID);

            if(person == null) { return false; }

            _personsDbContext.Persons.Remove(_personsDbContext.Persons.First(p => p.PersonID == personID));
            await _personsDbContext.SaveChangesAsync();
            return true;
        }
    }
}
