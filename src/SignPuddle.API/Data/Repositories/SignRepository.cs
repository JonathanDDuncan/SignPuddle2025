using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Models;

namespace SignPuddle.API.Data
{
    public interface ISignRepository
    {
        Task<IEnumerable<Sign>> GetAllAsync();
        Task<Sign?> GetByIdAsync(int id);
        Task<IEnumerable<Sign>> GetByDictionaryIdAsync(int dictionaryId);
        Task<IEnumerable<Sign>> SearchByGlossAsync(string searchTerm);
        Task<Sign> CreateAsync(Sign sign);
        Task<Sign?> UpdateAsync(Sign sign);
        Task<bool> DeleteAsync(int id);
    }

    public class SignRepository : ISignRepository
    {
        private readonly ApplicationDbContext _context;

        public SignRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sign>> GetAllAsync()
        {
            return await _context.Signs.Include(s => s.Dictionary).ToListAsync();
        }

        public async Task<Sign?> GetByIdAsync(int id)
        {
            return await _context.Signs
                .Include(s => s.Dictionary)
                .FirstOrDefaultAsync(s => s.PuddleSignId == id);
        }

        public async Task<IEnumerable<Sign>> GetByDictionaryIdAsync(int dictionaryId)
        {
            return await _context.Signs
                .Where(s => s.DictionaryId == dictionaryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Sign>> SearchByGlossAsync(string searchTerm)
        {
            return await _context.Signs
                .Where(s => s.Gloss != null && s.Gloss.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<Sign> CreateAsync(Sign sign)
        {
            _context.Signs.Add(sign);
            await _context.SaveChangesAsync();
            return sign;
        }

        public async Task<Sign?> UpdateAsync(Sign sign)
        {
            var existingSign = await _context.Signs.FindAsync(sign.PuddleSignId);
            if (existingSign == null)
            {
                return null;
            }

            _context.Entry(existingSign).CurrentValues.SetValues(sign);
            existingSign.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingSign;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var sign = await _context.Signs.FindAsync(id);
            if (sign == null)
            {
                return false;
            }

            _context.Signs.Remove(sign);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}