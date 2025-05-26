using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Models;

namespace SignPuddle.API.Data
{
    public interface IDictionaryRepository
    {
        Task<IEnumerable<Dictionary>> GetAllAsync();
        Task<Dictionary?> GetByIdAsync(int id);
        Task<IEnumerable<Dictionary>> GetByLanguageAsync(string language);
        Task<IEnumerable<Dictionary>> GetByOwnerAsync(string ownerId);
        Task<Dictionary> CreateAsync(Dictionary dictionary);
        Task<Dictionary?> UpdateAsync(Dictionary dictionary);
        Task<bool> DeleteAsync(int id);
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

        public async Task<Dictionary?> GetByIdAsync(int id)
        {
            return await _context.Dictionaries.FindAsync(id);
        }

        public async Task<IEnumerable<Dictionary>> GetByLanguageAsync(string language)
        {
            return await _context.Dictionaries
                .Where(d => d.Language == language)
                .ToListAsync();
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

        public async Task<bool> DeleteAsync(int id)
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
    }
}