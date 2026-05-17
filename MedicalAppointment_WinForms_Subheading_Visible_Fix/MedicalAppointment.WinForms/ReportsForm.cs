using System.Drawing;
using System.Windows.Forms;

namespace MedicalAppointment.WinForms;

public class ReportsForm : AppForm
{
    private readonly TableLayoutPanel cards = new();
    private readonly DonutChartPanel chart = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.White
    };

    public ReportsForm() : base("Reports")
    {
        AddSidebarTitle("Admin Panel");

        SidebarButton("Dashboard", () => Open(new AdminDashboardForm()));
        SidebarButton("Doctors", () => Open(new ManageDoctorsForm()));
        SidebarButton("Schedules", () => Open(new ManageSchedulesForm()));
        SidebarButton("Appointments", () => Open(new ManageAppointmentsForm()));
        SidebarButton("Logout", Logout);

        var layout = Ui.PageLayout(135, 280);

        layout.Dock = DockStyle.Fill;
        layout.Margin = Padding.Empty;
        layout.Padding = Padding.Empty;

        MainPanel.Controls.Add(layout);

        layout.Controls.Add(
            Ui.Header(
                "Reports",
                "Appointment summary and status distribution"
            ),
            0,
            0
        );

        cards.Dock = DockStyle.Fill;
        cards.ColumnCount = 4;
        cards.RowCount = 2;
        cards.BackColor = Ui.Bg;
        cards.Padding = new Padding(0, 10, 0, 20);
        cards.Margin = Padding.Empty;

        cards.ColumnStyles.Clear();
        cards.RowStyles.Clear();

        for (int i = 0; i < 4; i++)
        {
            cards.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 25)
            );
        }

        cards.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 125)
        );

        cards.RowStyles.Add(
            new RowStyle(SizeType.Absolute, 125)
        );

        layout.Controls.Add(cards, 0, 1);

        var chartPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            Padding = new Padding(35),
            Margin = Padding.Empty
        };

        chartPanel.Controls.Add(chart);

        layout.Controls.Add(chartPanel, 0, 2);

        Load += async (_, _) => await LoadData();
    }

    private Panel Card(string name, int value, Color color)
    {
        var p = Ui.Card();

        p.Dock = DockStyle.Fill;
        p.Margin = new Padding(0, 0, 14, 14);
        p.Padding = new Padding(22);
        p.BackColor = Color.White;

        var l = new Label
        {
            Text = name,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Ui.Dark,
            AutoSize = false,
            Width = 240,
            Height = 34,
            Location = new Point(22, 16),
            TextAlign = ContentAlignment.MiddleLeft
        };

        var v = new Label
        {
            Text = value.ToString(),
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = color,
            AutoSize = false,
            Width = 240,
            Height = 60,
            Location = new Point(22, 52),
            TextAlign = ContentAlignment.MiddleLeft
        };

        p.Controls.Add(l);
        p.Controls.Add(v);

        return p;
    }

    private async Task LoadData() => await Safe(async () =>
    {
        var r = await Api.GetReportSummary() ?? new ReportSummary();

        cards.Controls.Clear();

        cards.Controls.Add(
            Card("Users", r.users, Ui.Primary),
            0,
            0
        );

        cards.Controls.Add(
            Card("Doctors", r.doctors, Ui.Primary),
            1,
            0
        );

        cards.Controls.Add(
            Card("Schedules", r.schedules, Ui.Primary),
            2,
            0
        );

        cards.Controls.Add(
            Card("Appointments", r.appointments, Ui.Primary),
            3,
            0
        );

        cards.Controls.Add(
            Card(
                "Pending",
                r.pending,
                Color.FromArgb(245, 158, 11)
            ),
            0,
            1
        );

        cards.Controls.Add(
            Card(
                "Approved",
                r.approved,
                Color.FromArgb(34, 197, 94)
            ),
            1,
            1
        );

        cards.Controls.Add(
            Card(
                "Cancelled",
                r.cancelled,
                Color.FromArgb(239, 68, 68)
            ),
            2,
            1
        );

        int others =
            Math.Max(
                0,
                r.appointments
                - r.pending
                - r.approved
                - r.cancelled
            );

        cards.Controls.Add(
            Card(
                "Others",
                others,
                Color.FromArgb(100, 116, 139)
            ),
            3,
            1
        );

        chart.SetData(new[]
        {
            (
                "Approved",
                r.approved,
                Color.FromArgb(34, 197, 94)
            ),
            (
                "Others",
                others,
                Color.FromArgb(37, 99, 235)
            )
        });
    });

    public class DonutChartPanel : Panel
    {
        private List<(string Label, int Value, Color Color)> data = new();

        public void SetData(
            IEnumerable<(string Label, int Value, Color Color)> items
        )
        {
            data = items
                .Where(x => x.Value > 0)
                .ToList();

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int total = data.Sum(x => x.Value);

            if (total <= 0)
            {
                using Font noDataFont =
                    new Font("Segoe UI", 14, FontStyle.Bold);

                using SolidBrush grayBrush =
                    new SolidBrush(Color.Gray);

                e.Graphics.DrawString(
                    "No appointment data",
                    noDataFont,
                    grayBrush,
                    30,
                    30
                );

                return;
            }

            int size = Math.Min(Height - 80, 420);

            size = Math.Max(300, size);

            var rect = new Rectangle(
                70,
                45,
                size,
                size
            );

            float start = -90;

            foreach (var item in data)
            {
                float sweep =
                    item.Value * 360f / total;

                using var b =
                    new SolidBrush(item.Color);

                e.Graphics.FillPie(
                    b,
                    rect,
                    start,
                    sweep
                );

                start += sweep;
            }

            int hole =
                (int)(size * 0.58);

            var holeRect = new Rectangle(
                rect.Left + (size - hole) / 2,
                rect.Top + (size - hole) / 2,
                hole,
                hole
            );

            using var white =
                new SolidBrush(Color.White);

            e.Graphics.FillEllipse(
                white,
                holeRect
            );

            using var titleFont =
                new Font("Segoe UI", 18, FontStyle.Bold);

            using var textBrush =
                new SolidBrush(Ui.Dark);

            var text = total.ToString();

            var textSize =
                e.Graphics.MeasureString(
                    text,
                    titleFont
                );

            e.Graphics.DrawString(
                text,
                titleFont,
                textBrush,
                rect.Left
                + size / 2
                - textSize.Width / 2,
                rect.Top
                + size / 2
                - textSize.Height / 2
            );

            int x = rect.Right + 90;
            int y = rect.Top + 65;

            using var font =
                new Font("Segoe UI", 12, FontStyle.Bold);

            foreach (var item in data)
            {
                using var b =
                    new SolidBrush(item.Color);

                e.Graphics.FillRectangle(
                    b,
                    x,
                    y + 4,
                    18,
                    18
                );

                e.Graphics.DrawString(
                    $"{item.Label}: {item.Value}",
                    font,
                    textBrush,
                    x + 32,
                    y
                );

                y += 44;
            }
        }
    }
}