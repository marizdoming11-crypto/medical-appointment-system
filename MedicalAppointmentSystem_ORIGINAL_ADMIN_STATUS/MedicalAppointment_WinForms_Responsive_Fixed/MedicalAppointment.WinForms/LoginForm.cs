using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class LoginForm : Form
{
    private readonly ApiClient Api = new();
    private readonly TextBox txtApi = Ui.TextBox("API URL");
    private readonly TextBox txtUsername = Ui.TextBox("Username");
    private readonly TextBox txtPassword = Ui.TextBox("Password");
    private readonly CheckBox chkShow = new() { Text = "Show password", AutoSize = true, Font = Ui.BodyFont, BackColor = Color.White };

    public LoginForm()
    {
        Text = "Medical Appointment System - Login";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 620);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Bg;

        var wrapper = new Panel { Width = 520, Height = 500, BackColor = Color.White, Anchor = AnchorStyles.None, Padding = new Padding(45) };
        Controls.Add(wrapper);
        Resize += (_, _) => CenterCard(wrapper);
        Load += (_, _) => CenterCard(wrapper);

        var title = Ui.Label("Medical Appointment", 24, true); title.Location = new Point(45, 45); wrapper.Controls.Add(title);
        var sub = Ui.Label("Sign in to continue", 11); sub.ForeColor = Ui.Muted; sub.Location = new Point(48, 88); wrapper.Controls.Add(sub);

        txtApi.Text = Session.ApiBaseUrl; txtApi.SetBounds(45, 135, 430, 40); wrapper.Controls.Add(txtApi);
        txtUsername.SetBounds(45, 195, 430, 40); wrapper.Controls.Add(txtUsername);
        txtPassword.SetBounds(45, 255, 430, 40); txtPassword.UseSystemPasswordChar = true; wrapper.Controls.Add(txtPassword);
        chkShow.Location = new Point(45, 310); chkShow.CheckedChanged += (_, _) => txtPassword.UseSystemPasswordChar = !chkShow.Checked; wrapper.Controls.Add(chkShow);

        var btnLogin = Ui.PrimaryButton("Login"); btnLogin.SetBounds(45, 360, 430, 44); btnLogin.Click += async (_, _) => await DoLogin(); wrapper.Controls.Add(btnLogin);
        var btnRegister = Ui.LightButton("Create Account"); btnRegister.SetBounds(45, 418, 430, 44); btnRegister.Click += (_, _) => { new RegisterForm().Show(); Hide(); }; wrapper.Controls.Add(btnRegister);
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
            if (string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text)) { MessageBox.Show("Enter username and password."); return; }
            Session.ApiBaseUrl = txtApi.Text.Trim();
            var login = await Api.Login(txtUsername.Text.Trim(), txtPassword.Text);
            if (login == null) return;
            Session.AccessToken = login.access_token; Session.UserId = login.id; Session.FullName = login.fullName; Session.Username = login.username; Session.Role = login.role;
            if (Session.Role == "Admin") new AdminDashboardForm().Show(); else new UserDashboardForm().Show();
            Hide();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
