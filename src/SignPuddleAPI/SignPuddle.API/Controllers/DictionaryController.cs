using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SignPuddle.API.Data;
using SignPuddle.API.Models;
using SignPuddle.API.Data.Repositories;
using System.Security.Claims;

namespace SignPuddle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DictionaryController : SignPuddleBaseController
    {
        private readonly IDictionaryRepository _dictionaryRepository;

        public DictionaryController(IDictionaryRepository dictionaryRepository)
        {
            _dictionaryRepository = dictionaryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDictionaries()
        {
            var dictionaries = await _dictionaryRepository.GetAllAsync();
            return Ok(dictionaries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDictionaryById(string id)
        {
            var dictionary = await _dictionaryRepository.GetByIdAsync(id);
            if (dictionary == null)
            {
                return NotFound();
            }
            return Ok(dictionary);
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyDictionaries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var dictionaries = await _dictionaryRepository.GetByOwnerAsync(userId);
            return Ok(dictionaries);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateDictionary([FromBody] Dictionary dictionary)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            dictionary.OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            dictionary.Created = DateTime.UtcNow;
            dictionary.Updated = DateTime.UtcNow;
            
            var createdDictionary = await _dictionaryRepository.CreateAsync(dictionary);
            return CreatedAtAction(nameof(GetDictionaryById), new { id = createdDictionary.Id }, createdDictionary);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDictionary(string dictionaryId, [FromBody] Dictionary dictionary)
        {
            if (dictionaryId != dictionary.Id)
            {
                return BadRequest("ID mismatch");
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var existingDictionary = await _dictionaryRepository.GetByIdAsync(dictionaryId);
            if (existingDictionary == null)
            {
                return NotFound();
            }
            
            // Verify ownership
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (existingDictionary.OwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            
            // Preserve the original owner
            dictionary.OwnerId = existingDictionary.OwnerId;
            dictionary.Created = existingDictionary.Created;
            dictionary.Updated = DateTime.UtcNow;
            
            var updatedDictionary = await _dictionaryRepository.UpdateAsync(dictionary);
            return Ok(updatedDictionary);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDictionary(string id)
        {
            var dictionary = await _dictionaryRepository.GetByIdAsync(id);
            if (dictionary == null)
            {
                return NotFound();
            }
            
            // Verify ownership
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (dictionary.OwnerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }
            
            var result = await _dictionaryRepository.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] DictionarySearchParameters parameters)
        {
            parameters.Validate();
            var baseQuery = _dictionaryRepository.BuildSearchQuery(parameters);
            var totalCount = await _dictionaryRepository.CountSearchResultsAsync(baseQuery);
            var items = await _dictionaryRepository.ExecuteSearchQueryAsync(baseQuery, parameters.Page, parameters.PageSize);
            var dtos = items.Select(DictionaryRepository.MapToDto);
            return Ok(new { totalCount, items = dtos, page = parameters.Page, pageSize = parameters.PageSize });
        }
    }
}