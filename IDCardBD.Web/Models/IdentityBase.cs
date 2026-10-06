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

        public PrintStatus PrintStatus { get; set; } = PrintStatus.None;

        [Display(Name = "NID / Birth Cert No")]
        [StringLength(30)]
        public string? IdNumber { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [Display(Name = "Issued On")]
        [DataType(DataType.Date)]
        public DateTime? IssuedOn { get; set; }

        [Display(Name = "Valid Until")]
        [DataType(DataType.Date)]
        public DateTime? ValidUntil { get; set; }

        public string? Address { get; set; }
    }
}
