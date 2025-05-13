using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO
{
    public class PersonResponse
    {
        /// <summary>
        /// Represents DTO class that is used as return type of most methods of Persons Service
        /// </summary>
        /// 
        public Guid PersonID { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public double? Age { get; set; }

        /// <summary>
        /// compares the current object data with the parameter object
        /// </summary>
        /// <param name="obj">The PersonResponse Object to compare</param>
        /// <returns>True or False, indicating whether all person details are matched 
        /// with the spacified parameter object</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if (obj.GetType() != typeof(PersonResponse)) return false;

            PersonResponse personResponse = (PersonResponse)obj;

            return PersonID == personResponse.PersonID && PersonName == personResponse.PersonName &&
                    Email == personResponse.Email && DateOfBirth == personResponse.DateOfBirth &&
                    Gender == personResponse.Gender && Country ==  personResponse.Country &&
                    CountryID == personResponse.CountryID && Address == personResponse.Address && 
                    ReceiveNewsLetters == personResponse.ReceiveNewsLetters;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"Person ID: {PersonID},Person Name: {PersonName},Email: {Email},Date Of Birth: " +
                $"{DateOfBirth},Gender: {Gender},Country: {Country},Country ID: {CountryID}, " +
                $"Address: {Address}, ReceiveNewsLetters: {ReceiveNewsLetters}";
        }

        public string ToString(string? CountryName)
        {
            return $"Person ID: {PersonID},Person Name: {PersonName},Email: {Email},Date Of Birth: " +
                $"{DateOfBirth},Gender: {Gender},Country: {CountryName},Country ID: {CountryID}, " +
                $"Address: {Address}, ReceiveNewsLetters: {ReceiveNewsLetters}";
        }

        public PersonUpdateRequest ToPersonUpdateRequest()
        {
            return new PersonUpdateRequest()
            {
                PersonID = PersonID,
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth,
                ReceiveNewsLetters = ReceiveNewsLetters,
                Address = Address,
                CountryID = CountryID,
                Gender = (GenderOptions)Enum.Parse(typeof(GenderOptions),Gender, true)
            };
        }
    }

    public static class PersonExtensions
    {

        /// <summary>
        /// An extension method to convert an object of Person class into PersonResponse class
        /// </summary>
        /// <param name="person">The Person object to convert</param>
        /// <returns>Returns the converted PersonResponse object</returns>
        public static PersonResponse ToPersonResponse(this Person person)
        {


            return new PersonResponse()
            {
                PersonID = person.PersonID, PersonName = person.PersonName, 
                Email = person.Email, DateOfBirth = person.DateOfBirth,
                ReceiveNewsLetters = person.ReceiveNewsLetters, Address = person.Address,
                CountryID = person.CountryID, Gender = person.Gender,
                Age = (person.DateOfBirth != null) ?  Math.Round
                ((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25) : null
            };
        }

        
    }
}
