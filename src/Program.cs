// 桌面图标美化 —— 快捷方式小箭头 & UAC 小盾牌 一键管理
// 功能：
//   隐藏快捷方式小箭头：方案一 explorer.exe,-264 / 方案二 Taskbar.dll,-264 / 方案三 blank.ico
//   隐藏 UAC 小盾牌：   方案一 explorer.exe,-264 / 方案二 shell32.dll,-50
//   恢复系统默认 / 重建桌面图标缓存
// 当前生效的方案自动高亮；blank.ico 内嵌进 exe，单文件双击即用；界面跟随系统日间/夜间模式。

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ShortcutArrow
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        internal static void DragWindow(Form f, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(f.Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
            }
        }

        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 预览模式（供无提权环境下截图检查界面用），跳过管理员检测
            if (Environment.GetEnvironmentVariable("SAPREVIEW") != "1")
            {
                bool admin = false;
                try
                {
                    WindowsIdentity id = WindowsIdentity.GetCurrent();
                    WindowsPrincipal pr = new WindowsPrincipal(id);
                    admin = pr.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch { }
                if (!admin)
                {
                    MessageBox.Show("本工具需要管理员权限才能修改系统设置。\n请右键选择【以管理员身份运行】。",
                        "桌面图标美化工具", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            Ui.Apply(Ui.IsDarkMode());

            // 单实例：重复双击时提示并退出
            bool createdNew = false;
            using (Mutex mutex = new Mutex(true, "DesktopIconBeautyTool_SingleInstance", out createdNew))
            {
                if (!createdNew)
                {
                    if (Environment.GetEnvironmentVariable("SAPREVIEW") != "1")
                        MessageBox.Show("桌面图标美化工具已在运行中。", "桌面图标美化工具",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Application.Run(new MainForm());
            }
        }
    }

    // ------------------------- 配色（跟随系统日间/夜间模式） -------------------------
    internal static class Ui
    {
        public static Color Bg, Card, Border, TextMain, TextSub;
        public static Color Accent, AccentHv;      // 蓝：小箭头
        public static Color Teal, TealHv;          // #307DF1 蓝：UAC 小盾牌
        public static Color Violet, VioletHv;      // 紫：右键菜单
        public static Color Rebuild, RebuildHv;    // #FFDD7D 黄：重建图标缓存
        public static Color Green, Gray, Amber;    // Green=已生效 Gray=未修改 Amber=执行中
        public static Color OutlineHover, OutlineDown, PressedText;
        public static Color GradFrom, GradTo;      // 方案按钮统一渐变 #c850c0 → #4158d0
        public static Color MenuFrom, MenuTo;      // 右键菜单渐变 #ff9a9e → #fad0c4

        public static void Apply(bool dark)
        {
            if (dark)
            {
                Bg       = Color.FromArgb(24, 27, 34);
                Card     = Color.FromArgb(33, 37, 46);
                Border   = Color.FromArgb(56, 62, 75);
                TextMain = Color.FromArgb(233, 236, 242);
                TextSub  = Color.FromArgb(152, 160, 176);
                Accent   = Color.FromArgb(76, 130, 240);
                AccentHv = Color.FromArgb(62, 114, 222);
                Teal     = Color.FromArgb(48, 125, 241);
                TealHv   = Color.FromArgb(37, 102, 215);
                Violet   = Color.FromArgb(134, 96, 230);
                VioletHv = Color.FromArgb(114, 78, 210);
                Rebuild   = Color.FromArgb(246, 211, 101);
                RebuildHv = Color.FromArgb(253, 160, 133);
                Green    = Color.FromArgb(58, 188, 136);
                Gray     = Color.FromArgb(112, 120, 136);
                Amber    = Color.FromArgb(230, 154, 60);
                OutlineHover = Color.FromArgb(43, 49, 61);
                OutlineDown  = Color.FromArgb(50, 57, 70);
                PressedText  = Color.White;
                GradFrom = Color.FromArgb(200, 80, 192);
                GradTo   = Color.FromArgb(65, 88, 208);
                MenuFrom = Color.FromArgb(255, 154, 158);
                MenuTo   = Color.FromArgb(250, 208, 196);
            }
            else
            {
                Bg       = Color.FromArgb(244, 246, 250);
                Card     = Color.White;
                Border   = Color.FromArgb(228, 233, 241);
                TextMain = Color.FromArgb(35, 45, 62);
                TextSub  = Color.FromArgb(128, 140, 158);
                Accent   = Color.FromArgb(47, 106, 227);
                AccentHv = Color.FromArgb(36, 90, 199);
                Teal     = Color.FromArgb(48, 125, 241);
                TealHv   = Color.FromArgb(37, 102, 215);
                Violet   = Color.FromArgb(118, 74, 222);
                VioletHv = Color.FromArgb(101, 60, 205);
                Rebuild   = Color.FromArgb(246, 211, 101);
                RebuildHv = Color.FromArgb(253, 160, 133);
                Green    = Color.FromArgb(34, 168, 116);
                Gray     = Color.FromArgb(170, 178, 190);
                Amber    = Color.FromArgb(224, 145, 40);
                OutlineHover = Color.FromArgb(245, 248, 252);
                OutlineDown  = Color.FromArgb(234, 239, 246);
                PressedText  = Color.White;
                GradFrom = Color.FromArgb(200, 80, 192);
                GradTo   = Color.FromArgb(65, 88, 208);
                MenuFrom = Color.FromArgb(255, 154, 158);
                MenuTo   = Color.FromArgb(250, 208, 196);
            }
        }

        public static bool IsDarkMode()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (k != null)
                    {
                        object v = k.GetValue("AppsUseLightTheme");
                        if (v is int) return ((int)v) == 0;
                    }
                }
            }
            catch { }
            return false;
        }
    }

    internal static class UiPath
    {
        public static GraphicsPath Round(Rectangle r, int radius)
        {
            GraphicsPath p = new GraphicsPath();
            int d = radius * 2;
            if (d > r.Width) d = r.Width;
            if (d > r.Height) d = r.Height;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }

    // ------------------------- 按钮 -------------------------
    internal class RoundedButton : Control
    {
        public Color FillColor = Ui.Accent;
        public Color HoverColor = Ui.AccentHv;
        public bool Outline = false;
        public bool DarkText = false;   // 浅色底按钮使用深色文字
        public bool Gradient = false;   // 135° 渐变底（FillColor → GradTo）
        public Color GradTo = Color.FromArgb(65, 88, 208);
        public Color OutlineColor = Ui.Gray;
        public Color OutlineHover = Ui.OutlineHover;
        public Color OutlineDown = Ui.OutlineDown;

        private bool _hover, _down;

        public RoundedButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint | ControlStyles.ResizeRedraw
                   | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
            Font = new Font("Microsoft YaHei UI", 9.75F, FontStyle.Bold);
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiPath.Round(r, 9))
            {
                Color fill, text, line;
                bool gradient = false;
                if (!Enabled)
                {
                    fill = Ui.OutlineDown;
                    text = Ui.Gray; line = Ui.Border;
                }
                else if (Outline)
                {
                    fill = _down ? OutlineDown : (_hover ? OutlineHover : Ui.Card);
                    text = Ui.TextMain;
                    line = _hover || _down ? OutlineColor : Ui.Gray;
                }
                else
                {
                    fill = (_hover || _down) ? HoverColor : FillColor;
                    text = DarkText ? Color.FromArgb(82, 72, 34) : Ui.PressedText;
                    line = fill;
                    gradient = Gradient;
                }
                if (gradient)
                {
                    // 135° 对角渐变（GDI 角度 45° = CSS 135deg），悬停/按下叠加半透明暗层
                    Rectangle gr = new Rectangle(0, 0, Width, Height);
                    using (LinearGradientBrush gb = new LinearGradientBrush(gr, FillColor, GradTo, 45F))
                    {
                        gb.WrapMode = System.Drawing.Drawing2D.WrapMode.TileFlipXY;
                        g.FillPath(gb, path);
                    }
                    if (_hover || _down)
                    using (SolidBrush sh = new SolidBrush(Color.FromArgb(_down ? 40 : 20, 0, 0, 0)))
                        g.FillPath(sh, path);
                }
                else
                {
                    using (SolidBrush b = new SolidBrush(fill)) g.FillPath(b, path);
                }
                using (Pen p = new Pen(line, 1.1F)) g.DrawPath(p, path);

                // 逐字符绘制：字间距 +0.1px，水平/垂直严格居中
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                System.Drawing.StringFormat sf = System.Drawing.StringFormat.GenericTypographic;
                const float spacing = 0.1F;
                float total = -spacing;
                foreach (char c in Text) total += g.MeasureString(c.ToString(), Font, System.Drawing.PointF.Empty, sf).Width + spacing;
                float x = (Width - total) / 2F;
                float lh = g.MeasureString("中", Font, System.Drawing.PointF.Empty, sf).Height;
                float y = (Height - lh) / 2F + (_down ? 1 : 0);
                using (SolidBrush tb = new SolidBrush(text))
                {
                    foreach (char c in Text)
                    {
                        string s = c.ToString();
                        g.DrawString(s, Font, tb, x, y, sf);
                        x += g.MeasureString(s, Font, System.Drawing.PointF.Empty, sf).Width + spacing;
                    }
                }
            }
        }
    }

    // ------------------------- 圆角卡片（抗锯齿绘制，不用 Region 裁切） -------------------------
    internal class CardPanel : Panel
    {
        public int Radius = 10;

        public CardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 交给 OnPaint 统一画圆角，避免方角底色闪现
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Ui.Bg);
            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath p = UiPath.Round(r, Radius))
            {
                using (SolidBrush b = new SolidBrush(Ui.Card)) g.FillPath(b, p);
                using (Pen pen = new Pen(Ui.Border, 1F)) g.DrawPath(pen, p);
            }
            base.OnPaint(e);
        }
    }

    // 标题栏右侧按钮（最小化 / 关闭）：仅图标变色，无背景遮罩
    internal class TitleButton : Control
    {
        public string Kind = "close";
        private bool _hover;

        public TitleButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint | ControlStyles.ResizeRedraw
                   | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Ui.Card);
            Color c = _hover ? (Kind == "close" ? Color.FromArgb(236, 96, 110) : Ui.TextMain) : Ui.TextSub;
            using (Pen p = new Pen(c, 1.6F))
            {
                int cy = Height / 2;
                if (Kind == "close")
                {
                    g.DrawLine(p, 17, cy - 5, 27, cy + 5);
                    g.DrawLine(p, 27, cy - 5, 17, cy + 5);
                }
                else
                {
                    g.DrawLine(p, 17, cy + 2, 27, cy + 2);
                }
            }
        }
    }

    // 支持字间距与对齐方式的标签（状态行名 / 状态徽标 / 底部说明共用）
    internal class SpacedLabel : Control
    {
        public float LetterSpacing = 0.1F;
        public ContentAlignment Align = ContentAlignment.MiddleLeft;

        public SpacedLabel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.UserPaint | ControlStyles.ResizeRedraw
                   | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            Font = new Font("Microsoft YaHei UI", 8F);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Text.Length == 0) return;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            System.Drawing.StringFormat sf = System.Drawing.StringFormat.GenericTypographic;
            using (SolidBrush b = new SolidBrush(ForeColor))
            {
                float total = -LetterSpacing;
                foreach (char c in Text) total += g.MeasureString(c.ToString(), Font, System.Drawing.PointF.Empty, sf).Width + LetterSpacing;
                float x;
                if (Align == ContentAlignment.MiddleRight) x = Width - total - 2;
                else if (Align == ContentAlignment.MiddleCenter) x = (Width - total) / 2F;
                else x = 2;
                float y = (Height - g.MeasureString("中", Font, System.Drawing.PointF.Empty, sf).Height) / 2F;
                foreach (char c in Text)
                {
                    string s = c.ToString();
                    g.DrawString(s, Font, b, x, y, sf);
                    x += g.MeasureString(s, Font, System.Drawing.PointF.Empty, sf).Width + LetterSpacing;
                }
            }
        }
    }

    // ------------------------- 主窗口 -------------------------
    internal class MainForm : Form
    {
        private const string RK  = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";
        private const string RK32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons";
        private const string NK  = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Naming Templates";
        private const string EK  = @"Software\Microsoft\Windows\CurrentVersion\Explorer";
        private const string MENU_CLSID = @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}";

        private readonly CardPanel _statusCard = new CardPanel();
        private readonly SpacedLabel _arrowState = new SpacedLabel { Align = ContentAlignment.MiddleLeft, LetterSpacing = 0.4F };
        private readonly Label _arrowDetail = new Label();
        private readonly SpacedLabel _suffixState = new SpacedLabel { Align = ContentAlignment.MiddleLeft, LetterSpacing = 0.4F };
        private readonly Label _suffixDetail = new Label();
        private readonly SpacedLabel _shieldState = new SpacedLabel { Align = ContentAlignment.MiddleLeft, LetterSpacing = 0.4F };
        private readonly Label _shieldDetail = new Label();
        private readonly SpacedLabel _menuState = new SpacedLabel { Align = ContentAlignment.MiddleLeft, LetterSpacing = 0.4F };
        private readonly Label _menuDetail = new Label();
        private readonly SpacedLabel _arrowBadge = new SpacedLabel { Align = ContentAlignment.MiddleRight, LetterSpacing = 0.5F };
        private readonly SpacedLabel _shieldBadge = new SpacedLabel { Align = ContentAlignment.MiddleRight, LetterSpacing = 0.5F };
        private readonly SpacedLabel _menuBadge = new SpacedLabel { Align = ContentAlignment.MiddleRight, LetterSpacing = 0.5F };
        private readonly SpacedLabel _suffixBadge = new SpacedLabel { Align = ContentAlignment.MiddleRight, LetterSpacing = 0.5F };
        private Color _dotArrow = Ui.Gray;
        private Color _dotShield = Ui.Gray;
        private Color _dotMenu = Ui.Gray;
        private Color _dotSuffix = Ui.Gray;

        private readonly RoundedButton[] _btns = new RoundedButton[16];
        private RoundedButton _a1, _a2, _a3, _a4, _s1, _s2, _s3, _s4;
        private Label _secArrow, _secSuffix, _secShield, _secRestore, _secTool;
        private Panel _titleBar;
        private readonly List<Action> _restyle = new List<Action>();

        private bool _busy;
        private int _busyRow = -1;
        private bool _lastLocked;
        private bool _lastDark = Ui.IsDarkMode();
        private Icon _appIcon;
        private Image _logo;
        private MemoryStream _logoStream;

        [DllImport("dwmapi.dll")]
        private static extern void DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        // ---- 接管 TrustedInstaller 所有的注册表键所需 ----
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(string systemName, string privilegeName, out long luid);
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(IntPtr tokenHandle, bool disableAllPrivileges, ref TOKEN_PRIVILEGES newState, int bufferLengthInBytes, IntPtr previousState, IntPtr returnLength);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct TOKEN_PRIVILEGES
        {
            public int PrivilegeCount;
            public long Luid;      // LUID：偏移必须紧随 PrivilegeCount（原生 4 字节对齐）
            public int Attributes;
        }

        // 启用 SeTakeOwnership / SeRestore 特权（管理员默认拥有，但需显式启用）
        private static void EnablePrivilege(string privilegeName)
        {
            try
            {
                IntPtr token;
                if (!OpenProcessToken(Process.GetCurrentProcess().Handle, 0x28, out token)) return;
                TOKEN_PRIVILEGES tp;
                tp.PrivilegeCount = 1;
                tp.Attributes = 2; // SE_PRIVILEGE_ENABLED
                if (!LookupPrivilegeValue(null, privilegeName, out tp.Luid)) { CloseHandle(token); return; }
                AdjustTokenPrivileges(token, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
                CloseHandle(token);
            }
            catch { }
        }

        public MainForm()
        {
            Text = "桌面图标美化工具";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(520, 862);
            BackColor = Ui.Bg;
            DoubleBuffered = true;
            Font = new Font("Microsoft YaHei UI", 9F);
            KeyPreview = true;

            // 应用图标：任务栏 / 桌面快捷方式用 exe 图标；标题栏 LOGO 用内嵌 64px PNG 高质量缩放
            try
            {
                _appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                Icon = _appIcon;
            }
            catch { }
            try
            {
                using (Stream rs = Assembly.GetExecutingAssembly().GetManifestResourceStream("logo64.png"))
                {
                    if (rs != null)
                    {
                        _logoStream = new MemoryStream();
                        byte[] buf = new byte[4096];
                        int n;
                        while ((n = rs.Read(buf, 0, buf.Length)) > 0) _logoStream.Write(buf, 0, n);
                        _logoStream.Position = 0;
                        _logo = Image.FromStream(_logoStream);
                    }
                }
            }
            catch { }

            KeyDown += delegate(object s, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) Close(); };
            Load += delegate
            {
                // Win11：让 DWM 给无边框窗口做抗锯齿圆角；Win10 无效则保持直角
                try { int pref = 2; DwmSetWindowAttribute(Handle, 33, ref pref, 4); } catch { }
                RefreshStatus();
            };
            Paint += delegate(object s, PaintEventArgs e)
            {
                using (Pen p = new Pen(Ui.Border, 1F))
                    e.Graphics.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            };

            // ---------- 标题栏 ----------
            _titleBar = new Panel { Location = new Point(0, 0), Size = new Size(520, 54), BackColor = Ui.Card };
            _titleBar.MouseDown += delegate(object s, MouseEventArgs e) { Program.DragWindow(this, e); };
            _titleBar.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                if (_logo != null)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(_logo, 18, 14, 26, 26);
                }
                else if (_appIcon != null)
                    g.DrawIcon(_appIcon, new Rectangle(18, 14, 26, 26));

                // 标题：桌面图标美化工具 ● By Nerder（大圆点分隔，两侧留距）
                using (Font tf = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold))
                using (SolidBrush bm = new SolidBrush(Ui.TextMain))
                using (SolidBrush bs = new SolidBrush(Ui.TextSub))
                using (SolidBrush bd = new SolidBrush(Ui.Accent))
                {
                    System.Drawing.StringFormat sf = System.Drawing.StringFormat.GenericTypographic;
                    float x = 56F;
                    float th = g.MeasureString("中", tf, System.Drawing.PointF.Empty, sf).Height;
                    float y = (54F - th) / 2F;
                    g.DrawString("桌面图标美化工具", tf, bm, x, y, sf);
                    x += g.MeasureString("桌面图标美化工具", tf, System.Drawing.PointF.Empty, sf).Width + 8F;
                    g.FillEllipse(bd, x, 27F - 4F, 8F, 8F);
                    x += 8F + 8F;
                    g.DrawString("By Nerder", tf, bs, x, y, sf);
                }

                using (Pen pen = new Pen(Ui.Border, 1F))
                    g.DrawLine(pen, 0, 53, 520, 53);
            };
            _restyle.Add(delegate
            {
                _titleBar.BackColor = Ui.Card;
                _titleBar.Invalidate();
            });

            TitleButton btnMin = new TitleButton { Kind = "min", Location = new Point(520 - 92, 0), Size = new Size(46, 53) };
            TitleButton btnClose = new TitleButton { Kind = "close", Location = new Point(520 - 46, 0), Size = new Size(46, 53) };
            btnMin.Click += delegate { WindowState = FormWindowState.Minimized; };
            btnClose.Click += delegate { Close(); };
            _titleBar.Controls.Add(btnMin);
            _titleBar.Controls.Add(btnClose);
            Controls.Add(_titleBar);

            // ---------- 状态卡片：小箭头 + 后缀 + UAC 小盾牌 + 右键菜单 ----------
            _statusCard.Location = new Point(24, 66);
            _statusCard.Size = new Size(472, 186);
            _statusCard.Paint += delegate(object s, PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                DrawStatusDot(g, 23.5F, 21F, _dotArrow);
                DrawStatusDot(g, 23.5F, 65F, _dotSuffix);
                DrawStatusDot(g, 23.5F, 109F, _dotShield);
                DrawStatusDot(g, 23.5F, 153F, _dotMenu);
            };
            AddStatusTitle(_arrowState, 11);
            AddStatusLabel(_arrowDetail, 40, 30, 7.75F, false);
            AddStatusTitle(_suffixState, 55);
            AddStatusLabel(_suffixDetail, 40, 74, 7.75F, false);
            AddStatusTitle(_shieldState, 99);
            AddStatusLabel(_shieldDetail, 40, 118, 7.75F, false);
            AddStatusTitle(_menuState, 143);
            AddStatusLabel(_menuDetail, 40, 162, 7.75F, false);
            AddBadge(_arrowBadge, 11);
            AddBadge(_suffixBadge, 55);
            AddBadge(_shieldBadge, 99);
            AddBadge(_menuBadge, 143);
            _restyle.Add(delegate
            {
                _arrowState.ForeColor = Ui.TextMain;
                _suffixState.ForeColor = Ui.TextMain;
                _shieldState.ForeColor = Ui.TextMain;
                _menuState.ForeColor = Ui.TextMain;
                _arrowDetail.ForeColor = Ui.TextSub;
                _suffixDetail.ForeColor = Ui.TextSub;
                _shieldDetail.ForeColor = Ui.TextSub;
                _menuDetail.ForeColor = Ui.TextSub;
                _statusCard.Invalidate();
            });
            Controls.Add(_statusCard);

            // ---------- 隐藏快捷方式小箭头（蓝色分区，四按钮一行） ----------
            _secArrow = SectionLabel("隐藏快捷方式小箭头", 274, Ui.Accent);
            Controls.Add(_secArrow);
            _restyle.Add(delegate { _secArrow.ForeColor = Ui.Accent; });
            _a1 = AddButton(0, 300, 112, 24, "方案一（推荐）", false, delegate
            {
                RunOp("隐藏小箭头 · 方案一", 0, delegate { SetArrowValue(EnvRoot() + @"\explorer.exe,-264", false); },
                    "已隐藏小箭头（方案一）。\n\n请看桌面快捷方式：应当没有小箭头，也没有黑块或阴影。\n若图标未变化，请注销再登录或重启电脑。");
            });
            _a2 = AddButton(1, 300, 112, 144, "方案二", true, delegate
            {
                RunOp("隐藏小箭头 · 方案二", 0, delegate { SetArrowValue(EnvRoot() + @"\System32\Taskbar.dll,-264", false); },
                    "已隐藏小箭头（方案二）。\n\n请看桌面快捷方式：应当没有小箭头，也没有黑块或阴影。\n若图标未变化，请注销再登录或重启电脑。");
            });
            _a3 = AddButton(2, 300, 112, 264, "方案三（备选）", true, delegate
            {
                RunOp("隐藏小箭头 · 方案三", 0, delegate { SetArrowValue(EnvRoot() + @"\blank.ico,0", true); },
                    "已隐藏小箭头（方案三）。\n\n请看桌面快捷方式：应当没有小箭头，也没有黑块或阴影。\n若图标未变化，请注销再登录或重启电脑。");
            });
            _a4 = AddButton(3, 300, 112, 384, "自定义方案", true, delegate
            {
                MessageBox.Show(this, BuildCustomInfo(ReadShellIcons("29"), "29",
                    "点击方案一 ~ 方案三 可隐藏为对应方案。"), "自定义方案", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

            // ---------- 「- 快捷方式」后缀（蓝色分区） ----------
            _secSuffix = SectionLabel("隐藏「- 快捷方式」后缀", 366, Ui.Accent);
            Controls.Add(_secSuffix);
            _restyle.Add(delegate { _secSuffix.ForeColor = Ui.Accent; });
            RoundedButton bSuffixHide = AddButton(4, 392, 232, 24, "方案一（推荐）", true, delegate
            {
                RunOp("隐藏「- 快捷方式」后缀 · 方案一", 1, HideSuffix,
                    "已隐藏「- 快捷方式」后缀（Link 注册表方式）。\n\n注意：该设置只对【新建】的快捷方式生效，\n已有快捷方式的名称不会自动改变。新建一个快捷方式试试效果。");
            });
            RoundedButton bSuffixCustom = AddButton(5, 392, 232, 264, "自定义方案", true, delegate
            {
                MessageBox.Show(this, BuildSuffixCustomInfo(), "自定义方案", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

            // ---------- 隐藏UAC小盾牌（#307DF1 蓝，四按钮一行） ----------
            _secShield = SectionLabel("隐藏UAC小盾牌", 458, Ui.Teal);
            Controls.Add(_secShield);
            _restyle.Add(delegate { _secShield.ForeColor = Ui.Teal; });
            _s1 = AddButton(6, 484, 112, 24, "方案一（推荐）", false, delegate
            {
                RunOp("隐藏 UAC 小盾牌 · 方案一", 2, delegate { HideShield(EnvRoot() + @"\blank.ico,0", true); },
                    "已隐藏 UAC 小盾牌（方案一 · blank.ico）。\n\n使用完全透明的 blank.ico 作为覆盖图标。\n这只是视觉隐藏，不影响任何程序权限。\n若图标未变化，请注销再登录或重启电脑。");
            });
            _s2 = AddButton(7, 484, 112, 144, "方案二", true, delegate
            {
                RunOp("隐藏 UAC 小盾牌 · 方案二", 2, delegate { HideShield(EnvRoot() + @"\System32\shell32.dll,-50", false); },
                    "已隐藏 UAC 小盾牌（方案二 · shell32.dll,-50）。\n\n这只是视觉隐藏：程序的管理员权限与 UAC 弹窗行为完全不受影响。\n若图标未变化，请注销再登录或重启电脑。");
            });
            _s3 = AddButton(8, 484, 112, 264, "方案三（备选）", true, delegate
            {
                RunOp("隐藏 UAC 小盾牌 · 方案三", 2, delegate { HideShield(EnvRoot() + @"\explorer.exe,-264", false); },
                    "已隐藏 UAC 小盾牌（方案三 · explorer.exe,-264）。\n\n这只是视觉隐藏：程序的管理员权限与 UAC 弹窗行为完全不受影响。\n若图标未变化，请注销再登录或重启电脑。");
            });
            _s4 = AddButton(9, 484, 112, 384, "自定义方案", true, delegate
            {
                MessageBox.Show(this, BuildCustomInfo(ReadShellIcons("77"), "77",
                    "点击方案一 ~ 方案三 可隐藏为对应方案。"), "自定义方案", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });

            // ---------- 恢复系统默认 ----------
            _secRestore = SectionLabel("恢复系统默认", 550, Ui.TextSub);
            Controls.Add(_secRestore);
            _restyle.Add(delegate { _secRestore.ForeColor = Ui.TextSub; });
            RoundedButton bResArrow = AddButton(10, 576, 152, 24, "恢复默认小箭头", true, delegate
            {
                RunOp("恢复默认小箭头", 0, RestoreArrow,
                    "已恢复系统默认。\n\n快捷方式的小箭头应该回来了。\n若未恢复，重启一次电脑即可。");
            });
            RoundedButton bResSuffix = AddButton(15, 576, 152, 184, "恢复默认快捷方式后缀", true, delegate
            {
                RunOp("恢复默认快捷方式后缀", 1, RestoreSuffix,
                    "已恢复默认命名（已删除 Link 与 Naming Templates 设置）。\n\n新建快捷方式将重新显示「- 快捷方式」后缀。");
            });
            RoundedButton bResShield = AddButton(11, 576, 152, 344, "恢复默认 UAC 盾牌", true, delegate
            {
                RunOp("恢复默认 UAC 盾牌", 2, RestoreShield,
                    "已恢复系统默认 UAC 盾牌。\n\n资源管理器会还原原生盾牌图标。\n若未恢复，重启一次电脑即可。");
            });

            // ---------- 系统实用工具（紫/粉） ----------
            _secTool = SectionLabel("系统实用工具", 642, Ui.TextSub);
            Controls.Add(_secTool);
            _restyle.Add(delegate { _secTool.ForeColor = Ui.TextSub; });
            RoundedButton bMenuClassic = AddButton(13, 668, 232, 24, "Win10 经典右键菜单", true, delegate
            {
                RunOp("启用 Win10 经典右键菜单", 3, ClassicContextMenu,
                    "已切换为 Win10 经典全功能右键菜单。\n\n资源管理器已重启，直接在文件/桌面右键即可看到效果。\n随时可点击旁边的按钮恢复 Win11 新版菜单。");
            });
            RoundedButton bMenuDefault = AddButton(14, 668, 232, 264, "Win11 新版右键菜单", true, delegate
            {
                RunOp("恢复 Win11 新版右键菜单", 3, DefaultContextMenu,
                    "已恢复 Win11 新版默认右键菜单。\n\n资源管理器已重启，直接在文件/桌面右键即可看到效果。");
            });
            RoundedButton bRebuild = AddButton(12, 720, 472, 24, "重建桌面图标缓存", false, delegate
            {
                RunOp("重建桌面图标缓存", -1, RebuildCache,
                    "图标缓存已重建，资源管理器已重启。");
            });
            bRebuild.Font = new Font("Microsoft YaHei UI", 11.75F, FontStyle.Bold);

            // ---------- 底部说明 ----------
            Label subLine = new Label
            {
                Text = "快捷方式小箭头 / UAC 小盾牌 / 右键菜单 · 一键管理",
                Location = new Point(0, 786),
                Size = new Size(520, 18),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Ui.TextMain,
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                BackColor = Color.Transparent,
            };
            SpacedLabel footer = new SpacedLabel
            {
                Text = "操作完成后自动重建图标缓存并重启资源管理器 · 桌面立即生效",
                Location = new Point(0, 807),
                Size = new Size(520, 15),
                ForeColor = Ui.TextSub,
                Align = ContentAlignment.MiddleCenter,
            };
            Label versionLine = new Label
            {
                Text = "v" + AppInfo.Version,
                Location = new Point(0, 825),
                Size = new Size(520, 14),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Ui.TextSub,
                Font = new Font("Microsoft YaHei UI", 7.5F),
                BackColor = Color.Transparent,
            };
            _restyle.Add(delegate
            {
                subLine.ForeColor = Ui.TextMain;
                footer.ForeColor = Ui.TextSub;
            });
            Controls.Add(versionLine);
            Controls.Add(subLine);
            Controls.Add(footer);

            StyleButtons();

            // 轮询系统深色模式设置，切换主题后 2 秒内自动换肤
            System.Windows.Forms.Timer themeTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            themeTimer.Tick += delegate
            {
                bool d = Ui.IsDarkMode();
                if (d != _lastDark)
                {
                    _lastDark = d;
                    Ui.Apply(d);
                    Restyle();
                }
            };
            themeTimer.Start();
        }

        protected override void WndProc(ref Message m)
        {
            // 系统切换日间/夜间主题时实时换肤
            if (m.Msg == 0x001A) { Ui.Apply(Ui.IsDarkMode()); Restyle(); }
            base.WndProc(ref m);
        }

        private void Restyle()
        {
            foreach (Action a in _restyle) a();
            StyleButtons();
            BackColor = Ui.Bg;
            Invalidate(true);
        }

        private static string EnvRoot()
        {
            return Environment.GetEnvironmentVariable("SystemRoot");
        }

        private void AddStatusLabel(Label l, int x, int y, float size, bool bold)
        {
            l.Location = new Point(x, y);
            l.Size = new Size(420, bold ? 20 : 15);
            l.Font = new Font("Microsoft YaHei UI", size, bold ? FontStyle.Bold : FontStyle.Regular);
            l.ForeColor = bold ? Ui.TextMain : Ui.TextSub;
            l.BackColor = Color.Transparent;
            l.AutoEllipsis = true;
            _statusCard.Controls.Add(l);
        }

        // 状态行名：加粗 + 字间距
        private void AddStatusTitle(SpacedLabel l, int y)
        {
            l.Location = new Point(40, y);
            l.Size = new Size(250, 20);
            l.Font = new Font("Microsoft YaHei UI", 9.25F, FontStyle.Bold);
            l.ForeColor = Ui.TextMain;
            _statusCard.Controls.Add(l);
        }

        // 状态徽标：显示在标题行右侧，颜色随状态（绿=已修改，灰=默认），带字间距
        private void AddBadge(SpacedLabel l, int y)
        {
            l.Location = new Point(296, y);
            l.Size = new Size(162, 20);
            l.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            l.ForeColor = Ui.Gray;
            _statusCard.Controls.Add(l);
        }

        // 彩色加粗状态点：外圈柔光 + 实心核心
        private static void DrawStatusDot(Graphics g, float cx, float cy, Color color)
        {
            using (SolidBrush halo = new SolidBrush(Color.FromArgb(45, color)))
                g.FillEllipse(halo, cx - 7.5F, cy - 7.5F, 15F, 15F);
            using (SolidBrush core = new SolidBrush(color))
                g.FillEllipse(core, cx - 4.5F, cy - 4.5F, 9F, 9F);
        }

        private Label SectionLabel(string text, int y, Color color)
        {
            return new Label
            {
                Text = text,
                Location = new Point(24, y),
                Size = new Size(280, 20),
                Font = new Font("Microsoft YaHei UI", 9.25F, FontStyle.Bold),
                ForeColor = color,
                BackColor = Color.Transparent,
            };
        }

        private RoundedButton AddButton(int idx, int y, int w, int x, string text, bool outline, EventHandler onClick)
        {
            RoundedButton b = new RoundedButton
            {
                Text = text,
                Size = new Size(w, 44),
                Location = new Point(x, y),
                Outline = outline,
            };
            b.Click += onClick;
            _btns[idx] = b;
            Controls.Add(b);
            return b;
        }

        // 按功能分区着色（换肤时同样从这里取色）
        private void StyleButtons()
        {
            // 两个隐藏分区的方案按钮：统一渐变 #c850c0 → #4158d0
            foreach (RoundedButton b in new RoundedButton[] { _a1, _a2, _a3, _a4 })
            {
                if (b == null) continue;
                b.FillColor = Ui.GradFrom;
                b.GradTo = Ui.GradTo;
                b.Gradient = true;
                b.OutlineColor = Ui.Accent;
            }
            foreach (RoundedButton b in new RoundedButton[] { _s1, _s2, _s3, _s4 })
            {
                if (b == null) continue;
                b.FillColor = Ui.GradFrom;
                b.GradTo = Ui.GradTo;
                b.Gradient = true;
                b.OutlineColor = Ui.Teal;
            }
            // 后缀：与箭头分区同风格
            if (_btns[4] != null)
            {
                _btns[4].FillColor = Ui.GradFrom;
                _btns[4].GradTo = Ui.GradTo;
                _btns[4].Gradient = true;
                _btns[4].OutlineColor = Ui.Accent;
            }
            if (_btns[5] != null) _btns[5].OutlineColor = Ui.Accent;
            if (_btns[15] != null) _btns[15].OutlineColor = Ui.Accent;
            // 恢复默认：对应分区色描边
            if (_btns[10] != null) _btns[10].OutlineColor = Ui.Accent;
            if (_btns[11] != null) _btns[11].OutlineColor = Ui.Teal;
            // 重建图标缓存：渐变 #f6d365 → #fda085，浅底深色字
            if (_btns[12] != null)
            {
                _btns[12].FillColor = Ui.Rebuild;
                _btns[12].GradTo = Ui.RebuildHv;
                _btns[12].Gradient = true;
                _btns[12].DarkText = true;
            }
            // 右键菜单：粉渐变 #ff9a9e → #fad0c4（两个按钮随生效状态互斥高亮）
            foreach (RoundedButton b in new RoundedButton[] { _btns[13], _btns[14] })
            {
                if (b == null) continue;
                b.FillColor = Ui.MenuFrom;
                b.GradTo = Ui.MenuTo;
                b.Gradient = true;
                b.DarkText = true;
                b.OutlineColor = Ui.MenuFrom;
            }
            // 描边按钮的悬停底色跟随主题
            foreach (RoundedButton b in _btns)
            {
                if (b == null) continue;
                b.OutlineHover = Ui.OutlineHover;
                b.OutlineDown = Ui.OutlineDown;
                b.Invalidate();
            }
        }

        // ------------------------- 状态显示 -------------------------
        private static string ReadShellIcons(string name)
        {
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(RK))
                {
                    if (k == null) return null;
                    return k.GetValue(name) as string;
                }
            }
            catch { return null; }
        }

        private static bool Has(string value, string key)
        {
            return value != null && value.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void MarkScheme(RoundedButton b, bool active)
        {
            if (b.Outline == active) { b.Outline = !active; b.Invalidate(); }
        }

        private void RefreshStatus()
        {
            string v29 = ReadShellIcons("29");
            _arrowState.Text = "快捷方式小箭头";
            if (!string.IsNullOrEmpty(v29))
            {
                string scheme = Has(v29, "explorer.exe,-264") ? "方案一"
                    : Has(v29, "Taskbar.dll,-264") ? "方案二"
                    : Has(v29, "blank.ico") ? "方案三" : "自定义";
                _dotArrow = Ui.Green;
                _arrowBadge.Text = "已隐藏 · " + scheme;
                _arrowBadge.ForeColor = Ui.Green;
                _arrowDetail.Text = "Shell Icons \\29 = " + v29;
            }
            else
            {
                _dotArrow = Ui.Gray;
                _arrowBadge.Text = "默认";
                _arrowBadge.ForeColor = Ui.TextSub;
                _arrowDetail.Text = "尚未修改（HKLM\\SOFTWARE\\...\\Explorer\\Shell Icons\\29）";
            }

            byte[] linkVal = ReadLinkValue();
            string suffixNt = ReadNamingTemplate();
            _suffixState.Text = "- 快捷方式 后缀";
            if (IsZeroDword(linkVal) || !string.IsNullOrEmpty(suffixNt))
            {
                _dotSuffix = Ui.Green;
                _suffixBadge.Text = "已隐藏 · " + (IsZeroDword(linkVal) ? "方案一" : "自定义");
                _suffixBadge.ForeColor = Ui.Green;
                _suffixDetail.Text = linkVal != null
                    ? "Explorer\\Link = " + BitConverter.ToString(linkVal) + "（方案一 · 对新建快捷方式生效）"
                    : "Naming Templates\\Shortcut = " + suffixNt + "（对新建快捷方式生效）";
            }
            else
            {
                _dotSuffix = Ui.Gray;
                _suffixBadge.Text = "默认";
                _suffixBadge.ForeColor = Ui.TextSub;
                _suffixDetail.Text = "尚未修改 · 新建快捷方式仍会自动加上后缀";
            }
            // 后缀方案一：只有识别到已隐藏才高亮
            MarkScheme(_btns[4], IsZeroDword(linkVal) || !string.IsNullOrEmpty(suffixNt));

            string v77 = ReadShellIcons("77");
            _shieldState.Text = "UAC 小盾牌";
            if (!string.IsNullOrEmpty(v77))
            {
                string scheme = Has(v77, "blank.ico") ? "方案一"
                    : Has(v77, "shell32.dll,-50") ? "方案二"
                    : Has(v77, "explorer.exe,-264") ? "方案三" : "自定义";
                _dotShield = Ui.Green;
                _shieldBadge.Text = "已隐藏 · " + scheme;
                _shieldBadge.ForeColor = Ui.Green;
                _shieldDetail.Text = "Shell Icons \\77 = " + v77;
            }
            else
            {
                _dotShield = Ui.Gray;
                _shieldBadge.Text = "默认";
                _shieldBadge.ForeColor = Ui.TextSub;
                _shieldDetail.Text = "尚未修改（仅隐藏外观，程序权限不受任何影响）";
            }

            // 右键菜单样式：CLSID {86ca1aa0-...} 存在于 HKCU 或 HKLM → Win10 经典，否则 Win11 新版（默认）
            bool cuMenu = MenuKeyExistsIn(Registry.CurrentUser);
            bool lmMenu = MenuKeyExistsIn(Registry.LocalMachine);
            bool classicMenu = cuMenu || lmMenu;
            _menuState.Text = "右键菜单样式";
            if (classicMenu)
            {
                _dotMenu = Ui.Green;
                _menuBadge.Text = "Win10 经典";
                _menuBadge.ForeColor = Ui.Green;
                _menuDetail.Text = "CLSID 键存在于 "
                    + (cuMenu && lmMenu ? "HKCU + HKLM" : cuMenu ? "HKCU" : "HKLM")
                    + "（可能由其它工具写入）";
            }
            else
            {
                _dotMenu = Ui.Green;
                _menuBadge.Text = "Win11 新版";
                _menuBadge.ForeColor = Ui.Green;
                _menuDetail.Text = "CLSID 键不存在 · 系统默认上下文菜单";
            }
            // 哪个生效哪个亮：Win10 经典生效亮 Win10 按钮，Win11 新版生效亮 Win11 按钮
            MarkScheme(_btns[13], classicMenu);
            MarkScheme(_btns[14], !classicMenu);

            // 当前生效方案高亮（实心），未选中方案保持默认描边
            MarkScheme(_a1, Has(v29, "explorer.exe,-264"));
            MarkScheme(_a2, Has(v29, "Taskbar.dll,-264"));
            MarkScheme(_a3, Has(v29, "blank.ico"));
            MarkScheme(_a4, v29 != null && !Has(v29, "explorer.exe,-264")
                         && !Has(v29, "Taskbar.dll,-264") && !Has(v29, "blank.ico"));
            MarkScheme(_s1, Has(v77, "blank.ico"));
            MarkScheme(_s2, Has(v77, "shell32.dll,-50"));
            MarkScheme(_s3, v77 != null && Has(v77, "explorer.exe,-264"));
            MarkScheme(_s4, v77 != null && !Has(v77, "blank.ico")
                         && !Has(v77, "shell32.dll,-50") && !Has(v77, "explorer.exe,-264"));

            _statusCard.Invalidate();
        }

        // ------------------------- 操作 -------------------------
        // row：本次操作影响的状态卡行（0=小箭头 1=后缀 2=UAC盾牌 3=右键菜单 -1=不对应任何行）
        private void RunOp(string title, int row, Action work, string doneMsg)
        {
            if (_busy) return;
            _busy = true;
            _busyRow = row;
            _lastLocked = false;
            SetBusy(true);
            ThreadPool.QueueUserWorkItem(delegate
            {
                string msg = doneMsg;
                bool ok = true;
                try { work(); }
                catch (Exception ex)
                {
                    ok = false;
                    msg = "操作失败：" + ex.Message;
                }
                if (_lastLocked)
                    msg += "\n\n注意：有图标缓存文件被其它进程占用，未能删除。\n界面可能暂时显示旧图标，请【注销】或【重启】一次。";
                string finalMsg = msg;
                bool finalOk = ok;
                try
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        _busy = false;
                        SetBusy(false);
                        RefreshStatus();
                        MessageBox.Show(this, finalMsg, finalOk ? "完成" : "操作失败",
                            MessageBoxButtons.OK, finalOk ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    });
                }
                catch { }
            });
        }

        private void SetBusy(bool busy)
        {
            foreach (RoundedButton b in _btns)
                if (b != null) b.Enabled = !busy;
            if (busy)
            {
                // 只让受影响的状态行进入执行态（琥珀点 + 徽标），其它行信息原样保留
                SpacedLabel badge = null;
                if (_busyRow == 0) { _dotArrow = Ui.Amber; badge = _arrowBadge; }
                else if (_busyRow == 1) { _dotSuffix = Ui.Amber; badge = _suffixBadge; }
                else if (_busyRow == 2) { _dotShield = Ui.Amber; badge = _shieldBadge; }
                else if (_busyRow == 3) { _dotMenu = Ui.Amber; badge = _menuBadge; }
                if (badge != null)
                {
                    badge.Text = "正在执行…";
                    badge.ForeColor = Ui.Amber;
                }
            }
            _statusCard.Invalidate();
        }

        // ------------------------- 「- 快捷方式」后缀 -------------------------
        // Link = 00 00 00 00（REG_BINARY）→ 新建快捷方式不再自动加「- 快捷方式」后缀
        // 恢复默认 = 删除 Link 值（等同系统默认；也可设回 hex:1e,00,00,00，效果一致）
        private void HideSuffix()
        {
            using (RegistryKey k = Registry.CurrentUser.CreateSubKey(EK))
                k.SetValue("Link", new byte[] { 0, 0, 0, 0 }, RegistryValueKind.Binary);
        }

        private void RestoreSuffix()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(EK, true))
                    if (k != null) k.DeleteValue("Link", false);
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(NK, true))
                    if (k != null) k.DeleteValue("Shortcut", false);
            }
            catch { }
        }

        private static byte[] ReadLinkValue()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(EK))
                    if (k != null) return k.GetValue("Link") as byte[];
            }
            catch { }
            return null;
        }

        private static string ReadNamingTemplate()
        {
            try
            {
                using (RegistryKey k = Registry.CurrentUser.OpenSubKey(NK))
                    if (k != null) return k.GetValue("Shortcut") as string;
            }
            catch { }
            return null;
        }

        private static bool IsZeroDword(byte[] b)
        {
            return b != null && b.Length == 4 && b[0] == 0 && b[1] == 0 && b[2] == 0 && b[3] == 0;
        }

        // 自定义方案弹窗（后缀）：显示 Link / Naming Templates 的具体参数
        private string BuildSuffixCustomInfo()
        {
            byte[] link = ReadLinkValue();
            string nt = ReadNamingTemplate();
            if (link == null && nt == null)
                return "当前未修改注册表，新建快捷方式仍会显示「- 快捷方式」后缀。\n\n点击「方案一」可隐藏后缀。";
            string detail = "";
            if (link != null)
                detail += "Explorer\\Link = " + BitConverter.ToString(link)
                    + (IsZeroDword(link) ? "（方案一 · 隐藏后缀）" : "（自定义值）") + "\n";
            if (nt != null)
                detail += "Naming Templates\\Shortcut = " + nt + "（自定义值）\n";
            return "当前注册表参数：\n\n" + detail
                + "\n点击「方案一」可隐藏后缀；「恢复系统默认 → 恢复默认快捷方式后缀」可还原。";
        }

        // ------------------------- 公共 -------------------------
        // 自定义方案弹窗：不在预设中 → 显示参数；在预设中 → 说明当前生效的是哪个方案
        private string BuildCustomInfo(string v, string valueName, string tail)
        {
            if (v == null)
                return "当前未修改注册表，为系统默认显示。\n\n" + tail;
            string preset = null;
            if (Has(v, "explorer.exe,-264")) preset = "方案一";
            else if (Has(v, "Taskbar.dll,-264")) preset = "方案二";
            else if (Has(v, "blank.ico")) preset = "方案三";
            if (preset != null)
                return "当前生效的是" + preset + "（不属于自定义方案）：\n\nShell Icons \\" + valueName + " = " + v
                    + "\n\n点击对应方案按钮可切换；只有使用其它工具设置过图标时，这里才会显示自定义参数。";
            return "当前自定义方案参数（可能由其它工具设置）：\n\nShell Icons \\" + valueName + " = " + v
                + "\n\n" + tail;
        }

        private void ExtractBlankIco()
        {
            string dst = Path.Combine(EnvRoot(), "blank.ico");
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("blank.ico"))
            {
                if (s == null) throw new IOException("程序内未找到 blank.ico 资源。");
                using (FileStream f = new FileStream(dst, FileMode.Create, FileAccess.Write))
                {
                    byte[] buf = new byte[4096];
                    int n;
                    while ((n = s.Read(buf, 0, buf.Length)) > 0) f.Write(buf, 0, n);
                }
            }
        }

        // ------------------------- 小箭头 -------------------------
        private void SetArrowValue(string value, bool deployIco)
        {
            string root = EnvRoot();
            if (deployIco)
            {
                ExtractBlankIco();
            }
            using (RegistryKey k = Registry.LocalMachine.CreateSubKey(RK))
                k.SetValue("29", value, RegistryValueKind.String);
            DeleteShellIconValue(RK32, "29");
            if (!deployIco)
            {
                // 盾牌方案一正在使用 blank.ico 时不能删，否则会把盾牌弄坏
                if (!ShieldUsesBlank()) TryDelFile(Path.Combine(root, "blank.ico"));
                TryDelFile(Path.Combine(root, "blank-alpha0.ico"));
            }
            RebuildCache();
        }

        private void RestoreArrow()
        {
            DeleteShellIconValue(RK, "29");
            DeleteShellIconValue(RK32, "29");
            string root = EnvRoot();
            if (!ShieldUsesBlank()) TryDelFile(Path.Combine(root, "blank.ico"));
            TryDelFile(Path.Combine(root, "blank-alpha0.ico"));
            RebuildCache();
        }

        private bool ShieldUsesBlank()
        {
            string v77 = ReadShellIcons("77");
            return v77 != null && Has(v77, "blank.ico");
        }

        // ------------------------- UAC 小盾牌 -------------------------
        private void HideShield(string value, bool deployIco)
        {
            if (deployIco) ExtractBlankIco();
            using (RegistryKey k = Registry.LocalMachine.CreateSubKey(RK))
                k.SetValue("77", value, RegistryValueKind.String);
            DeleteShellIconValue(RK32, "77");
            RebuildCache();
        }

        private void RestoreShield()
        {
            DeleteShellIconValue(RK, "77");
            DeleteShellIconValue(RK32, "77");
            RebuildCache();
        }

        // ------------------------- Win11 右键菜单 -------------------------
        // CLSID {86ca1aa0-...} 存在且默认值为空 → Win10 经典全功能右键菜单
        private void ClassicContextMenu()
        {
            SwitchContextMenu(delegate
            {
                using (RegistryKey k = Registry.CurrentUser.CreateSubKey(
                    @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"))
                    k.SetValue(null, "", RegistryValueKind.String);
            });
        }

        private void DefaultContextMenu()
        {
            SwitchContextMenu(delegate
            {
                // 同时清理 HKCU 与 HKLM（部分工具会把键写到 HKLM；HKCR 是两者合并视图，漏删则菜单不变）
                TryDeleteMenuKey();
                if (MenuKeyExists())
                {
                    Thread.Sleep(300);
                    TryDeleteMenuKey();
                }
                if (MenuKeyExists())
                    throw new IOException("CLSID 键删除失败，请手动执行：reg delete \"HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\" /f 与 reg delete \"HKLM\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\" /f");
            });
        }

        // 先结束资源管理器 → 停止期间修改注册表 → 再拉起：
        // Explorer 启动时必然按新键值加载菜单样式，避免“删了键菜单却没变”的缓存问题
        private void SwitchContextMenu(Action regAction)
        {
            StartWatchdog();
            foreach (Process p in Process.GetProcessesByName("explorer"))
            {
                try { p.Kill(); } catch { }
            }
            Thread.Sleep(1500);
            regAction();
            Thread.Sleep(500);
            try { Process.Start(Path.Combine(EnvRoot(), "explorer.exe")); } catch { }
            Thread.Sleep(3000);
            if (ExplorerCount() == 0)
            {
                try { Process.Start(Path.Combine(EnvRoot(), "explorer.exe")); } catch { }
                Thread.Sleep(3000);
            }
            if (ExplorerCount() == 0)
                throw new IOException("资源管理器未能自动启动：请按 Ctrl+Shift+Esc → 文件 → 运行新任务 → 输入 explorer.exe");
        }

        private static bool MenuKeyExists()
        {
            return MenuKeyExistsIn(Registry.CurrentUser) || MenuKeyExistsIn(Registry.LocalMachine);
        }

        private static bool MenuKeyExistsIn(RegistryKey root)
        {
            try
            {
                using (RegistryKey k = root.OpenSubKey(MENU_CLSID))
                    return k != null;
            }
            catch { return false; }
        }

        // 返回 null = 清理完成；否则返回失败原因
        private static string TryDeleteMenuKey()
        {
            // HKCU 与 HKLM 都要清理（HKCR 为合并视图，任一存在都会显示经典菜单）
            foreach (RegistryKey root in new RegistryKey[] { Registry.CurrentUser, Registry.LocalMachine })
            {
                try
                {
                    using (RegistryKey k = root.OpenSubKey(MENU_CLSID, true))
                    {
                        if (k == null) continue;
                        foreach (string sub in k.GetSubKeyNames())
                        {
                            try { k.DeleteSubKeyTree(sub, false); } catch { }
                        }
                    }
                    root.DeleteSubKeyTree(MENU_CLSID, false);
                }
                catch { }
                // HKLM 的键通常被 TrustedInstaller 持有（Administrators 只有读权限），
                // 普通删除被拒时：启用特权 → 接管所有权 → 授权完全控制 → 强制删除
                if (MenuKeyExistsIn(root))
                {
                    string err = null;
                    try { ForceDeleteMenuKeyIn(root); }
                    catch (Exception ex) { err = ex.Message; }
                    if (MenuKeyExistsIn(root))
                        return (root.Name) + " 下的 CLSID 键强删失败" + (err == null ? "" : "：" + err);
                }
            }
            return null;
        }

        private static void ForceDeleteMenuKeyIn(RegistryKey root)
        {
            EnablePrivilege("SeTakeOwnershipPrivilege");
            EnablePrivilege("SeRestorePrivilege");
            ForceDeleteKeyRecursive(root, MENU_CLSID, 0);
        }

        // 递归强删：对每一层先接管所有权（TrustedInstaller 持有的键，管理员默认只读），
        // 再授予 Administrators 完全控制，然后自底向上删除
        private static void ForceDeleteKeyRecursive(RegistryKey root, string path, int depth)
        {
            if (depth > 8) return;

            // 1) 接管键所有权
            using (RegistryKey k = root.OpenSubKey(path,
                RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership))
            {
                if (k == null) return;
                RegistrySecurity rs = k.GetAccessControl(AccessControlSections.Owner);
                rs.SetOwner(new NTAccount("BUILTIN", "Administrators"));
                k.SetAccessControl(rs);
            }
            // 2) 授予 Administrators 完全控制（子键继承）
            using (RegistryKey k = root.OpenSubKey(path,
                RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ChangePermissions))
            {
                if (k == null) return;
                RegistrySecurity rs = k.GetAccessControl(AccessControlSections.Access);
                rs.AddAccessRule(new RegistryAccessRule(
                    new NTAccount("BUILTIN", "Administrators"), RegistryRights.FullControl,
                    InheritanceFlags.ContainerInherit, PropagationFlags.None,
                    AccessControlType.Allow));
                k.SetAccessControl(rs);
            }
            // 3) 先递归清理子键（子键可能有独立的 TrustedInstaller ACL）
            using (RegistryKey k = root.OpenSubKey(path, true))
            {
                if (k != null)
                    foreach (string sub in k.GetSubKeyNames())
                        ForceDeleteKeyRecursive(root, path + "\\" + sub, depth + 1);
            }
            // 4) 删除本键
            root.DeleteSubKey(path, false);
        }

        // ------------------------- 公共 -------------------------
        private void DeleteShellIconValue(string keyPath, string name)
        {
            try
            {
                using (RegistryKey k = Registry.LocalMachine.OpenSubKey(keyPath, true))
                    if (k != null) k.DeleteValue(name, false);
            }
            catch { }
        }

        private void TryDelFile(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { }
        }

        private void RebuildCache()
        {
            StartWatchdog();
            foreach (Process p in Process.GetProcessesByName("explorer"))
            {
                try { p.Kill(); } catch { }
            }
            Thread.Sleep(2500);

            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            bool locked = false;
            TryDelCacheFile(Path.Combine(local, "IconCache.db"), ref locked);
            string dir = Path.Combine(local, @"Microsoft\Windows\Explorer");
            if (Directory.Exists(dir))
            {
                foreach (string f in Directory.GetFiles(dir, "iconcache_*.db"))
                    TryDelCacheFile(f, ref locked);
            }
            _lastLocked = locked;

            Thread.Sleep(5000);
            if (ExplorerCount() == 0)
            {
                try { Process.Start(Path.Combine(EnvRoot(), "explorer.exe")); } catch { }
                Thread.Sleep(5000);
            }
            if (ExplorerCount() == 0)
                throw new IOException("资源管理器未能自动启动：请按 Ctrl+Shift+Esc → 文件 → 运行新任务 → 输入 explorer.exe");
        }

        private void TryDelCacheFile(string path, ref bool locked)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { locked = true; }
        }

        // 看门狗：20 秒后确认 explorer 是否活着，没活就拉起来
        private void StartWatchdog()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                Thread.Sleep(20000);
                if (ExplorerCount() == 0)
                {
                    try
                    {
                        string exe = Path.Combine(EnvRoot(), "explorer.exe");
                        if (File.Exists(exe)) Process.Start(exe);
                    }
                    catch { }
                }
            });
        }

        private static int ExplorerCount()
        {
            return Process.GetProcessesByName("explorer").Length;
        }
    }
}
