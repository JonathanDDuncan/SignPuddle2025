using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Models;
using SignPuddle.API.Data.Repositories;

namespace SignPuddle.API.Data
{
    public interface ISignRepository
    {
        Task<IEnumerable<Sign>> GetAllAsync();
        Task<Sign?> GetByIdAsync(string id);
        Task<IEnumerable<Sign>> GetByDictionaryIdAsync(string dictionaryId);
        Task<IEnumerable<Sign>> SearchByGlossAsync(string searchTerm);
        Task<Sign> CreateAsync(Sign sign);
        Task<Sign?> UpdateAsync(Sign sign);
        Task<bool> DeleteAsync(string id);
        IQueryable<Sign> BuildSearchQuery(SignSearchParameters parameters);
        Task<List<Sign>> ExecuteSearchQueryAsync(IQueryable<Sign> query, int? page = null, int? pageSize = null);
        Task<int> CountSearchResultsAsync(IQueryable<Sign> query);
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

        public async Task<Sign?> GetByIdAsync(string id)
        {
            return await _context.Signs
                .Include(s => s.Dictionary)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Sign>> GetByDictionaryIdAsync(string dictionaryId)
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



        public async Task<Sign?> UpdateAsync(Sign sign)
        {
            var existingSign = await _context.Signs.FindAsync(sign.Id);
            if (existingSign == null)
            {
                return null;
            }

            _context.Entry(existingSign).CurrentValues.SetValues(sign);
            existingSign.Updated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingSign;
        }

        public async Task<bool> DeleteAsync(string id)
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

        public IQueryable<Sign> BuildSearchQuery(SignSearchParameters parameters)
        {
            var query = _context.Signs.AsQueryable();
            if (!string.IsNullOrWhiteSpace(parameters.Gloss))
                query = query.Where(s => s.Gloss != null && s.Gloss.Contains(parameters.Gloss));
            if (!string.IsNullOrWhiteSpace(parameters.DictionaryId))
                query = query.Where(s => s.DictionaryId == parameters.DictionaryId);
            if (!string.IsNullOrWhiteSpace(parameters.Fsw))
                query = query.Where(s => s.Fsw != null && s.Fsw.Contains(parameters.Fsw));
            return query;
        }

        public async Task<List<Sign>> ExecuteSearchQueryAsync(IQueryable<Sign> query, int? page = null, int? pageSize = null)
        {
            if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
                query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
            return await query.ToListAsync();
        }

        public async Task<int> CountSearchResultsAsync(IQueryable<Sign> query)
        {
            return await query.CountAsync();
        }

        public static SignDto MapToDto(Sign entity)
        {
            return new SignDto
            {
                Id = entity.Id,
                PuddleSignId = entity.PuddleSignId,
                Gloss = entity.Gloss,
                DictionaryId = entity.DictionaryId,
                Fsw = entity.Fsw
            };
        }

        public async Task<IEnumerable<Sign>> GetSignsByDictionaryAsync(Guid dictionaryId)
        {
            var entities = await _context.Signs
                .Where(s => s.PuddleId == dictionaryId.ToString())
                .ToListAsync();
            return entities;
        }

        public async Task<Sign> CreateAsync(Sign sign)
        {
            var entity = new Sign
            {
                Id = sign.Id,
                PuddleSignId = sign.PuddleSignId,
                Gloss = sign.Gloss,
                DictionaryId = sign.DictionaryId,
                Fsw = sign.Fsw,
                PuddleId = sign.PuddleId,
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow
            };
            _context.Signs.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}