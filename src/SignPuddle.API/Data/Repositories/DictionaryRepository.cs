using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Models;
using SignPuddle.API.Data.Repositories;

namespace SignPuddle.API.Data
{
    public interface IDictionaryRepository
    {
        Task<IEnumerable<Dictionary>> GetAllAsync();
        Task<Dictionary?> GetByIdAsync(string id);
        Task<IEnumerable<Dictionary>> GetByOwnerAsync(string ownerId);
        Task<Dictionary> CreateAsync(Dictionary dictionary);
        Task<Dictionary?> UpdateAsync(Dictionary dictionary);
        Task<bool> DeleteAsync(string id);
        IQueryable<Dictionary> BuildSearchQuery(DictionarySearchParameters parameters);
        Task<int> CountSearchResultsAsync(IQueryable<Dictionary> query);
        Task<List<Dictionary>> ExecuteSearchQueryAsync(IQueryable<Dictionary> query, int? page = null, int? pageSize = null);
    }

    public class DictionaryRepository : IDictionaryRepository
    {
        private readonly ApplicationDbContext _context;

        public DictionaryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Dictionary>> GetAllAsync()
        {
            return await _context.Dictionaries.ToListAsync();
        }

        public async Task<Dictionary?> GetByIdAsync(string id)
        {
            return await _context.Dictionaries.FindAsync(id);
        }

        public async Task<IEnumerable<Dictionary>> GetByOwnerAsync(string ownerId)
        {
            return await _context.Dictionaries
                .Where(d => d.OwnerId == ownerId)
                .ToListAsync();
        }

        public async Task<Dictionary> CreateAsync(Dictionary dictionary)
        {
            _context.Dictionaries.Add(dictionary);
            await _context.SaveChangesAsync();
            return dictionary;
        }

        public async Task<Dictionary?> UpdateAsync(Dictionary dictionary)
        {
            var existingDictionary = await _context.Dictionaries.FindAsync(dictionary.Id);
            if (existingDictionary == null)
            {
                return null;
            }

            _context.Entry(existingDictionary).CurrentValues.SetValues(dictionary);
            existingDictionary.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingDictionary;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var dictionary = await _context.Dictionaries.FindAsync(id);
            if (dictionary == null)
            {
                return false;
            }

            _context.Dictionaries.Remove(dictionary);
            await _context.SaveChangesAsync();
            return true;
        }

        public IQueryable<Dictionary> BuildSearchQuery(DictionarySearchParameters parameters)
        {
            var query = _context.Dictionaries.AsQueryable();
            if (!string.IsNullOrWhiteSpace(parameters.Query))
                query = query.Where(d => d.Name.Contains(parameters.Query) || (d.Description != null && d.Description.Contains(parameters.Query)));
            if (!string.IsNullOrWhiteSpace(parameters.OwnerId))
                query = query.Where(d => d.OwnerId == parameters.OwnerId);
            if (parameters.IsPublic.HasValue)
                query = query.Where(d => d.IsPublic == parameters.IsPublic.Value);
            return query;
        }

        public async Task<List<Dictionary>> ExecuteSearchQueryAsync(IQueryable<Dictionary> query, int? page = null, int? pageSize = null)
        {
            if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            return await query.ToListAsync();
        }

        public async Task<int> CountSearchResultsAsync(IQueryable<Dictionary> query)
        {
            return await query.CountAsync();
        }

        public static DictionaryDto MapToDto(Dictionary entity)
        {
            return new DictionaryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsPublic = entity.IsPublic,
                OwnerId = entity.OwnerId
            };
        }
    }
}