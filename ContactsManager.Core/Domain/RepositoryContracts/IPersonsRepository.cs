using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ContactsManager.Core.Domain.Entities;

namespace RepositoryContracts
{
    /// <summary>
    /// Interface to the data access logic for the Persons entity
    /// </summary>
    public interface IPersonsRepository
    {
        /// <summary>
        /// Adds a new Person to the data store
        /// </summary>
        /// <param name="person">Person to add</param>
        /// <returns>Returns the Person after adding it to the data store</returns>
        Task<Person> AddPerson(Person person);

        /// <summary>
        /// Returns all persons in the data store
        /// </summary>
        /// <returns>List of all persons in the data store</returns>
        Task<List<Person>> GetAllPersons();

        /// <summary>
        /// Searching for a person in data store, based on a given id
        /// </summary>
        /// <param name="personID">the Person id (Guid to search)</param>
        /// <returns>If there is a match, returns the matching person, otherwise returns null</returns>
        Task<Person?> GetPersonById(Guid personID);

        /// <summary>
        /// Returns all person object based on the given expression
        /// </summary>
        /// <param name="predicate">LINQ expression to check</param>
        /// <returns>all matching persons with given condition</returns>
       Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate);

        /// <summary>
        /// Deletes a person object based on the person id
        /// </summary>
        /// <param name="personID">Person id to search</param>
        /// <returns>Returns true, if the deletion is seccessful, otherwise false</returns>
        Task<bool> DeletePersonByPersonID(Guid personID);

        /// <summary>
        /// Updates a person object
        /// </summary>
        /// <param name="person">Person object to update</param>
        /// <returns>Returns the update person object</returns>
        Task<Person> UpdatePerson(Person person);
    }
}
