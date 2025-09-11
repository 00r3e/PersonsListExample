using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Country entity
    /// </summary>

    public interface ICountriesGetterService
    {
        /// <summary>
        /// Returns all countries from the list
        /// </summary>
        /// <returns>Returs all countries from the list as List of CountryResponse</returns>
        Task<List<CountryResponse>> GetAllCountries();


        /// <summary>
        /// Returns country object by countryID
        /// </summary>
        /// <param name="countryID"></param>
        /// <returns>Returns country object by Guid countryID</returns>
        Task<CountryResponse?> GetCountryByCountryID(Guid? countryID);
    }
}
