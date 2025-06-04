using System.Xml.Serialization;

namespace SignPuddle.API.Models
{
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

        [XmlAttribute("cdt")]
        public long CreatedTimestamp { get; set; }

        [XmlAttribute("mdt")]
        public long ModifiedTimestamp { get; set; }

        [XmlAttribute("nextid")]
        public int NextId { get; set; }

        // DTD Elements in order: (term*,text*,png?,svg?,src*,entry*)
        [XmlElement("term")]
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

        // Helper properties for backward compatibility
        public string? DictionaryName => Terms.FirstOrDefault();
        public string? Text => TextElements.FirstOrDefault();
        public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(CreatedTimestamp).UtcDateTime;
        public DateTime Modified => DateTimeOffset.FromUnixTimeSeconds(ModifiedTimestamp).UtcDateTime;
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

        [XmlAttribute("cdt")]
        public long CreatedTimestamp { get; set; }

        [XmlAttribute("mdt")]
        public long ModifiedTimestamp { get; set; }

        [XmlAttribute("usr")]
        public string User { get; set; } = string.Empty;

        // DTD Elements in order: (term*,text*,png?,svg?,video?,src*)
        [XmlElement("term")]
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
        public DateTime Created => DateTimeOffset.FromUnixTimeSeconds(CreatedTimestamp).UtcDateTime;
        public DateTime Modified => DateTimeOffset.FromUnixTimeSeconds(ModifiedTimestamp).UtcDateTime;

        // Extract FSW notation (first term starting with "AS" or "M")
        public string? FswNotation => Terms.FirstOrDefault(t => t.StartsWith("AS") || t.StartsWith("M"));

        // Extract gloss (first non-FSW term)
        public string? Gloss => Terms.FirstOrDefault(t => !t.StartsWith("AS") && !t.StartsWith("M"));

        // Backward compatibility properties
        public string? Text => TextElements.FirstOrDefault();
        public string? Source => Sources.FirstOrDefault();
    }
}