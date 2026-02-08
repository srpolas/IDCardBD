using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IDCardBD.Web.Models
{
    public class Student : IdentityBase
    {
        [Required]
        [StringLength(10)]
        public string RollNumber { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        public string FathersName { get; set; } = string.Empty;
        public string MothersName { get; set; } = string.Empty;

        [Display(Name = "Class Name")]
        public int? ClassId { get; set; }

        [ForeignKey("ClassId")]
        public SchoolClass? Class { get; set; }

        [Display(Name = "Section Name")]
        public int? SectionId { get; set; }

        [ForeignKey("SectionId")]
        public Section? Section { get; set; }

        [Display(Name = "Academic Group")]
        public int? GroupId { get; set; }

        [ForeignKey("GroupId")]
        public AcademicGroup? Group { get; set; }

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
