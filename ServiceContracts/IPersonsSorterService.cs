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
    public interface IPersonsSorterService
    {
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
    }
}
