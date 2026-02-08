using System.ComponentModel.DataAnnotations;

namespace IDCardBD.Web.Models
{
    public class Teacher : IdentityBase
    {
        [Required]
        [StringLength(50)]
        public string TeacherCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Designation { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Emergency Contact")]
        public string EmergencyContact { get; set; } = string.Empty;

        [StringLength(5)]
        public string? BloodGroup { get; set; }
    }
}
