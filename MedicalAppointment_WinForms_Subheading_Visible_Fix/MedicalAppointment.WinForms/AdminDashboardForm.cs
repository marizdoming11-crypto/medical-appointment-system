using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class AdminDashboardForm : AppForm
{
    private readonly TableLayoutPanel cards = new();
    private readonly DataGridView grid = Ui.Grid();

    public AdminDashboardForm() : base("Admin Dashboard")
    {
        AddSidebarTitle("Admin Panel");

        SidebarButton("Dashboard", async () => await LoadData());
        SidebarButton("Manage Doctors", () => Open(new ManageDoctorsForm()));
        SidebarButton("Manage Schedules", () => Open(new ManageSchedulesForm()));
        SidebarButton("Appointments", () => Open(new ManageAppointmentsForm()));
        SidebarButton("Reports", () => Open(new ReportsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(140, 170);
        layout.Dock = DockStyle.Fill;
        layout.Margin = Padding.Empty;
        layout.Padding = new Padding(0);

        MainPanel.Controls.Add(layout);

        layout.Controls.Add(
            Ui.Header("System Administrator", $"Welcome, {Session.FullName}"),
            0,
            0
        );

        cards.Dock = DockStyle.Fill;
        cards.ColumnCount = 4;
        cards.RowCount = 1;
        cards.BackColor = Ui.Bg;
        cards.Padding = new Padding(0, 0, 0, 8);

        cards.ColumnStyles.Clear();

        for (int i = 0; i < 4; i++)
            cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

        layout.Controls.Add(cards, 0, 1);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(18),
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty
        };

        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var title = Ui.SectionTitle("Latest Appointments");
        title.Dock = DockStyle.Fill;
        title.AutoSize = false;
        title.Height = 56;
        title.TextAlign = ContentAlignment.MiddleLeft;
        title.Margin = Padding.Empty;

        grid.Dock = DockStyle.Fill;
        grid.Margin = Padding.Empty;

        content.Controls.Add(title, 0, 0);
        content.Controls.Add(grid, 0, 1);

        layout.Controls.Add(content, 0, 2);

        Load += async (_, _) => await LoadData();
    }

    private Panel Card(string name, int value, Color color)
    {
        var p = Ui.Card();

        p.Dock = DockStyle.Fill;
        p.Margin = new Padding(0, 0, 18, 18);
        p.Padding = new Padding(22, 18, 22, 18);

        var l = Ui.Label(name, 11, true);
        l.AutoSize = false;
        l.Width = 220;
        l.Height = 28;
        l.Location = new Point(22, 18);

        p.Controls.Add(l);

        var v = Ui.Label(value.ToString(), 26, true);
        v.AutoSize = false;
        v.Width = 220;
        v.Height = 56;
        v.Location = new Point(22, 58);
        v.ForeColor = color;

        p.Controls.Add(v);

        return p;
    }

    private async Task LoadData() => await Safe(async () =>
    {
        var summary = await Api.GetReportSummary() ?? new ReportSummary();

        cards.Controls.Clear();

        cards.Controls.Add(Card("Doctors", summary.doctors, Ui.Primary), 0, 0);
        cards.Controls.Add(Card("Patients", summary.users, Color.FromArgb(16, 185, 129)), 1, 0);
        cards.Controls.Add(Card("Pending", summary.pending, Color.FromArgb(245, 158, 11)), 2, 0);
        cards.Controls.Add(Card("Approved", summary.approved, Color.FromArgb(34, 197, 94)), 3, 0);

        var appts = await Api.GetAllAppointments();

        grid.DataSource = appts.Take(10).Select(a => new
        {
            a.id,
            Patient = a.user?.fullName ?? a.userId.ToString(),
            Doctor = a.doctor?.fullName ?? a.doctorId.ToString(),
            a.appointmentDate,
            a.reason,
            a.status
        }).ToList();
    });
}