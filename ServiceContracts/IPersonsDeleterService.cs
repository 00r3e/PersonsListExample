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
    public interface IPersonsDeleterService
    {
        /// <summary>
        /// Deletes the spacified person object based on given person ID
        /// </summary>
        /// <param name="personID"></param>
        /// <returns>returns true if delete is succeed, else returns false</returns>
        Task<bool> DeletePerson(Guid? personID);
    }
}
