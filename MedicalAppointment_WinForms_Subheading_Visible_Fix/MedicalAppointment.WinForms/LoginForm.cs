using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class LoginForm : Form
{
    private readonly ApiClient Api = new();

    private readonly TextBox txtUsername = Ui.TextBox("Username");
    private readonly TextBox txtPassword = Ui.TextBox("Password");

    private readonly CheckBox chkShow = new()
    {
        Text = "Show password",
        AutoSize = true,
        Font = new Font("Segoe UI", 10),
        BackColor = Color.White
    };

    public LoginForm()
    {
        Text = "Medical Appointment System - Login";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Bg;

        var wrapper = new Panel
        {
            Width = 800,
            Height = 580,
            BackColor = Color.White,
            Anchor = AnchorStyles.None
        };

        Controls.Add(wrapper);

        Resize += (_, _) => CenterCard(wrapper);
        Load += (_, _) => CenterCard(wrapper);
        Shown += (_, _) => CenterCard(wrapper);

        // TITLE
        var title = new Label
        {
            Text = "Medical Appointment System",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = Ui.Dark,
            AutoSize = false,
            Width = 720,
            Height = 78,
            Location = new Point(40, 45),
            TextAlign = ContentAlignment.MiddleLeft
        };

        wrapper.Controls.Add(title);

        // SUBTITLE
        var sub = new Label
        {
            Text = "Sign in to continue",
            Font = new Font("Segoe UI", 12),
            ForeColor = Ui.Muted,
            AutoSize = false,
            Width = 720,
            Height = 32,
            Location = new Point(42, 120),
            TextAlign = ContentAlignment.MiddleLeft
        };

        wrapper.Controls.Add(sub);

        // USERNAME
        txtUsername.Font = new Font("Segoe UI", 11);
        txtUsername.SetBounds(40, 190, 720, 42);

        wrapper.Controls.Add(txtUsername);

        // PASSWORD
        txtPassword.Font = new Font("Segoe UI", 11);
        txtPassword.SetBounds(40, 255, 720, 42);
        txtPassword.UseSystemPasswordChar = true;

        wrapper.Controls.Add(txtPassword);

        // SHOW PASSWORD
        chkShow.Location = new Point(40, 315);

        chkShow.CheckedChanged += (_, _) =>
        {
            txtPassword.UseSystemPasswordChar = !chkShow.Checked;
        };

        wrapper.Controls.Add(chkShow);

        // LOGIN BUTTON
        var btnLogin = Ui.PrimaryButton("Login");

        btnLogin.SetBounds(40, 375, 720, 50);

        btnLogin.Click += async (_, _) => await DoLogin();

        wrapper.Controls.Add(btnLogin);

        // REGISTER BUTTON
        var btnRegister = Ui.LightButton("Create Account");

        btnRegister.SetBounds(40, 445, 720, 50);

        btnRegister.Click += (_, _) =>
        {
            new RegisterForm().Show();
            Hide();
        };

        wrapper.Controls.Add(btnRegister);
    }

    private void CenterCard(Control card)
    {
        card.Left = (ClientSize.Width - card.Width) / 2;
        card.Top = (ClientSize.Height - card.Height) / 2;
    }

    private async Task DoLogin()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show(
                    "Enter username and password.",
                    "Validation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            // FIXED API URL
            Session.ApiBaseUrl = "http://127.0.0.1:8001";

            var login = await Api.Login(
                txtUsername.Text.Trim(),
                txtPassword.Text
            );

            if (login == null)
                return;

            Session.AccessToken = login.access_token;
            Session.UserId = login.id;
            Session.FullName = login.fullName;
            Session.Username = login.username;
            Session.Role = login.role;

            if (Session.Role == "Admin")
                new AdminDashboardForm().Show();
            else
                new UserDashboardForm().Show();

            Hide();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Login Failed",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}