using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class EditAppointmentForm : AppForm
{
    private readonly int appointmentId;
    private readonly ComboBox cmbDoctor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11), Height = 38 };
    private readonly DateTimePicker dtpDate = new() { Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short, Height = 38 };
    private readonly TextBox txtReason = Ui.TextBox("Reason");
    private readonly int doctorId;

    public EditAppointmentForm(int id, int doctorId, string date, string reason) : base("Edit Appointment")
    {
        appointmentId = id; this.doctorId = doctorId;
        AddSidebarTitle("Patient Panel");
        SidebarButton("My Appointments", () => Open(new MyAppointmentsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(80);
        MainPanel.Controls.Add(layout);
        layout.Controls.Add(Ui.Header("Edit Appointment", "Only pending appointments can be updated"), 0, 0);

        var card = Ui.Card(); card.Dock = DockStyle.Top; card.Height = 340; card.Margin = new Padding(0); layout.Controls.Add(card, 0, 1);
        AddField(card, "Doctor", cmbDoctor, 25);
        AddField(card, "Date", dtpDate, 90);
        var r = Ui.Label("Reason", 10, true); r.Location = new Point(35, 155); card.Controls.Add(r);
        txtReason.Multiline = true; txtReason.Text = reason; txtReason.SetBounds(210, 150, 520, 80); card.Controls.Add(txtReason);
        if (DateTime.TryParse(date, out var parsed)) dtpDate.Value = parsed;
        var update = Ui.PrimaryButton("Update Appointment"); update.SetBounds(210, 260, 220, 44); update.Click += async (_, _) => await Update(); card.Controls.Add(update);
        var back = Ui.LightButton("Back"); back.SetBounds(445, 260, 140, 44); back.Click += (_, _) => Open(new MyAppointmentsForm()); card.Controls.Add(back);
        Load += async (_, _) => await LoadDoctors();
    }

    private void AddField(Control parent, string label, Control input, int y)
    {
        var l = Ui.Label(label, 10, true); l.Location = new Point(35, y + 6); parent.Controls.Add(l);
        input.SetBounds(210, y, 520, 38); parent.Controls.Add(input);
    }

    private async Task LoadDoctors() => await Safe(async () =>
    {
        var doctors = await Api.GetDoctors(); cmbDoctor.DataSource = doctors; cmbDoctor.DisplayMember = "fullName"; cmbDoctor.ValueMember = "id"; cmbDoctor.SelectedValue = doctorId;
    });

    private async Task Update() => await Safe(async () =>
    {
        await Api.UpdateMyAppointment(appointmentId, new AppointmentUpdate { doctorId = Convert.ToInt32(cmbDoctor.SelectedValue), appointmentDate = dtpDate.Value.ToString("yyyy-MM-dd"), reason = txtReason.Text.Trim() });
        MessageBox.Show("Appointment updated."); Open(new MyAppointmentsForm());
    });
}
