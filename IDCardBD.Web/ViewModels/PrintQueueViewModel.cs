using IDCardBD.Web.Models;

namespace IDCardBD.Web.ViewModels
{
    public class PrintQueueViewModel
    {
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Employee> Employees { get; set; } = new List<Employee>();
    }
}
