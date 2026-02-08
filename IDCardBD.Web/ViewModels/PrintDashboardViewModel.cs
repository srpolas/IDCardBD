using IDCardBD.Web.Models;

namespace IDCardBD.Web.ViewModels
{
    public class PrintDashboardViewModel
    {
        public int SentToPrintCount { get; set; }
        public int ProcessingCount { get; set; }
        public int PrintedCount { get; set; }
        public int ReadyForDeliveryCount { get; set; }

        public PrintStatus CurrentStatus { get; set; }
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}
