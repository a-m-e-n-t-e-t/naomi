using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace _3D
{
    /// <summary>
    /// Обычная форма с GLControl: показывает вращающийся составной
    /// тетраэдр с логотипом Cyberdyne в окне.
    /// </summary>
    public partial class FM_3D : Form
    {
        private readonly TetrahedronScene _scene = new TetrahedronScene();
        private readonly Timer _timer = new Timer();
        private DateTime _lastFrame = DateTime.Now;
        private bool _glReady = false;

        public FM_3D()
        {
            InitializeComponent();
            this.Text = "3D — составной тетраэдр (Cyberdyne)";

            glControl.Dock = DockStyle.Fill;
            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;

            _timer.Interval = 16; // ~60 FPS
            _timer.Tick += Timer_Tick;
        }

        private void GlControl_Load(object sender, EventArgs e)
        {
            glControl.MakeCurrent();
            _scene.InitGL();
            _glReady = true;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!_glReady) return;
            DateTime now = DateTime.Now;
            double dt = (now - _lastFrame).TotalSeconds;
            if (dt > 0.1) dt = 0.1; // защита от больших скачков
            _lastFrame = now;

            _scene.Update(dt);
            glControl.Invalidate();
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!_glReady) return;
            glControl.MakeCurrent();
            _scene.Render(glControl.Width, glControl.Height, transparentBackground: false);
            glControl.SwapBuffers();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (!_glReady) return;
            glControl.Invalidate();
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
