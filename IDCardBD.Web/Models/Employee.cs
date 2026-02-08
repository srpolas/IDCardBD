using System.ComponentModel.DataAnnotations;

namespace IDCardBD.Web.Models
{
    public class Employee : IdentityBase
    {
        [Required]
        [StringLength(50)]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Designation { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Department { get; set; } = string.Empty;

        [StringLength(5)]
        public string? BloodGroup { get; set; }
    }
}
