using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class UserDashboardForm : AppForm
{
    private readonly Label lblTotal = Ui.Label("0", 26, true);
    private readonly Label lblPending = Ui.Label("0", 26, true);
    private readonly Label lblApproved = Ui.Label("0", 26, true);
    private readonly TableLayoutPanel cards = new();
    private readonly DataGridView grid = Ui.Grid();

    public UserDashboardForm() : base("User Dashboard")
    {
        AddSidebarTitle("Patient Panel");
        SidebarButton("Dashboard", async () => await LoadData());
        SidebarButton("Doctors", () => Open(new DoctorsForm()));
        SidebarButton("Book Appointment", () => Open(new CreateAppointmentForm()));
        SidebarButton("My Appointments", () => Open(new MyAppointmentsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(80, 135);
        MainPanel.Controls.Add(layout);
        layout.Controls.Add(Ui.Header("Patient Dashboard", $"Welcome, {Session.FullName}"), 0, 0);

        cards.Dock = DockStyle.Fill;
        cards.ColumnCount = 3;
        cards.RowCount = 1;
        cards.BackColor = Ui.Bg;
        for (int i = 0; i < 3; i++) cards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        layout.Controls.Add(cards, 0, 1);

        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18) };
        var title = Ui.Label("Recent Appointments", 14, true); title.Dock = DockStyle.Top; title.Height = 42;
        content.Controls.Add(grid);
        content.Controls.Add(title);
        layout.Controls.Add(content, 0, 2);

        Load += async (_, _) => await LoadData();
    }

    private Panel Card(string title, Label value, Color color)
    {
        var p = Ui.Card(); p.Dock = DockStyle.Fill; p.Margin = new Padding(0, 0, 18, 18);
        var t = Ui.Label(title, 11, true); t.Location = new Point(18, 16); p.Controls.Add(t);
        value.Location = new Point(18, 52); value.ForeColor = color; p.Controls.Add(value);
        return p;
    }

    private async Task LoadData()
    {
        await Safe(async () =>
        {
            var list = await Api.GetMyAppointments(Session.UserId);
            lblTotal.Text = list.Count.ToString();
            lblPending.Text = list.Count(a => a.status == "Pending").ToString();
            lblApproved.Text = list.Count(a => a.status == "Approved").ToString();
            cards.Controls.Clear();
            cards.Controls.Add(Card("Total Appointments", lblTotal, Ui.Primary), 0, 0);
            cards.Controls.Add(Card("Pending", lblPending, Color.FromArgb(245, 158, 11)), 1, 0);
            cards.Controls.Add(Card("Approved", lblApproved, Color.FromArgb(34, 197, 94)), 2, 0);
            grid.DataSource = list.Select(a => new { a.id, Doctor = a.doctor?.fullName ?? a.doctorId.ToString(), a.appointmentDate, a.reason, a.status }).ToList();
        });
    }
}
