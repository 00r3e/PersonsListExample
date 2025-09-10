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
    public interface IPersonsAdderService
    {
        /// <summary>
        /// Adds a new Person in to an existing list of persons
        /// </summary>
        /// <param name="personAddRequest">Person to add</param>
        /// <returns>Returns the same Person Details,
        /// along with newly generated PersonID</returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);

    }
}
