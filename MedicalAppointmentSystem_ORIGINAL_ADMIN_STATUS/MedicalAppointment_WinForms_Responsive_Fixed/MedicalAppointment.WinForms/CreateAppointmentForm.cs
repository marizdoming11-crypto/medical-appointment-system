using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class CreateAppointmentForm : AppForm
{
    private readonly ComboBox cmbDoctor = new() { DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11), Height = 38 };
    private readonly DateTimePicker dtpDate = new() { Font = new Font("Segoe UI", 11), Format = DateTimePickerFormat.Short, Height = 38 };
    private readonly TextBox txtReason = Ui.TextBox("Reason / concern");
    private readonly int? selectedDoctorId;

    public CreateAppointmentForm(int? doctorId = null) : base("Book Appointment")
    {
        selectedDoctorId = doctorId;
        AddSidebarTitle("Patient Panel");
        SidebarButton("Dashboard", () => Open(new UserDashboardForm()));
        SidebarButton("Doctors", () => Open(new DoctorsForm()));
        SidebarButton("My Appointments", () => Open(new MyAppointmentsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(80);
        MainPanel.Controls.Add(layout);
        layout.Controls.Add(Ui.Header("Book Appointment", "Fill in the details below to submit a pending appointment"), 0, 0);

        var card = Ui.Card();
        card.Dock = DockStyle.Top;
        card.Height = 360;
        card.Margin = new Padding(0);
        layout.Controls.Add(card, 0, 1);

        AddField(card, "Doctor", cmbDoctor, 25);
        AddField(card, "Appointment Date", dtpDate, 90);
        var reasonLabel = Ui.Label("Reason / Concern", 10, true); reasonLabel.Location = new Point(35, 155); card.Controls.Add(reasonLabel);
        txtReason.Multiline = true; txtReason.SetBounds(210, 150, 520, 90); card.Controls.Add(txtReason);
        var submit = Ui.PrimaryButton("Submit Appointment"); submit.SetBounds(210, 270, 220, 44); submit.Click += async (_, _) => await Submit(); card.Controls.Add(submit);
        var cancel = Ui.LightButton("Cancel"); cancel.SetBounds(445, 270, 140, 44); cancel.Click += (_, _) => Open(new UserDashboardForm()); card.Controls.Add(cancel);
        Load += async (_, _) => await LoadDoctors();
    }

    private void AddField(Control parent, string label, Control input, int y)
    {
        var l = Ui.Label(label, 10, true); l.Location = new Point(35, y + 6); parent.Controls.Add(l);
        input.SetBounds(210, y, 520, 38); parent.Controls.Add(input);
    }

    private async Task LoadDoctors()
    {
        await Safe(async () =>
        {
            var doctors = await Api.GetDoctors();
            cmbDoctor.DataSource = doctors; cmbDoctor.DisplayMember = "fullName"; cmbDoctor.ValueMember = "id";
            if (selectedDoctorId.HasValue) cmbDoctor.SelectedValue = selectedDoctorId.Value;
        });
    }

    private async Task Submit()
    {
        await Safe(async () =>
        {
            if (cmbDoctor.SelectedValue == null || string.IsNullOrWhiteSpace(txtReason.Text)) { MessageBox.Show("Select doctor and enter reason."); return; }
            await Api.CreateAppointment(new AppointmentCreate { userId = Session.UserId, doctorId = Convert.ToInt32(cmbDoctor.SelectedValue), appointmentDate = dtpDate.Value.ToString("yyyy-MM-dd"), reason = txtReason.Text.Trim() });
            MessageBox.Show("Appointment submitted. Status is Pending."); Open(new MyAppointmentsForm());
        });
    }
}
