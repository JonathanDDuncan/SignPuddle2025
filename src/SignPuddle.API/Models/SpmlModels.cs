using System.Xml.Serialization;
using System.Text.Json.Serialization;

namespace SignPuddle.API.Models
{
    public class SpmlMeta
    {
        // Meta attributes
        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("value")]
        public string? Value { get; set; }

        [XmlAttribute("lang")]
        public string? Language { get; set; }

        // Title property for backward compatibility
        [XmlIgnore]
        public string? Title => Name?.ToLower() == "title" ? Value : null;
    }

    /// <summary>
    /// SPML Document root element - fully compliant with spml_1.6.dtd
    /// DTD: <!ELEMENT spml (term*,text*,png?,svg?,src*,entry*)>
    /// </summary>
    [XmlRoot("spml")]
    public class SpmlDocument
    {
        // DTD Attributes: root, type, puddle, uuid, cdt, mdt, nextid (all IMPLIED)
        [XmlAttribute("root")]
        public string? Root { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; } = string.Empty;

        [XmlAttribute("puddle")]
        public int PuddleId { get; set; }

        [XmlAttribute("uuid")]
        public string? Uuid { get; set; }

        [XmlIgnore]
        public long? CreatedTimestamp { get; set; }

        [XmlAttribute("cdt")]
        public string? CreatedTimestampString
        {
            get => CreatedTimestamp?.ToString();
            set => CreatedTimestamp = long.TryParse(value, out var v) ? v : (long?)null;
        }

        [XmlIgnore]
        public long? ModifiedTimestamp { get; set; }

        [XmlAttribute("mdt")]
        public string? ModifiedTimestampString
        {
            get => ModifiedTimestamp?.ToString();
            set => ModifiedTimestamp = long.TryParse(value, out var v) ? v : (long?)null;
        }

        [XmlAttribute("nextid")]
        public int NextId { get; set; }

        // DTD Elements in order: (term*,text*,png?,svg?,src*,entry*)
        [XmlElement("term")]
        [JsonPropertyName("terms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Terms { get; set; } = new List<string>();

        [XmlElement("text")]
        public List<string> TextElements { get; set; } = new List<string>();

        [XmlElement("png")]
        public string? PngData { get; set; }

        [XmlElement("svg")]
        public string? SvgData { get; set; }

        [XmlElement("src")]
        public List<string> Sources { get; set; } = new List<string>();

        [XmlElement("entry")]
        public List<SpmlEntry> Entries { get; set; } = new List<SpmlEntry>();

        [XmlElement("meta")]
        public List<SpmlMeta> Meta { get; set; } = new List<SpmlMeta>();

        // Helper properties for backward compatibility
        [XmlIgnore]
        public string? DictionaryName
        {
            get
            {
                var name = Meta?.FirstOrDefault(m => string.Equals(m.Name, "title", StringComparison.OrdinalIgnoreCase))?.Value;
                if (!string.IsNullOrWhiteSpace(name))
                    return name;
                var term = Terms?.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(term))
                    return term;
                // If Terms is empty but TextElements has a value, use that (for tests expecting 'Empty Dictionary' or similar)
                var text = TextElements?.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;
                return "Unknown";
            }
        }
        public string? Text => TextElements.FirstOrDefault();
        public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(CreatedTimestamp ?? 0).UtcDateTime;
        public DateTime Modified => DateTimeOffset.FromUnixTimeSeconds(ModifiedTimestamp ?? 0).UtcDateTime;
    }

    /// <summary>
    /// SPML Entry element - fully compliant with spml_1.6.dtd
    /// DTD: <!ELEMENT entry (term*,text*,png?,svg?,video?,src*)>
    /// </summary>
    public class SpmlEntry
    {
        // DTD Attributes: id, uuid, prev, next, cdt, mdt, usr (all IMPLIED)
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("uuid")]
        public string? Uuid { get; set; }

        [XmlAttribute("prev")]
        public string? Previous { get; set; }

        [XmlAttribute("next")]
        public string? Next { get; set; }

        [XmlIgnore]
        public long? CreatedTimestamp { get; set; }

        [XmlAttribute("cdt")]
        public string? CreatedTimestampString
        {
            get => CreatedTimestamp?.ToString();
            set => CreatedTimestamp = long.TryParse(value, out var v) ? v : (long?)null;
        }

        [XmlIgnore]
        public long? ModifiedTimestamp { get; set; }

        [XmlAttribute("mdt")]
        public string? ModifiedTimestampString
        {
            get => ModifiedTimestamp?.ToString();
            set => ModifiedTimestamp = long.TryParse(value, out var v) ? v : (long?)null;
        }

        [XmlAttribute("usr")]
        public string User { get; set; } = string.Empty;

        // DTD Elements in order: (term*,text*,png?,svg?,video?,src*)
        [XmlElement("term")]
        [JsonPropertyName("terms")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Terms { get; set; } = new List<string>();

        [XmlElement("text")]
        public List<string> TextElements { get; set; } = new List<string>();

        [XmlElement("png")]
        public string? PngData { get; set; }

        [XmlElement("svg")]
        public string? SvgData { get; set; }

        [XmlElement("video")]
        public string? Video { get; set; }

        [XmlElement("src")]
        public List<string> Sources { get; set; } = new List<string>();

        // Helper properties for backward compatibility
        public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(CreatedTimestamp ?? 0).UtcDateTime;
        public DateTime Modified => DateTimeOffset.FromUnixTimeSeconds(ModifiedTimestamp ?? 0).UtcDateTime;

        // Extract FSW notation (first term starting with "AS" or "M")
        public string? FswNotation => Terms.FirstOrDefault(t => t.StartsWith("AS") || t.StartsWith("M"));

        // Extract gloss (first non-FSW term)
        public string? Gloss => Terms.FirstOrDefault(t => !t.StartsWith("AS") && !t.StartsWith("M"));

        // Backward compatibility properties
        public string? Text => TextElements.FirstOrDefault();
        public string? Source => Sources.FirstOrDefault();
    }

    public class SpmlPuddleInfo
    {
        // Puddle attributes
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("name")]
        public string? Name { get; set; }

        [XmlAttribute("type")]
        public string? Type { get; set; }

        [XmlAttribute("lang")]
        public string? Language { get; set; }

        [XmlAttribute("owner")]
        public string? Owner { get; set; }

        [XmlIgnore]
        public long? CreatedTimestamp { get; set; }

        [XmlAttribute("cdt")]
        public string? CreatedTimestampString
        {
            get => CreatedTimestamp?.ToString();
            set => CreatedTimestamp = long.TryParse(value, out var v) ? v : (long?)null;
        }

        [XmlIgnore]
        public long? ModifiedTimestamp { get; set; }

        [XmlAttribute("mdt")]
        public string? ModifiedTimestampString
        {
            get => ModifiedTimestamp?.ToString();
            set => ModifiedTimestamp = long.TryParse(value, out var v) ? v : (long?)null;
        }

        // Helper properties
        public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(CreatedTimestamp ?? 0).UtcDateTime;
        public DateTime Modified => DateTimeOffset.FromUnixTimeSeconds(ModifiedTimestamp ?? 0).UtcDateTime;
    }
}