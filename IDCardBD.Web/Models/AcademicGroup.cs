using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IDCardBD.Web.Models
{
    public class AcademicGroup
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Group Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public SchoolClass? Class { get; set; }
    }
}
