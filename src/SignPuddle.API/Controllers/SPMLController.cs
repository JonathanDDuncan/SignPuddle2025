using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SignPuddle.API.Data;
using SignPuddle.API.Models;
using SignPuddle.API.Services;

namespace SignPuddle.API.Controllers
{
    [ApiController]
    [Route("api/spml")]
    public class SPMLController : ControllerBase
    {
        private readonly ISpmlImportService _spmlImportService;
        private readonly IDictionaryRepository _dictionaryRepository;
        private readonly ISignService _signService;

        public SPMLController(ISpmlImportService spmlImportService, IDictionaryRepository dictionaryRepository, ISignService signService)
        {
            _spmlImportService = spmlImportService;
            _dictionaryRepository = dictionaryRepository;
            _signService = signService;
        }

        /// <summary>
        /// Import a dictionary from an SPML file
        /// </summary>
        /// <param name="file">The SPML file to import</param>
        /// <returns>Import result with dictionary and signs data</returns>
        [HttpPost("import")]
        public async Task<ActionResult<SpmlImportResult>> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            if (!file.FileName.EndsWith(".spml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File must be an SPML file");
            }

            try
            {
                // Read file content
                using var reader = new StreamReader(file.OpenReadStream());
                var xmlContent = await reader.ReadToEndAsync();

                // Parse SPML
                var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

                var existingDictionary = await LoadExistingDictionary(spmlDocument);

                Dictionary savedDictionary;
                List<SignPuddle.API.Models.Sign> updatedSigns;
                string ownerId = "signpuddle-import";
                if (existingDictionary != null)
                {
                    var (signsToAdd, signsToUpdate) = await MergeInSPML(existingDictionary, spmlDocument);
                    // Update existing dictionary

                    foreach (var sign in signsToAdd)
                    {
                        await _signService.CreateSignAsync(sign,ownerId);
                    }

                    foreach (var sign in signsToUpdate)
                    {
                        await _signService.UpdateSignAsync(sign);
                    }
                    updatedSigns = signsToAdd.Concat(signsToUpdate).ToList();
                    savedDictionary = existingDictionary;
                }
                else
                {
                    // Convert to dictionary
                    var dictionary = await _spmlImportService.ConvertToDictionaryAsync(spmlDocument, ownerId);

                    // Save dictionary to DB
                    savedDictionary = await _dictionaryRepository.CreateAsync(dictionary);
                    // Convert signs with the real dictionary ID
                    var signs = await _spmlImportService.ConvertToSignsAsync(spmlDocument, savedDictionary.Id);
                    updatedSigns = signs;
                    // Save signs to DB
                    foreach (var sign in signs)
                    {
                        await _signService.CreateSignAsync(sign, ownerId);
                    }
                }
                var result = new SpmlImportResult
                {
                    Dictionary = savedDictionary,
                    UpdatedSigns = updatedSigns,
                    OriginalPuddleId = spmlDocument.PuddleId,
                    ImportedAt = DateTime.UtcNow,
                    TotalEntries = spmlDocument.Entries.Count
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error importing SPML file: {ex.Message}");
            }
        }

        private async Task<Dictionary?> LoadExistingDictionary(SpmlDocument spmlDocument)
        {
            // Check if dictionary already exists if so load it, else create a new one
            var existingDictionaries = await _dictionaryRepository.GetAllAsync();
            var existingDictionary = existingDictionaries
                .FirstOrDefault(d => d.PuddleType == spmlDocument.Type && d.PuddleId == spmlDocument.PuddleId.ToString());
            return existingDictionary;
        }

        /// <summary>
        /// Preview an SPML file without importing it
        /// </summary>
        /// <param name="file">The SPML file to preview</param>
        /// <returns>Preview information about the SPML file</returns>
        [HttpPost("import/preview")]
        public async Task<ActionResult<SpmlPreview>> Preview(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file provided");
            }

