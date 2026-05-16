using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class DoctorsForm : AppForm
{
    private readonly TextBox txtSearch = Ui.TextBox("Search doctor or specialization");
    private readonly DataGridView grid = Ui.Grid();

    public DoctorsForm() : base("Available Doctors")
    {
        AddSidebarTitle("Patient Panel");
        SidebarButton("Dashboard", () => Open(new UserDashboardForm()));
        SidebarButton("Book Appointment", () => Open(new CreateAppointmentForm()));
        SidebarButton("My Appointments", () => Open(new MyAppointmentsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(80, 62);
        MainPanel.Controls.Add(layout);
        layout.Controls.Add(Ui.Header("Available Doctors", "Find and book an appointment with a doctor"), 0, 0);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Ui.Bg, WrapContents = false };
        txtSearch.Width = 360;
        var search = Ui.PrimaryButton("Search"); search.Width = 110; search.Click += async (_, _) => await LoadDoctors();
        var book = Ui.LightButton("Book Selected Doctor"); book.Width = 190; book.Click += (_, _) => BookSelected();
        toolbar.Controls.Add(txtSearch); toolbar.Controls.Add(search); toolbar.Controls.Add(book);
        layout.Controls.Add(toolbar, 0, 1);

        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18) };
        content.Controls.Add(grid);
        layout.Controls.Add(content, 0, 2);
        Load += async (_, _) => await LoadDoctors();
    }

    private async Task LoadDoctors() => await Safe(async () =>
        grid.DataSource = (await Api.GetDoctors(txtSearch.Text)).Select(d => new { d.id, d.fullName, d.specialization, d.contactNumber }).ToList());

    private void BookSelected()
    {
        if (grid.CurrentRow == null) return;
        int id = Convert.ToInt32(grid.CurrentRow.Cells["id"].Value);
        Open(new CreateAppointmentForm(id));
    }
}
