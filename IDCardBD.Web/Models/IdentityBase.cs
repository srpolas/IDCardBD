using System.ComponentModel.DataAnnotations;

namespace IDCardBD.Web.Models
{
    public abstract class IdentityBase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string? PhotoPath { get; set; }

        public string? QRCode { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public UserCategory Category { get; set; }

        public bool IsPrinted { get; set; }
    }
}