            if (!file.FileName.EndsWith(".spml", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("File must be an SPML file");
            }

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                var xmlContent = await reader.ReadToEndAsync();

                var spmlDocument = await _spmlImportService.ParseSpmlAsync(xmlContent);

                var preview = new SpmlPreview
                {
                    DictionaryName = spmlDocument.DictionaryName,
                    Type = spmlDocument.Type,
                    PuddleId = spmlDocument.PuddleId,
                    Created = spmlDocument.Created,
                    Modified = spmlDocument.Modified,
                    TotalEntries = spmlDocument.Entries.Count,
                    EntriesWithGloss = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Gloss)),
                    EntriesWithText = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Text)),
                    EntriesWithVideo = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Video)),
                    EntriesWithSource = spmlDocument.Entries.Count(e => !string.IsNullOrEmpty(e.Source)),
                    SampleEntries = spmlDocument.Entries.Take(5).Select(e => new SpmlEntryPreview
                    {
                        Id = e.EntryId ?? 0,
                        FswNotation = e.Fsw,
                        Gloss = e.Gloss,
                        User = e.User,
                        Created = e.Created
                    }).ToList()
                };

                return Ok(preview);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error previewing SPML file: {ex.Message}");
            }
        }

        /// <summary>
        /// Merges SPML entries into the existing dictionary's signs by matching PuddleId and PuddleSignId.
        /// Returns (signsToAdd, signsToUpdate).
        /// </summary>
        private async Task<(List<SignPuddle.API.Models.Sign> signsToAdd,
        List<SignPuddle.API.Models.Sign> signsToUpdate)>
        MergeInSPML(SignPuddle.API.Models.Dictionary existingDictionary, SpmlDocument spmlDocument)
        {
            // Get all existing signs for this dictionary
            var existingSigns = await _signService.GetSignsByDictionaryAsync(existingDictionary.Id);
            var existingSignsDict = existingSigns.ToDictionary(s => s.PuddleSignId);

            var signsToAdd = new List<SignPuddle.API.Models.Sign>();
            var signsToUpdate = new List<SignPuddle.API.Models.Sign>();

            foreach (var entry in spmlDocument.Entries)
            {
                if (!entry.EntryId.HasValue || string.IsNullOrWhiteSpace(entry.Fsw) || string.IsNullOrWhiteSpace(entry.Gloss))
                    continue;
                var puddleSignId = entry.EntryId.Value;
                if (existingSignsDict.TryGetValue(puddleSignId, out var existingSign))
                {
                    // Update existing sign if any field has changed
                    bool needsUpdate = false;
                    if (existingSign.Fsw != entry.Fsw) needsUpdate = true;
                    if (existingSign.Gloss != entry.Gloss) needsUpdate = true;
                    if (existingSign.SgmlText != entry.Text) needsUpdate = true;
                    if (needsUpdate)
                    {
                        existingSign.Fsw = entry.Fsw;
                        existingSign.Gloss = entry.Gloss;
                        existingSign.SgmlText = entry.Text;
                        existingSign.Updated = DateTime.UtcNow;
                        existingSign.UpdatedBy = entry.User;
                        signsToUpdate.Add(existingSign);
                    }
                }
                else
                {
                    // Add new sign
                    var newSign = new SignPuddle.API.Models.Sign
                    {
                        DictionaryId = existingDictionary.Id,
                        PuddleSignId = puddleSignId,
                        PuddleId = spmlDocument.PuddleId.ToString(),
                        Fsw = entry.Fsw ?? string.Empty,
                        Gloss = entry.Gloss,
                        SgmlText = entry.Text,
                        Created = entry.Created,
                        Updated = entry.Modified,
                        CreatedBy = entry.User,
                        UpdatedBy = entry.User
                    };
                    signsToAdd.Add(newSign);
                }
            }
            return (signsToAdd, signsToUpdate);
        }
    }

    public class SpmlImportResult
    {
        public Dictionary Dictionary { get; set; } = new Dictionary();
        public List<SignPuddle.API.Models.Sign> UpdatedSigns { get; set; } = new List<SignPuddle.API.Models.Sign>();
        public int OriginalPuddleId { get; set; }
        public DateTime ImportedAt { get; set; }
        public int TotalEntries { get; set; }
    }

    public class SpmlPreview
    {
        public string? DictionaryName { get; set; }
        public string Type { get; set; } = string.Empty;
        public int PuddleId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int TotalEntries { get; set; }
        public int EntriesWithGloss { get; set; }
        public int EntriesWithText { get; set; }
        public int EntriesWithVideo { get; set; }
        public int EntriesWithSource { get; set; }
        public List<SpmlEntryPreview> SampleEntries { get; set; } = new List<SpmlEntryPreview>();
    }

    public class SpmlEntryPreview
    {
        public int Id { get; set; }
        public string? FswNotation { get; set; }
        public string? Gloss { get; set; }
        public string User { get; set; } = string.Empty;
        public DateTime Created { get; set; }
    }
}
