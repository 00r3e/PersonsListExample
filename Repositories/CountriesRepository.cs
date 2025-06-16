using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repositories
{
    public class CountriesRepository : ICountriesRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public CountriesRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext; 
        }
        public async Task<Country> AddCountry(Country country)
        {
            _dbContext.Countries.Add(country);
            await _dbContext.SaveChangesAsync();
            return country;
        }

        public async Task<List<Country>> GetAllCountries()
        {
            return await _dbContext.Countries.ToListAsync();
        }

        public async Task<Country?> GetCountryById(Guid countryID)
        {
            return await _dbContext.Countries.FirstOrDefaultAsync(temp => temp.CountryID == countryID);
        }

        public async Task<Country?> GetCountryByName(string countryName)
        {
            return await _dbContext.Countries.FirstOrDefaultAsync(temp => temp.CountryName == countryName);

        }
    }
}
