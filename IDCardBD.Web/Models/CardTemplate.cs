using System.ComponentModel.DataAnnotations;

namespace IDCardBD.Web.Models
{
    public class CardTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string FrontBgPath { get; set; } = string.Empty;

        [Required]
        public string BackBgPath { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
