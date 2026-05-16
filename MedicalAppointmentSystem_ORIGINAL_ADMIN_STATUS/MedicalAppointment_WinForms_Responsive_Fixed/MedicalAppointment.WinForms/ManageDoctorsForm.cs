using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class ManageDoctorsForm : AppForm
{
    private readonly TextBox txtSearch = Ui.TextBox("Search doctor");
    private readonly DataGridView grid = Ui.Grid();

    public ManageDoctorsForm() : base("Manage Doctors")
    {
        AddSidebarTitle("Admin Panel");
        SidebarButton("Dashboard", () => Open(new AdminDashboardForm()));
        SidebarButton("Schedules", () => Open(new ManageSchedulesForm()));
        SidebarButton("Appointments", () => Open(new ManageAppointmentsForm()));
        SidebarButton("Reports", () => Open(new ReportsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(80, 62);
        MainPanel.Controls.Add(layout);
        layout.Controls.Add(Ui.Header("Manage Doctors", "Add, update, search, and delete doctor records"), 0, 0);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Ui.Bg, WrapContents = false };
        txtSearch.Width = 280;
        var search = Ui.PrimaryButton("Search"); search.Width = 100; search.Click += async (_, _) => await LoadData();
        var add = Ui.PrimaryButton("Add Doctor"); add.Width = 130; add.Click += async (_, _) => await AddEdit();
        var edit = Ui.LightButton("Edit"); edit.Width = 90; edit.Click += async (_, _) => await Edit();
        var del = Ui.LightButton("Delete"); del.Width = 100; del.Click += async (_, _) => await Delete();
        toolbar.Controls.Add(txtSearch); toolbar.Controls.Add(search); toolbar.Controls.Add(add); toolbar.Controls.Add(edit); toolbar.Controls.Add(del);
        layout.Controls.Add(toolbar, 0, 1);

        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18) };
        content.Controls.Add(grid);
        layout.Controls.Add(content, 0, 2);
        Load += async (_, _) => await LoadData();
    }

    private async Task LoadData() => await Safe(async () =>
        grid.DataSource = (await Api.GetDoctors(txtSearch.Text)).Select(d => new { d.id, d.fullName, d.specialization, d.contactNumber }).ToList());

    private async Task AddEdit(int? id = null, string name = "", string spec = "", string contact = "")
    {
        using var f = new DoctorEditorForm(name, spec, contact);
        if (f.ShowDialog() != DialogResult.OK) return;
        await Safe(async () => { if (id.HasValue) await Api.UpdateDoctor(id.Value, f.Dto); else await Api.CreateDoctor(f.Dto); await LoadData(); });
    }

    private async Task Edit()
    {
        if (grid.CurrentRow == null) return;
        await AddEdit(Convert.ToInt32(grid.CurrentRow.Cells["id"].Value), grid.CurrentRow.Cells["fullName"].Value?.ToString() ?? "", grid.CurrentRow.Cells["specialization"].Value?.ToString() ?? "", grid.CurrentRow.Cells["contactNumber"].Value?.ToString() ?? "");
    }

    private async Task Delete()
    {
        if (grid.CurrentRow == null) return;
        if (MessageBox.Show("Delete selected doctor?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        await Safe(async () => { await Api.DeleteDoctor(Convert.ToInt32(grid.CurrentRow.Cells["id"].Value)); await LoadData(); });
    }
}

public class DoctorEditorForm : Form
{
    private readonly TextBox txtName = Ui.TextBox("Full name");
    private readonly TextBox txtSpec = Ui.TextBox("Specialization");
    private readonly TextBox txtContact = Ui.TextBox("Contact number");
    public DoctorCreate Dto => new() { fullName = txtName.Text.Trim(), specialization = txtSpec.Text.Trim(), contactNumber = txtContact.Text.Trim() };

    public DoctorEditorForm(string name, string spec, string contact)
    {
        Text = "Doctor"; Size = new System.Drawing.Size(460, 330); StartPosition = FormStartPosition.CenterParent; BackColor = Ui.Bg;
        var card = Ui.Card(); card.Dock = DockStyle.Fill; Controls.Add(card);
        var title = Ui.Label("Doctor Details", 17, true); title.Location = new System.Drawing.Point(30, 22); card.Controls.Add(title);
        txtName.Text = name; txtSpec.Text = spec; txtContact.Text = contact;
        txtName.SetBounds(30, 75, 370, 38); txtSpec.SetBounds(30, 130, 370, 38); txtContact.SetBounds(30, 185, 370, 38); card.Controls.AddRange(new Control[]{txtName, txtSpec, txtContact});
        var save = Ui.PrimaryButton("Save"); save.SetBounds(30, 245, 170, 42); save.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); }; card.Controls.Add(save);
        var cancel = Ui.LightButton("Cancel"); cancel.SetBounds(220, 245, 170, 42); cancel.Click += (_, _) => Close(); card.Controls.Add(cancel);
    }
}
