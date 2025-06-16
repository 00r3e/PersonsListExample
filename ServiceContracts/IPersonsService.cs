using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{

    /// <summary>
    /// Represents business logic for manipulating
    /// </summary>
    public interface IPersonsService
    {
        /// <summary>
        /// Adds a new Person in to an existing list of persons
        /// </summary>
        /// <param name="personAddRequest">Person to add</param>
        /// <returns>Returns the same Person Details,
        /// along with newly generated PersonID</returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);


        /// <summary>
        /// Returns all persons
        /// </summary>
        /// <returns>Returns a list of object of PersonResponse type</returns>
        Task<List<PersonResponse>> GetAllPersons();

        /// <summary>
        /// Returns the person object based on the given personid
        /// </summary>
        /// <param name="personID">person id to search</param>
        /// <returns>Returns matching person object</returns>
        Task<PersonResponse?> GetPersonByPersonID(Guid? personID);

        /// <summary>
        /// Returns all person objects that matches with the given search field and search string
        /// </summary>
        /// <param name="searchBy">Search field to search</param>
        /// <param name="searchString">search string to search</param>
        /// <returns>Returs all the matching persons based on the given search field and search string</returns>
        Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString);
        
        
        /// <summary>
        /// Returns sorted list of persons
        /// </summary>
        /// <param name="allPersons">Represents list of persons to sort</param>
        /// <param name="sortBy">Name of the property (key), based on which the 
        /// persons should be sorted</param>
        /// <param name="sortOrderOptions">ASCENDING or DESCENDING</param>
        /// <returns>Returns sorted persons as PersonResponse list</returns>
        Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, 
                                        string sortBy, SortOrderOptions sortOrderOptions);
        /// <summary>
        /// Updates the specified person details based on given person ID
        /// </summary>
        /// <param name="personUpdateRequest">Person details to update,
        /// including person id</param>
        /// <returns>Returns the person response object after updation</returns>
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);

        /// <summary>
        /// Deletes the spacified person object based on given person ID
        /// </summary>
        /// <param name="personID"></param>
        /// <returns>returns true if delete is succeed, else returns false</returns>
        Task<bool> DeletePerson(Guid? personID);
        
        /// <summary>
        /// Returns a CSV File off all persons
        /// </summary>
        /// <returns>Returns a memory stream with CSV data</returns>
        Task<MemoryStream> GetPersonsCSV();

        /// <summary>
        /// Returns persons as Excel 
        /// </summary>
        /// <returns>Returns the memory stream with Excel data of persons</returns>
        Task<MemoryStream> GetPersonsExcel();


    }
}
