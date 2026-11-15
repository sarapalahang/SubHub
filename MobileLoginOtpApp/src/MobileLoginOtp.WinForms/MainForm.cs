using System.Drawing;
using System.Windows.Forms;

namespace MobileLoginOtp.WinForms;

public sealed class MainForm : Form
{
    private static readonly Color PageColor = Color.FromArgb(245, 247, 251);
    private static readonly Color SidebarColor = Color.FromArgb(23, 39, 59);
    private static readonly Color AccentColor = Color.FromArgb(54, 116, 219);
    private static readonly Color TextColor = Color.FromArgb(35, 49, 70);
    private static readonly Color MutedColor = Color.FromArgb(112, 124, 143);

    private readonly Panel _body;
    private readonly Label _pageTitle;
    private readonly Label _pageSubtitle;
    private Button? _activeNavigation;

    public MainForm()
    {
        Text = "MobileLoginOtp | پنل مدیریت";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1180, 680);
        MinimumSize = new Size(900, 620);
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;
        BackColor = PageColor;
        Font = new Font("Segoe UI", 9.5F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            RightToLeft = RightToLeft.Yes,
            Margin = Padding.Empty,
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 252));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var sidebar = BuildSidebar();
        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(30, 24, 30, 22),
            BackColor = PageColor,
        };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
        };
        header.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _pageTitle = new Label
        {
            Text = "داشبورد مدیریت",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 21F, FontStyle.Bold),
            ForeColor = TextColor,
        };
        _pageSubtitle = new Label
        {
            Text = "مدیریت یکپارچه‌ی ورود، پیامک، کاربران و پرداخت‌ها",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = MutedColor,
        };
        header.Controls.Add(_pageTitle, 0, 0);
        header.Controls.Add(_pageSubtitle, 0, 1);

        _body = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = PageColor,
        };
        content.Controls.Add(header, 0, 0);
        content.Controls.Add(_body, 0, 1);

        root.Controls.Add(sidebar, 0, 0);
        root.Controls.Add(content, 1, 0);
        Controls.Add(root);

        ShowDashboard();
    }

    private Control BuildSidebar()
    {
        var sidebar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(16, 18, 16, 16),
            BackColor = SidebarColor,
        };
        sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 88));
        for (var i = 0; i < 5; i++)
        {
            sidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        }
        sidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var brand = new Panel { Dock = DockStyle.Fill };
        brand.Controls.Add(new Label
        {
            Text = "MobileLoginOtp",
            Dock = DockStyle.Top,
            Height = 35,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.White,
        });
        brand.Controls.Add(new Label
        {
            Text = "پنل مدیریتی",
            Dock = DockStyle.Top,
            Height = 27,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(174, 190, 210),
        });
        sidebar.Controls.Add(brand, 0, 0);

        AddNavigation(sidebar, 1, "نمای کلی", ShowDashboard, true);
        AddNavigation(sidebar, 2, "کاربران و نقش‌ها", () => ShowModule(
            "کاربران و نقش‌ها",
            "مدیریت کاربران، تعریف نقش‌ها و تعیین سطح دسترسی."));
        AddNavigation(sidebar, 3, "ورود OTP و پیامک", () => ShowModule(
            "ورود OTP و پیامک",
            "پیگیری درخواست‌های کد یک‌بارمصرف و گزارش ارسال پیامک‌ها."));
        AddNavigation(sidebar, 4, "پرداخت‌ها", () => ShowModule(
            "پرداخت‌ها",
            "مشاهده‌ی تراکنش‌ها و پیگیری نتیجه‌ی پرداخت‌های زرین‌پال."));
        AddNavigation(sidebar, 5, "دستیار منابع انسانی", () => ShowModule(
            "دستیار منابع انسانی",
            "پرسش‌وپاسخ منابع انسانی با سرویس هوش مصنوعی پروژه."));

        var connection = new Label
        {
            Text = "وضعیت اتصال\nرابط کاربری آماده است؛ اتصال به API انجام نشده.",
            Dock = DockStyle.Bottom,
            Height = 58,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(174, 190, 210),
            Font = new Font("Segoe UI", 8.5F),
        };
        sidebar.Controls.Add(connection, 0, 6);
        return sidebar;
    }

    private void AddNavigation(TableLayoutPanel sidebar, int row, string text, Action action, bool selected = false)
    {
        var button = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Height = 42,
            FlatStyle = FlatStyle.Flat,
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(12, 0, 12, 0),
            Margin = new Padding(0, 2, 0, 4),
            BackColor = selected ? Color.FromArgb(42, 65, 94) : SidebarColor,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
        };
        button.FlatAppearance.BorderSize = 0;
        button.Click += (_, _) =>
        {
            if (_activeNavigation is not null)
            {
                _activeNavigation.BackColor = SidebarColor;
            }

            _activeNavigation = button;
            button.BackColor = Color.FromArgb(42, 65, 94);
            action();
        };
        sidebar.Controls.Add(button, 0, row);

        if (selected)
        {
            _activeNavigation = button;
        }
    }

    private void ShowDashboard()
    {
        _pageTitle.Text = "داشبورد مدیریت";
        _pageSubtitle.Text = "نمای کلی بخش‌های اصلی سامانه";
        _body.Controls.Clear();

        var page = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
        };
        page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));
        page.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            RightToLeft = RightToLeft.Yes,
            Margin = new Padding(0, 0, 0, 8),
        };
        for (var i = 0; i < 4; i++)
        {
            metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }
        metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        metrics.Controls.Add(CreateMetricCard("کاربران فعال", Color.FromArgb(54, 116, 219)), 0, 0);
        metrics.Controls.Add(CreateMetricCard("درخواست‌های OTP", Color.FromArgb(119, 91, 208)), 1, 0);
        metrics.Controls.Add(CreateMetricCard("پیامک‌های امروز", Color.FromArgb(20, 155, 131)), 2, 0);
        metrics.Controls.Add(CreateMetricCard("پرداخت‌های موفق", Color.FromArgb(222, 143, 46)), 3, 0);

        var sectionTitle = new Label
        {
            Text = "بخش‌های سامانه",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold),
            ForeColor = TextColor,
            Margin = new Padding(0, 4, 0, 0),
        };

        var features = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            RightToLeft = RightToLeft.Yes,
            Margin = Padding.Empty,
        };
        features.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        features.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        features.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        features.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        features.Controls.Add(CreateFeatureCard("کاربران و نقش‌ها", "فهرست کاربران، تعریف نقش و مدیریت دسترسی‌ها.", "کاربر", () => ShowModule(
            "کاربران و نقش‌ها", "مدیریت کاربران، تعریف نقش‌ها و تعیین سطح دسترسی.")), 0, 0);
        features.Controls.Add(CreateFeatureCard("ورود OTP و پیامک", "ورود با کد یک‌بارمصرف و گزارش پیامک‌های ارسالی.", "OTP", () => ShowModule(
            "ورود OTP و پیامک", "پیگیری درخواست‌های کد یک‌بارمصرف و گزارش ارسال پیامک‌ها.")), 1, 0);
        features.Controls.Add(CreateFeatureCard("پرداخت‌ها", "پیگیری تراکنش‌ها و وضعیت تأیید پرداخت.", "پرداخت", () => ShowModule(
            "پرداخت‌ها", "مشاهده‌ی تراکنش‌ها و پیگیری نتیجه‌ی پرداخت‌های زرین‌پال.")), 0, 1);
        features.Controls.Add(CreateFeatureCard("دستیار منابع انسانی", "پرسش‌وپاسخ HR با سرویس هوش مصنوعی پروژه.", "HR", () => ShowModule(
            "دستیار منابع انسانی", "پرسش‌وپاسخ منابع انسانی با سرویس هوش مصنوعی پروژه.")), 1, 1);

        var note = new Label
        {
            Text = "برای نمایش آمار واقعی و انجام عملیات، این پنل باید به API برنامه متصل شود.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = MutedColor,
            BackColor = Color.FromArgb(235, 240, 248),
            Padding = new Padding(14, 0, 14, 0),
            Margin = new Padding(0, 8, 0, 0),
        };

        page.Controls.Add(metrics, 0, 0);
        page.Controls.Add(sectionTitle, 0, 1);
        page.Controls.Add(features, 0, 2);
        page.Controls.Add(note, 0, 3);
        _body.Controls.Add(page);
    }

    private Control CreateMetricCard(string title, Color accent)
    {
        var card = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Margin = new Padding(4, 4, 4, 4),
        };
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 4));
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.Controls.Add(new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = accent,
            Margin = Padding.Empty,
        }, 0, 0);

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(13, 7, 13, 5),
            BackColor = Color.White,
            Margin = Padding.Empty,
        };
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        content.Controls.Add(new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = TextColor,
        }, 0, 0);
        content.Controls.Add(new Label
        {
            Text = "—",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = accent,
        }, 0, 1);
        content.Controls.Add(new Label
        {
            Text = "پس از اتصال API نمایش داده می‌شود",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 8F),
            ForeColor = MutedColor,
        }, 0, 2);
        card.Controls.Add(content, 0, 1);
        return card;
    }

    private Control CreateFeatureCard(string title, string description, string tag, Action action)
    {
        var card = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            RightToLeft = RightToLeft.Yes,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(13, 8, 13, 6),
            Margin = new Padding(5),
        };
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
        card.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        card.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

        var tagLabel = new Label
        {
            Text = tag,
            Size = new Size(78, 25),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(232, 240, 253),
            ForeColor = AccentColor,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Margin = Padding.Empty,
        };
        var titleLabel = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = TextColor,
            Margin = Padding.Empty,
        };
        var descriptionLabel = new Label
        {
            Text = description,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopRight,
            ForeColor = MutedColor,
            Margin = Padding.Empty,
        };
        var buttonRow = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
        var openButton = new Button
        {
            Text = "نمایش بخش",
            Size = new Size(112, 29),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(243, 247, 253),
            ForeColor = AccentColor,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
            Margin = Padding.Empty,
        };
        openButton.FlatAppearance.BorderColor = Color.FromArgb(218, 229, 245);
        openButton.Click += (_, _) => action();
        buttonRow.Controls.Add(openButton);

        card.Controls.Add(tagLabel, 0, 0);
        card.Controls.Add(titleLabel, 0, 1);
        card.Controls.Add(descriptionLabel, 0, 2);
        card.Controls.Add(buttonRow, 0, 3);
        return card;
    }

    private void ShowModule(string title, string description)
    {
        _pageTitle.Text = title;
        _pageSubtitle.Text = description;
        _body.Controls.Clear();

        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 205,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(24),
        };
        panel.Controls.Add(new Label
        {
            Text = "این بخش به قابلیت‌های پروژه مربوط است و برای نمایش یا ویرایش اطلاعات، باید به API متصل شود.",
            Dock = DockStyle.Top,
            Height = 62,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = TextColor,
            Font = new Font("Segoe UI", 11F),
        });
        var backButton = new Button
        {
            Text = "بازگشت به داشبورد",
            Dock = DockStyle.Bottom,
            Height = 40,
            Width = 180,
            FlatStyle = FlatStyle.Flat,
            BackColor = AccentColor,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            UseVisualStyleBackColor = false,
        };
        backButton.FlatAppearance.BorderSize = 0;
        backButton.Click += (_, _) => ShowDashboard();
        panel.Controls.Add(backButton);
        _body.Controls.Add(panel);
    }
}
