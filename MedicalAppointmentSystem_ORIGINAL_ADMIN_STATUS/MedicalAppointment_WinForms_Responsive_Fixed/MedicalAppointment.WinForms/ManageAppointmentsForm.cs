using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class ManageAppointmentsForm : AppForm
{
    private readonly ComboBox cmbStatus = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = Ui.BodyFont, Width = 180, Height = 38 };
    private readonly DataGridView grid = Ui.Grid();

    public ManageAppointmentsForm() : base("Manage Appointments")
    {
        AddSidebarTitle("Admin Panel");
        SidebarButton("Dashboard", () => Open(new AdminDashboardForm()));
        SidebarButton("Doctors", () => Open(new ManageDoctorsForm()));
        SidebarButton("Schedules", () => Open(new ManageSchedulesForm()));
        SidebarButton("Reports", () => Open(new ReportsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(80, 62);
        MainPanel.Controls.Add(layout);
        layout.Controls.Add(Ui.Header("Manage Appointments", "Approve, reject, cancel, or complete selected appointments"), 0, 0);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Ui.Bg, WrapContents = false };
        cmbStatus.Items.AddRange(new object[] { "", "Pending", "Approved", "Rejected", "Completed", "Cancelled" }); cmbStatus.SelectedIndex = 0;
        var filter = Ui.PrimaryButton("Filter"); filter.Width = 90; filter.Click += async (_, _) => await LoadData();
        toolbar.Controls.Add(cmbStatus); toolbar.Controls.Add(filter);
        foreach (var s in new[] { "Approved", "Rejected", "Cancelled", "Completed" })
        {
            var b = Ui.LightButton(s); b.Width = 120; b.Click += async (_, _) => await SetStatus(s); toolbar.Controls.Add(b);
        }
        layout.Controls.Add(toolbar, 0, 1);

        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18) };
        content.Controls.Add(grid);
        layout.Controls.Add(content, 0, 2);
        Load += async (_, _) => await LoadData();
    }

    private async Task LoadData() => await Safe(async () =>
        grid.DataSource = (await Api.GetAllAppointments(cmbStatus.Text)).Select(a => new { a.id, Patient = a.user?.fullName ?? a.userId.ToString(), Doctor = a.doctor?.fullName ?? a.doctorId.ToString(), a.appointmentDate, a.reason, a.status }).ToList());

    private async Task SetStatus(string status)
    {
        if (grid.CurrentRow == null) return;
        await Safe(async () => { await Api.AdminSetAppointmentStatus(Convert.ToInt32(grid.CurrentRow.Cells["id"].Value), status); MessageBox.Show($"Appointment set to {status}."); await LoadData(); });
    }
}
