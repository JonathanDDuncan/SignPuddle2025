using SignPuddle.API.Data;
using SignPuddle.API.Models;

namespace SignPuddle.API.Services
{
    public interface ISymbolService
    {
        Task<IEnumerable<Symbol>> GetAllSymbolsAsync();
        Task<Symbol?> GetSymbolByKeyAsync(string key);
        Task<IEnumerable<Symbol>> GetSymbolsByCategoryAsync(string category);
        Task<IEnumerable<Symbol>> GetSymbolsByGroupAsync(string group);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
        Task<IEnumerable<string>> GetGroupsByCategoryAsync(string category);
    }

    public class SymbolService : ISymbolService
    {
        private readonly ISymbolRepository _symbolRepository;

        public SymbolService(ISymbolRepository symbolRepository)
        {
            _symbolRepository = symbolRepository;
        }

        public async Task<IEnumerable<Symbol>> GetAllSymbolsAsync()
        {
            return await _symbolRepository.GetAllAsync();
        }

        public async Task<Symbol?> GetSymbolByKeyAsync(string key)
        {
            return await _symbolRepository.GetByKeyAsync(key);
        }

        public async Task<IEnumerable<Symbol>> GetSymbolsByCategoryAsync(string category)
        {
            return await _symbolRepository.GetByCategoryAsync(category);
        }

        public async Task<IEnumerable<Symbol>> GetSymbolsByGroupAsync(string group)
        {
            return await _symbolRepository.GetByGroupAsync(group);
        }

        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            var symbols = await _symbolRepository.GetAllAsync();
            return symbols.Select(s => s.Category).Distinct();
        }

        public async Task<IEnumerable<string>> GetGroupsByCategoryAsync(string category)
        {
            var symbols = await _symbolRepository.GetByCategoryAsync(category);
            return symbols.Select(s => s.Group).Distinct();
        }
    }
}