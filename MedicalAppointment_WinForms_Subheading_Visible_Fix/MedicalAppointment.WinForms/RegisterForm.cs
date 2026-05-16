using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class RegisterForm : Form
{
    private readonly ApiClient Api = new();
    private readonly TextBox txtName = Ui.TextBox("Full name");
    private readonly TextBox txtUsername = Ui.TextBox("Username");
    private readonly TextBox txtPassword = Ui.TextBox("Password");
    private readonly TextBox txtConfirm = Ui.TextBox("Confirm password");
    private readonly CheckBox chkShow = new() { Text = "Show password", AutoSize = true, Font = Ui.BodyFont, BackColor = Color.White };

    public RegisterForm()
    {
        Text = "Register";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 650);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Ui.Bg;

        var card = new Panel { Width = 520, Height = 540, BackColor = Color.White, Anchor = AnchorStyles.None, Padding = new Padding(45) };
        Controls.Add(card);
        Resize += (_, _) => Center(card); Load += (_, _) => Center(card);

        var title = Ui.Label("Create Account", 24, true); title.Location = new Point(45, 42); card.Controls.Add(title);
        var sub = Ui.Label("Register your patient account", 11); sub.ForeColor = Ui.Muted; sub.Location = new Point(48, 84); card.Controls.Add(sub);
        txtName.SetBounds(45, 130, 430, 40); card.Controls.Add(txtName);
        txtUsername.SetBounds(45, 190, 430, 40); card.Controls.Add(txtUsername);
        txtPassword.SetBounds(45, 250, 430, 40); txtPassword.UseSystemPasswordChar = true; card.Controls.Add(txtPassword);
        txtConfirm.SetBounds(45, 310, 430, 40); txtConfirm.UseSystemPasswordChar = true; card.Controls.Add(txtConfirm);
        chkShow.Location = new Point(45, 365); chkShow.CheckedChanged += (_, _) => { txtPassword.UseSystemPasswordChar = !chkShow.Checked; txtConfirm.UseSystemPasswordChar = !chkShow.Checked; }; card.Controls.Add(chkShow);
        var btn = Ui.PrimaryButton("Register"); btn.SetBounds(45, 415, 430, 44); btn.Click += async (_, _) => await Register(); card.Controls.Add(btn);
        var back = Ui.LightButton("Back to Login"); back.SetBounds(45, 472, 430, 44); back.Click += (_, _) => { new LoginForm().Show(); Hide(); }; card.Controls.Add(back);
    }

    private void Center(Control c) { c.Left = (ClientSize.Width - c.Width) / 2; c.Top = (ClientSize.Height - c.Height) / 2; }

    private async Task Register()
    {
        try
        {
            if (txtName.Text == "" || txtUsername.Text == "" || txtPassword.Text == "") { MessageBox.Show("Fill all fields."); return; }
            if (txtPassword.Text != txtConfirm.Text) { MessageBox.Show("Passwords do not match."); return; }
            await Api.Register(txtName.Text.Trim(), txtUsername.Text.Trim(), txtPassword.Text);
            MessageBox.Show("Registration successful. You can now login."); new LoginForm().Show(); Hide();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Register Failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
