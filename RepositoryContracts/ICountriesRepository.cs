using Entities;

namespace RepositoryContracts
{
    /// <summary>
    /// Interface to the data access logic for the Country entity
    /// </summary>
    public interface ICountriesRepository
    {
        /// <summary>
        /// Adds a new Country to the data store
        /// </summary>
        /// <param name="country">Country to add</param>
        /// <returns>Returns the Country after adding it to the data store</returns>
        Task<Country> AddCountry(Country country);

        /// <summary>
        /// Returns all countries in the data store
        /// </summary>
        /// <returns>List of all countries in the data store</returns>
        Task<List<Country>> GetAllCountries();

        /// <summary>
        /// Searching for a country in data store, based on a given id
        /// </summary>
        /// <param name="countryID">the Country id (Guid to search)</param>
        /// <returns>If there is a match, returns the matching Country, otherwise returns null</returns>
        Task<Country?> GetCountryById(Guid countryID);

        /// <summary>
        /// Returns a Country based on a given name
        /// </summary>
        /// <param name="countryName">Country name to search</param>
        /// <returns>If there is a match, returns the matching Country, otherwise returns null</returns>
        Task<Country?> GetCountryByName(string countryName);
    }
}
