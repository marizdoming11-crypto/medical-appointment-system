using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public static class Ui
{
    public static readonly Color Primary = Color.FromArgb(37, 99, 235);
    public static readonly Color PrimaryDark = Color.FromArgb(30, 64, 175);
    public static readonly Color Bg = Color.FromArgb(244, 247, 251);
    public static readonly Color Dark = Color.FromArgb(15, 23, 42);
    public static readonly Color Muted = Color.FromArgb(100, 116, 139);
    public static readonly Font TitleFont = new("Segoe UI", 22, FontStyle.Bold);
    public static readonly Font HeaderFont = new("Segoe UI", 13, FontStyle.Bold);
    public static readonly Font BodyFont = new("Segoe UI", 10);

    public static Button PrimaryButton(string text) => StyleButton(new Button { Text = text, BackColor = Primary, ForeColor = Color.White }, true);
    public static Button LightButton(string text) => StyleButton(new Button { Text = text, BackColor = Color.White, ForeColor = Primary }, false);

    private static Button StyleButton(Button b, bool primary)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = primary ? 0 : 1;
        b.FlatAppearance.BorderColor = Primary;
        b.Height = 42;
        b.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        b.Cursor = Cursors.Hand;
        b.TextAlign = ContentAlignment.MiddleCenter;
        return b;
    }

    public static TextBox TextBox(string placeholder = "") => new()
    {
        Font = new Font("Segoe UI", 11),
        Height = 38,
        PlaceholderText = placeholder,
        BorderStyle = BorderStyle.FixedSingle,
        Margin = new Padding(0, 6, 0, 10)
    };

    public static Label Label(string text, int size = 10, bool bold = false) => new()
    {
        Text = text,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
        AutoSize = true,
        ForeColor = Dark,
        Margin = new Padding(0, 0, 0, 8)
    };

    public static Panel Card() => new()
    {
        BackColor = Color.White,
        Padding = new Padding(20),
        Margin = new Padding(0, 0, 18, 15)
    };

    public static DataGridView Grid()
    {
        var g = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            Font = new Font("Segoe UI", 10),
            RowTemplate = { Height = 38 },
            EnableHeadersVisualStyles = false,
            GridColor = Color.FromArgb(226, 232, 240)
        };
        g.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        g.ColumnHeadersHeight = 42;
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
        g.DefaultCellStyle.SelectionForeColor = Dark;
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        return g;
    }

    public static TableLayoutPanel PageLayout(int headerHeight = 80, int toolbarHeight = 0)
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = toolbarHeight > 0 ? 3 : 2,
            BackColor = Bg
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, headerHeight));
        if (toolbarHeight > 0) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, toolbarHeight));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        return layout;
    }

    public static Panel Header(string title, string subtitle = "")
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Bg, Padding = new Padding(0, 0, 0, 10) };
        var t = Label(title, 22, true); t.Location = new Point(0, 0); p.Controls.Add(t);
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            var s = Label(subtitle, 10, false); s.ForeColor = Muted; s.Location = new Point(2, 40); p.Controls.Add(s);
        }
        return p;
    }
}

public class AppForm : Form
{
    protected readonly ApiClient Api = new();
    protected Panel MainPanel = new();
    protected FlowLayoutPanel Sidebar = new();
    private TableLayoutPanel? Shell;

    public AppForm(string title, bool withSidebar = true)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1050, 620);
        BackColor = Ui.Bg;
        Font = Ui.BodyFont;
        AutoScaleMode = AutoScaleMode.Dpi;

        MainPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 22, 24, 22),
            BackColor = Ui.Bg,
            Margin = Padding.Empty
        };

        if (withSidebar)
        {
            Shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Ui.Bg,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            Shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
            Shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            Shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(Shell);

            Sidebar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Ui.PrimaryDark,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(14, 28, 14, 18),
                Margin = Padding.Empty
            };

            Shell.Controls.Add(Sidebar, 0, 0);
            Shell.Controls.Add(MainPanel, 1, 0);
        }
        else
        {
            Controls.Add(MainPanel);
        }

        Resize += (_, _) => AdjustResponsiveLayout();
        Shown += (_, _) => AdjustResponsiveLayout();
    }

    private void AdjustResponsiveLayout()
    {
        if (Shell == null) return;

        // Good for small 12.5-inch laptop screens while still using full screen.
        int sidebarWidth = ClientSize.Width < 1250 ? 205 : 220;
        Shell.ColumnStyles[0].Width = sidebarWidth;

        MainPanel.Padding = ClientSize.Width < 1250
            ? new Padding(18, 18, 18, 18)
            : new Padding(24, 22, 24, 22);

        int innerWidth = sidebarWidth - Sidebar.Padding.Left - Sidebar.Padding.Right;
        foreach (Control control in Sidebar.Controls)
            control.Width = innerWidth;
    }

    protected void AddSidebarTitle(string title)
    {
        var label = new Label
        {
            Text = title,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            Height = 66,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            Margin = new Padding(0, 0, 0, 12)
        };
        Sidebar.Controls.Add(label);
    }

    protected Button SidebarButton(string text, Action click)
    {
        var btn = new Button
        {
            Text = "  " + text,
            Height = 46,
            BackColor = Ui.PrimaryDark,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 3, 0, 3),
            AutoEllipsis = true
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.MouseEnter += (_, _) => btn.BackColor = Ui.Primary;
        btn.MouseLeave += (_, _) => btn.BackColor = Ui.PrimaryDark;
        btn.Click += (_, _) => click();
        Sidebar.Controls.Add(btn);
        return btn;
    }

    protected void Logout()
    {
        Session.Clear();
        new LoginForm().Show();
        Hide();
    }

    protected async Task Safe(Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    protected void Open(Form form)
    {
        form.Show();
        Hide();
    }
}
