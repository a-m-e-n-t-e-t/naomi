using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace _3D
{
    /// <summary>
    /// Прозрачный fullscreen-оверлей поверх рабочего стола с GLControl,
    /// на котором рендерится вращающийся составной тетраэдр с логотипом
    /// Cyberdyne.
    ///
    /// Особенности:
    ///   • FormBorderStyle.None — без рамки.
    ///   • Bounds = весь экран.
    ///   • TopMost = true — поверх всех окон.
    ///   • TransparencyKey = BackColor = Magenta — прозрачный фон.
    ///   • WS_EX_LAYERED | WS_EX_TRANSPARENT — клики проходят сквозь окно
    ///     к рабочему столу (можно работать с иконками).
    ///   • Двойной клик — закрыть оверлей (через глобальный хоткей,
    ///     т.к. клики проходят сквозь форму).
    ///   • Контекстное меню по правому клику в правом-нижнем углу.
    /// </summary>
    public class DesktopOverlay : Form
    {
        private readonly TetrahedronScene _scene = new TetrahedronScene();
        private readonly Timer _timer = new Timer();
        // Stopwatch вместо DateTime.Now: монотонные измерения без аллокаций
        // и без скачков при переводе системных часов.
        private readonly System.Diagnostics.Stopwatch _clock =
            System.Diagnostics.Stopwatch.StartNew();
        private bool _glReady = false;
        private readonly GLControl _glControl;

        // Win32 для click-through
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte alpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const uint LWA_COLORKEY = 0x00000001;

        public DesktopOverlay()
        {
            // Конфигурация формы
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;
            ShowInTaskbar = false;
            TransparencyKey = Color.Magenta;
            BackColor = Color.Magenta;
            Bounds = Screen.PrimaryScreen.Bounds;
            Name = "DesktopOverlay";
            Text = "3D Desktop Overlay";

            // GLControl с прозрачным фоном
            _glControl = new GLControl();
            _glControl.Dock = DockStyle.Fill;
            _glControl.BackColor = Color.Magenta; // = прозрачный через TransparencyKey
            _glControl.VSync = false;
            _glControl.Load += GlControl_Load;
            _glControl.Paint += GlControl_Paint;
            _glControl.Resize += (s, e) => { if (_glReady) _glControl.Invalidate(); };

            Controls.Add(_glControl);

            // Таймер анимации
            _timer.Interval = 16;
            _timer.Tick += Timer_Tick;

            // Закрытие по Escape — через фильтр сообщений (т.к. форма click-through,
            // обычные KeyPreview не работает). Используем контекстное меню.
            ContextMenu = new ContextMenu();
            ContextMenu.MenuItems.Add("Закрыть оверлей", (s, e) => Close());
            ContextMenu.MenuItems.Add("Переключить click-through", (s, e) => ToggleClickThrough());
            ContextMenu.MenuItems.Add("Переключить внутренние грани", (s, e) =>
            {
                _scene.ShowInternalFaces = !_scene.ShowInternalFaces;
            });
        }

        private bool _clickThrough = true;

        private void ToggleClickThrough()
        {
            _clickThrough = !_clickThrough;
            UpdateWindowStyle();
        }

        private void UpdateWindowStyle()
        {
            int ex = GetWindowLong(Handle, GWL_EXSTYLE);
            int newEx = WS_EX_LAYERED;
            if (_clickThrough) newEx |= WS_EX_TRANSPARENT;
            SetWindowLong(Handle, GWL_EXSTYLE, newEx);
            // Прозрачность по цвету-ключу (Magenta), без альфа-канала для всей формы
            SetLayeredWindowAttributes(Handle, (uint)ColorTranslator.ToWin32(Color.Magenta), 0, LWA_COLORKEY);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            UpdateWindowStyle();
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            _glControl.MakeCurrent();
            _scene.InitGL();
            _glReady = true;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_glReady) return;
            double dt = _clock.Elapsed.TotalSeconds;
            if (dt > 0.1) dt = 0.1;
            _clock.Restart();

            _scene.Update(dt);
            _glControl.Invalidate();
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!_glReady) return;
            _glControl.MakeCurrent();
            // transparentBackground = true → GL.ClearColor с альфа-0
            // Это работает в паре с TransparencyKey = Magenta:
            // GLControl.BackColor = Magenta → эти пиксели форма считает прозрачными.
            _scene.Render(_glControl.Width, _glControl.Height, transparentBackground: true);
            _glControl.SwapBuffers();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            _scene.Dispose();
            base.OnFormClosed(e);
        }
    }
}
