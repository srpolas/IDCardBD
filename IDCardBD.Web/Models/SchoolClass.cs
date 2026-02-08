using System.ComponentModel.DataAnnotations;

namespace IDCardBD.Web.Models
{
    public class SchoolClass
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Class Name")]
        public string Name { get; set; } = string.Empty;

        public ICollection<Section> Sections { get; set; } = new List<Section>();
    }
}
