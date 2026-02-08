

namespace IDCardBD.Web.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalEmployees { get; set; }
        public int TotalTeachers { get; set; }
        public int PendingPrints { get; set; }
        public List<RecentActivityViewModel> RecentActivities { get; set; } = new();
    }

    public class RecentActivityViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; }
        public string? PhotoPath { get; set; }
    }
}
