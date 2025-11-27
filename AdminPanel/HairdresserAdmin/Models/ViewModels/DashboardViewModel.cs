namespace HairdresserAdmin.Models.ViewModels
{
    public class DashboardViewModel
    {
        public DateOnly SelectedDate { get; set; }
        public List<AppointmentViewModel> Appointments { get; set; } = new();
        public int TotalAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int CancelledAppointments { get; set; }
    }

    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string Time { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ServiceType { get; set; }
        public string? Notes { get; set; }
        public int DurationMinutes { get; set; }

        public string StatusBadgeClass => Status switch
        {
            "confirmed" => "badge bg-success",
            "pending" => "badge bg-warning",
            "cancelled" => "badge bg-danger",
            "completed" => "badge bg-info",
            _ => "badge bg-secondary"
        };

        public string StatusText => Status switch
        {
            "confirmed" => "Onaylandı",
            "pending" => "Bekliyor",
            "cancelled" => "İptal",
            "completed" => "Tamamlandı",
            _ => Status
        };
    }
}

