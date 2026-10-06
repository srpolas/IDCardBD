using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IDCardBD.Web.Models
{
    public class CardTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Institute Name")]
        [StringLength(100)]
        public string? InstituteName { get; set; }

        [Required]
        public string FrontBgPath { get; set; } = string.Empty;

        [Required]
        public string BackBgPath { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        [StringLength(10)]
        public string Orientation { get; set; } = "Landscape"; // Landscape | Portrait

        /// <summary>JSON array of CardElement describing the front layout (drag-and-drop designer).</summary>
        public string? FrontElementsJson { get; set; }

        /// <summary>JSON array of CardElement describing the back layout.</summary>
        public string? BackElementsJson { get; set; }
    }

    /// <summary>
    /// One positioned item on a card side. Coordinates are percentages of the card area
    /// (0-100) so layouts scale to any render size. Persisted as JSON in
    /// CardTemplate.FrontElementsJson / BackElementsJson.
    /// </summary>
    public class CardElement
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text"; // text | photo | qr

        /// <summary>Data field key (see PdfService field map) or literal text when Field = "custom".</summary>
        [JsonPropertyName("field")]
        public string? Field { get; set; }

        /// <summary>Literal text used when Field = "custom" (e.g. a label like "Roll:").</summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("x")]
        public double X { get; set; }

        [JsonPropertyName("y")]
        public double Y { get; set; }

        /// <summary>Width in % of card width (photo/qr height derives from width for qr).</summary>
        [JsonPropertyName("w")]
        public double W { get; set; }

        /// <summary>Height in % of card height (photo only).</summary>
        [JsonPropertyName("h")]
        public double H { get; set; }

        /// <summary>Font size in PDF points (text elements).</summary>
        [JsonPropertyName("fontSize")]
        public double FontSize { get; set; } = 8;

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#000000";

        [JsonPropertyName("bold")]
        public bool Bold { get; set; }

        /// <summary>left | center | right</summary>
        [JsonPropertyName("align")]
        public string Align { get; set; } = "left";

        /// <summary>Show this element on the printed card (toggle without deleting).</summary>
        [JsonPropertyName("visible")]
        public bool Visible { get; set; } = true;

        /// <summary>Font family name (text elements). Null/empty = Lato.</summary>
        [JsonPropertyName("fontFamily")]
        public string? FontFamily { get; set; }

        /// <summary>Rotation in degrees, clockwise, around the element's center (text elements).</summary>
        [JsonPropertyName("rotation")]
        public double Rotation { get; set; }
    }
}
