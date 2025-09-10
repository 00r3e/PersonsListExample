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
    public interface IPersonsGetterService
    {

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
