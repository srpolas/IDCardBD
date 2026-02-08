using System.ComponentModel.DataAnnotations;

namespace IDCardBD.Web.Models
{
    public class Student : IdentityBase
    {
        [Required]
        [StringLength(10)]
        public string RollNumber { get; set; } = string.Empty;

        public string FathersName { get; set; } = string.Empty;
        public string MothersName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Grade { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Section { get; set; }

        [Required]
        [StringLength(100)]
        public string GuardianName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? FatherName { get; set; }

        [StringLength(100)]
        public string? MotherName { get; set; }

        public string? Address { get; set; }

        [StringLength(5)]
        public string? BloodGroup { get; set; }
    }
}
