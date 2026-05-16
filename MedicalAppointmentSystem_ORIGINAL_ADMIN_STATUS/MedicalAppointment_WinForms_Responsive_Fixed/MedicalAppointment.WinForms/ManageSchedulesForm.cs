using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class ManageSchedulesForm : AppForm
{
    private readonly DataGridView grid = Ui.Grid();

    public ManageSchedulesForm() : base("Manage Schedules")
    {
        AddSidebarTitle("Admin Panel");
        SidebarButton("Dashboard", () => Open(new AdminDashboardForm()));
        SidebarButton("Doctors", () => Open(new ManageDoctorsForm()));
        SidebarButton("Appointments", () => Open(new ManageAppointmentsForm()));
        SidebarButton("Reports", () => Open(new ReportsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(80, 62);
        MainPanel.Controls.Add(layout);
        layout.Controls.Add(Ui.Header("Manage Schedules", "Manage doctor schedule availability"), 0, 0);

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, BackColor = Ui.Bg, WrapContents = false };
        var add = Ui.PrimaryButton("Add Schedule"); add.Width = 145; add.Click += async (_, _) => await AddEdit();
        var edit = Ui.LightButton("Edit"); edit.Width = 90; edit.Click += async (_, _) => await Edit();
        var del = Ui.LightButton("Delete"); del.Width = 100; del.Click += async (_, _) => await Delete();
        var refresh = Ui.LightButton("Refresh"); refresh.Width = 110; refresh.Click += async (_, _) => await LoadData();
        toolbar.Controls.Add(add); toolbar.Controls.Add(edit); toolbar.Controls.Add(del); toolbar.Controls.Add(refresh);
        layout.Controls.Add(toolbar, 0, 1);

        var content = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18) };
        content.Controls.Add(grid);
        layout.Controls.Add(content, 0, 2);
        Load += async (_, _) => await LoadData();
    }

    private async Task LoadData() => await Safe(async () =>
        grid.DataSource = (await Api.GetSchedules()).Select(s => new { s.id, s.doctorId, Doctor = s.doctor?.fullName ?? s.doctorId.ToString(), s.scheduleDate, s.startTime, s.endTime, s.isAvailable }).ToList());

    private async Task AddEdit(int? id = null, int doctorId = 0, string date = "", string start = "", string end = "", bool avail = true)
    {
        var doctors = await Api.GetDoctors();
        using var f = new ScheduleEditorForm(doctors, doctorId, date, start, end, avail);
        if (f.ShowDialog() != DialogResult.OK) return;
        await Safe(async () => { if (id.HasValue) await Api.UpdateSchedule(id.Value, f.Dto); else await Api.CreateSchedule(f.Dto); await LoadData(); });
    }

    private async Task Edit()
    {
        if (grid.CurrentRow == null) return;
        await AddEdit(Convert.ToInt32(grid.CurrentRow.Cells["id"].Value), Convert.ToInt32(grid.CurrentRow.Cells["doctorId"].Value), grid.CurrentRow.Cells["scheduleDate"].Value?.ToString() ?? "", grid.CurrentRow.Cells["startTime"].Value?.ToString() ?? "", grid.CurrentRow.Cells["endTime"].Value?.ToString() ?? "", Convert.ToBoolean(grid.CurrentRow.Cells["isAvailable"].Value));
    }

    private async Task Delete()
    {
        if (grid.CurrentRow == null) return;
        if (MessageBox.Show("Delete selected schedule?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        await Safe(async () => { await Api.DeleteSchedule(Convert.ToInt32(grid.CurrentRow.Cells["id"].Value)); await LoadData(); });
    }
}

public class ScheduleEditorForm : Form
{
    private readonly ComboBox cmbDoctor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new System.Drawing.Font("Segoe UI", 11), Height = 38 };
    private readonly DateTimePicker dtpDate = new() { Format = DateTimePickerFormat.Short, Font = new System.Drawing.Font("Segoe UI", 11), Height = 38 };
    private readonly TextBox txtStart = Ui.TextBox("Start time ex. 09:00");
    private readonly TextBox txtEnd = Ui.TextBox("End time ex. 17:00");
    private readonly CheckBox chkAvail = new() { Text = "Available", AutoSize = true, Font = Ui.BodyFont };
    public ScheduleCreate Dto => new() { doctorId = Convert.ToInt32(cmbDoctor.SelectedValue), scheduleDate = dtpDate.Value.ToString("yyyy-MM-dd"), startTime = txtStart.Text.Trim(), endTime = txtEnd.Text.Trim(), isAvailable = chkAvail.Checked };

    public ScheduleEditorForm(List<Doctor> doctors, int doctorId, string date, string start, string end, bool avail)
    {
        Text = "Schedule"; Size = new System.Drawing.Size(490, 405); StartPosition = FormStartPosition.CenterParent; BackColor = Ui.Bg;
        var card = Ui.Card(); card.Dock = DockStyle.Fill; Controls.Add(card);
        var title = Ui.Label("Schedule Details", 17, true); title.Location = new System.Drawing.Point(30, 22); card.Controls.Add(title);
        cmbDoctor.DataSource = doctors; cmbDoctor.DisplayMember = "fullName"; cmbDoctor.ValueMember = "id"; if (doctorId > 0) cmbDoctor.SelectedValue = doctorId;
        if (DateTime.TryParse(date, out var d)) dtpDate.Value = d; txtStart.Text = start; txtEnd.Text = end; chkAvail.Checked = avail;
        cmbDoctor.SetBounds(30, 75, 390, 38); dtpDate.SetBounds(30, 130, 390, 38); txtStart.SetBounds(30, 185, 390, 38); txtEnd.SetBounds(30, 240, 390, 38); chkAvail.Location = new System.Drawing.Point(30, 295);
        card.Controls.AddRange(new Control[] { cmbDoctor, dtpDate, txtStart, txtEnd, chkAvail });
        var save = Ui.PrimaryButton("Save"); save.SetBounds(30, 330, 180, 42); save.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); }; card.Controls.Add(save);
        var cancel = Ui.LightButton("Cancel"); cancel.SetBounds(230, 330, 180, 42); cancel.Click += (_, _) => Close(); card.Controls.Add(cancel);
    }
}
