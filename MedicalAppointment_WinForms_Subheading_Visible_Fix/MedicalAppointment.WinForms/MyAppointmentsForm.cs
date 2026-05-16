using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class MyAppointmentsForm : AppForm
{
    private readonly DataGridView grid = Ui.Grid();

    public MyAppointmentsForm() : base("My Appointments")
    {
        AddSidebarTitle("Patient Panel");
        SidebarButton("Dashboard", () => Open(new UserDashboardForm()));
        SidebarButton("Doctors", () => Open(new DoctorsForm()));
        SidebarButton("Book Appointment", () => Open(new CreateAppointmentForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(80, 62);
        MainPanel.Controls.Add(layout);
        layout.Controls.Add(Ui.Header("My Appointments", "View and manage your pending appointments"), 0, 0);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Ui.Bg, WrapContents = false };
        var edit = Ui.PrimaryButton("Edit Selected"); edit.Width = 150; edit.Click += (_, _) => Edit();
        var cancel = Ui.LightButton("Cancel Selected"); cancel.Width = 170; cancel.Click += async (_, _) => await Cancel();
        var refresh = Ui.LightButton("Refresh"); refresh.Width = 120; refresh.Click += async (_, _) => await LoadData();
        toolbar.Controls.Add(edit); toolbar.Controls.Add(cancel); toolbar.Controls.Add(refresh);
        layout.Controls.Add(toolbar, 0, 1);

        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18) };
        content.Controls.Add(grid);
        layout.Controls.Add(content, 0, 2);
        Load += async (_, _) => await LoadData();
    }

    private async Task LoadData() => await Safe(async () =>
        grid.DataSource = (await Api.GetMyAppointments(Session.UserId)).Select(a => new { a.id, Doctor = a.doctor?.fullName ?? a.doctorId.ToString(), a.doctorId, a.appointmentDate, a.reason, a.status }).ToList());

    private void Edit()
    {
        if (grid.CurrentRow == null) return;
        if (grid.CurrentRow.Cells["status"].Value?.ToString() != "Pending") { MessageBox.Show("Only pending appointments can be edited."); return; }
        Open(new EditAppointmentForm(Convert.ToInt32(grid.CurrentRow.Cells["id"].Value), Convert.ToInt32(grid.CurrentRow.Cells["doctorId"].Value), grid.CurrentRow.Cells["appointmentDate"].Value?.ToString() ?? "", grid.CurrentRow.Cells["reason"].Value?.ToString() ?? ""));
    }

    private async Task Cancel()
    {
        if (grid.CurrentRow == null) return;
        if (grid.CurrentRow.Cells["status"].Value?.ToString() != "Pending") { MessageBox.Show("Only pending appointments can be cancelled."); return; }
        if (MessageBox.Show("Cancel this appointment?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        await Safe(async () => { await Api.CancelMyAppointment(Convert.ToInt32(grid.CurrentRow.Cells["id"].Value)); MessageBox.Show("Appointment cancelled."); await LoadData(); });
    }
}
