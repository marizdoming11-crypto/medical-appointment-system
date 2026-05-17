using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class CreateAppointmentForm : AppForm
{
    private readonly ComboBox cmbDoctor = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Font = new Font("Segoe UI", 11),
        Height = 42
    };

    private readonly DateTimePicker dtpDate = new()
    {
        Font = new Font("Segoe UI", 11),
        Format = DateTimePickerFormat.Short,
        Height = 42
    };

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

        var layout = Ui.PageLayout(135, 430);
        layout.Dock = DockStyle.Fill;
        layout.Margin = Padding.Empty;
        layout.Padding = Padding.Empty;

        MainPanel.Controls.Add(layout);

        layout.Controls.Add(
            Ui.Header(
                "Book Appointment",
                "Fill in the details below to submit a pending appointment"
            ),
            0,
            0
        );

        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(40),
            Margin = Padding.Empty,
            AutoScroll = true
        };

        layout.Controls.Add(card, 0, 1);

        AddField(card, "Doctor", cmbDoctor, 35);
        AddField(card, "Appointment Date", dtpDate, 105);

        var reasonLabel = new Label
        {
            Text = "Reason / Concern",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Ui.Dark,
            AutoSize = false,
            Width = 160,
            Height = 34,
            Location = new Point(35, 180),
            TextAlign = ContentAlignment.MiddleLeft
        };

        card.Controls.Add(reasonLabel);

        txtReason.Multiline = true;
        txtReason.Font = new Font("Segoe UI", 11);
        txtReason.SetBounds(220, 175, 620, 120);

        card.Controls.Add(txtReason);

        var submit = Ui.PrimaryButton("Submit Appointment");
        submit.SetBounds(220, 330, 260, 50);
        submit.Click += async (_, _) => await Submit();

        card.Controls.Add(submit);

        var cancel = Ui.LightButton("Cancel");
        cancel.SetBounds(500, 330, 160, 50);
        cancel.Click += (_, _) => Open(new UserDashboardForm());

        card.Controls.Add(cancel);

        Load += async (_, _) => await LoadDoctors();
    }

    private void AddField(Control parent, string label, Control input, int y)
    {
        var l = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            ForeColor = Ui.Dark,
            AutoSize = false,
            Width = 160,
            Height = 34,
            Location = new Point(35, y + 4),
            TextAlign = ContentAlignment.MiddleLeft
        };

        parent.Controls.Add(l);

        input.SetBounds(220, y, 620, 42);

        parent.Controls.Add(input);
    }

    private async Task LoadDoctors()
    {
        await Safe(async () =>
        {
            var doctors = await Api.GetDoctors();

            cmbDoctor.DataSource = doctors;
            cmbDoctor.DisplayMember = "fullName";
            cmbDoctor.ValueMember = "id";

            if (selectedDoctorId.HasValue)
                cmbDoctor.SelectedValue = selectedDoctorId.Value;
        });
    }

    private async Task Submit()
    {
        await Safe(async () =>
        {
            if (cmbDoctor.SelectedValue == null ||
                string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show(
                    "Select doctor and enter reason.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            await Api.CreateAppointment(
                new AppointmentCreate
                {
                    userId = Session.UserId,
                    doctorId = Convert.ToInt32(cmbDoctor.SelectedValue),
                    appointmentDate = dtpDate.Value.ToString("yyyy-MM-dd"),
                    reason = txtReason.Text.Trim()
                }
            );

            MessageBox.Show(
                "Appointment submitted. Status is Pending.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            Open(new MyAppointmentsForm());
        });
    }
}