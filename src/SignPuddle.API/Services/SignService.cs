using SignPuddle.API.Data;
using SignPuddle.API.Models;

namespace SignPuddle.API.Services
{
    public interface ISignService
    {
        Task<IEnumerable<Sign>> GetAllSignsAsync();
        Task<Sign?> GetSignByIdAsync(int id);
        Task<IEnumerable<Sign>> GetSignsByDictionaryAsync(int dictionaryId);
        Task<IEnumerable<Sign>> SearchSignsByGlossAsync(string searchTerm);
        Task<Sign> CreateSignAsync(Sign sign, string userId);
        Task<Sign?> UpdateSignAsync(Sign sign, string userId);
        Task<bool> DeleteSignAsync(int id, string userId);
    }

    public class SignService : ISignService
    {
        private readonly ISignRepository _signRepository;
        private readonly IUserService _userService;

        public SignService(ISignRepository signRepository, IUserService userService)
        {
            _signRepository = signRepository;
            _userService = userService;
        }

        public async Task<IEnumerable<Sign>> GetAllSignsAsync()
        {
            return await _signRepository.GetAllAsync();
        }

        public async Task<Sign?> GetSignByIdAsync(int id)
        {
            return await _signRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Sign>> GetSignsByDictionaryAsync(int dictionaryId)
        {
            return await _signRepository.GetByDictionaryIdAsync(dictionaryId);
        }

        public async Task<IEnumerable<Sign>> SearchSignsByGlossAsync(string searchTerm)
        {
            return await _signRepository.SearchByGlossAsync(searchTerm);
        }

        public async Task<Sign> CreateSignAsync(Sign sign, string userId)
        {
            sign.CreatedBy = userId;
            sign.UpdatedBy = userId;
            sign.Created = DateTime.UtcNow;
            sign.Updated = DateTime.UtcNow;
            
            return await _signRepository.CreateAsync(sign);
        }

        public async Task<Sign?> UpdateSignAsync(Sign sign, string userId)
        {
            sign.UpdatedBy = userId;
            sign.Updated = DateTime.UtcNow;
            
            return await _signRepository.UpdateAsync(sign);
        }

        public async Task<bool> DeleteSignAsync(int id, string userId)
        {
            // Optional: Add permission check here
            // var user = await _userService.GetUserByIdAsync(userId);
            
            return await _signRepository.DeleteAsync(id);
        }
    }
}