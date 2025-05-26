using Microsoft.EntityFrameworkCore;
using SignPuddle.API.Models;

namespace SignPuddle.API.Data
{
    public interface ISymbolRepository
    {
        Task<IEnumerable<Symbol>> GetAllAsync();
        Task<Symbol?> GetByKeyAsync(string key);
        Task<IEnumerable<Symbol>> GetByCategoryAsync(string category);
        Task<IEnumerable<Symbol>> GetByGroupAsync(string group);
    }

    public class SymbolRepository : ISymbolRepository
    {
        private readonly ApplicationDbContext _context;

        public SymbolRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Symbol>> GetAllAsync()
        {
            return await _context.Symbols.ToListAsync();
        }

        public async Task<Symbol?> GetByKeyAsync(string key)
        {
            return await _context.Symbols.FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task<IEnumerable<Symbol>> GetByCategoryAsync(string category)
        {
            return await _context.Symbols
                .Where(s => s.Category == category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Symbol>> GetByGroupAsync(string group)
        {
            return await _context.Symbols
                .Where(s => s.Group == group)
                .ToListAsync();
        }
    }
}