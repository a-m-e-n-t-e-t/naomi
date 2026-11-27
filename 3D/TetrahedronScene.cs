using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace _3D
{
    /// <summary>
    /// Сцена «составной правильный тетраэдр из 4 одинаковых треугольных
    /// пирамид» с логотипом Cyberdyne на внешних гранях.
    ///
    /// Свойства сцены:
    ///   • Правильный тетраэдр с длиной ребра 2, центр в (0,0,0).
    ///   • 4 пирамиды: каждая имеет 3 вершины из тетраэдра как основание
    ///     и центр (0,0,0) как вершину.
    ///   • Вращение: случайно выбранная ось + случайно выбранная скорость,
    ///     параметры плавно интерполируются к новым случайным значениям
    ///     каждые 2-5 секунд.
    ///   • Отдаление пирамид: каждая пирамида движется вдоль нормали своей
    ///     внешней грани (от центра тетраэдра наружу), амплитуда/частота/
    ///     фаза — случайные и периодически меняются.
    ///   • На каждой внешней грани натянута текстура логотипа Cyberdyne.
    /// </summary>
    internal class TetrahedronScene : IDisposable
    {
        // -----------------------------------------------------------------
        // Вершины правильного тетраэдра (длина ребра = 2, центр = 0,0,0).
        // -----------------------------------------------------------------
        private static readonly Vector3[] TetraVertices =
        {
            new Vector3( 1,  1,  1) * (1f / (float)Math.Sqrt(2)),  // V0
            new Vector3( 1, -1, -1) * (1f / (float)Math.Sqrt(2)),  // V1
            new Vector3(-1,  1, -1) * (1f / (float)Math.Sqrt(2)),  // V2
            new Vector3(-1, -1,  1) * (1f / (float)Math.Sqrt(2)),  // V3
        };

        /// <summary>
        /// Каждая из 4 пирамид описывается индексами 3 вершин основания
        /// (это вершины одной из 4 граней тетраэдра) и индексом «исключённой»
        /// вершины — в её направлении пирамида отдаляется от центра.
        /// </summary>
        private struct PyramidDef
        {
            public int A, B, C;        // индексы вершин основания (внешняя грань)
            public int OutVertex;      // индекс вершины, в направлении которой пирамида отдаляется
        }

        // 4 грани тетраэдра (каждая задаётся тремя вершинами);
        // OutVertex — оставшаяся 4-я вершина тетраэдра.
        private static readonly PyramidDef[] Pyramids =
        {
            new PyramidDef { A = 1, B = 2, C = 3, OutVertex = 0 }, // грань V1V2V3, отдаляется к V0
            new PyramidDef { A = 0, B = 2, C = 3, OutVertex = 1 }, // грань V0V2V3, отдаляется к V1
            new PyramidDef { A = 0, B = 1, C = 3, OutVertex = 2 }, // грань V0V1V3, отдаляется к V2
            new PyramidDef { A = 0, B = 1, C = 2, OutVertex = 3 }, // грань V0V1V2, отдаляется к V3
        };

        // -----------------------------------------------------------------
        // Состояние вращения (плавно интерполируется между случайными целями).
        // -----------------------------------------------------------------
        private Vector3 _curAxis = new Vector3(0, 1, 0);
        private Vector3 _tgtAxis = new Vector3(0, 1, 0);
        private float _curSpeed = 0.6f;   // рад/с
        private float _tgtSpeed = 0.6f;
        private float _angle = 0f;
        private double _nextAxisChange = 3.0;

        // -----------------------------------------------------------------
        // Состояние отдаления для каждой пирамиды.
        // -----------------------------------------------------------------
        private struct PyramidState
        {
            public float CurAmp, TgtAmp;       // текущая/целевая амплитуда отдаления
            public float CurFreq, TgtFreq;     // частота колебаний
            public float CurPhase, TgtPhase;   // фаза
            public double NextChange;          // когда в следующий раз сменить параметры
        }

        private readonly PyramidState[] _pyrStates = new PyramidState[4];
        private double _time = 0.0;

        // Один генератор на всю сцену: создаётся один раз, а не каждый кадр
        // (создание Random в каждом Update() давало аллокации и корреляцию
        // последовательностей из-за совпадающих сидов).
        private readonly Random _rng = new Random();

        // -----------------------------------------------------------------
        // Текстура логотипа.
        // -----------------------------------------------------------------
        private int _logoTextureId;
        private bool _initialized = false;

        // -----------------------------------------------------------------
        // Опции.
        // -----------------------------------------------------------------
        /// <summary>
        /// Если true, рендерит также «внутренние» грани пирамид (боковые
        /// треугольники от центра к рёбрам). Иначе видны только внешние
        /// грани с логотипом — эффект «парящих щитов».
        /// </summary>
        public bool ShowInternalFaces { get; set; } = true;

        /// <summary>Масштаб всей сцены (для встраивания в окна разного размера).</summary>
        public float Scale { get; set; } = 1.6f;

        public TetrahedronScene()
        {
            // Начальная инициализация случайных параметров пирамид
            var rng = new Random();
            for (int i = 0; i < 4; i++)
            {
                _pyrStates[i].CurAmp = 0f;
                _pyrStates[i].TgtAmp = 0.1f + (float)_rng.NextDouble() * 0.6f;
                _pyrStates[i].CurFreq = 0.3f + (float)_rng.NextDouble() * 0.8f;
                _pyrStates[i].TgtFreq = 0.3f + (float)_rng.NextDouble() * 0.8f;
                _pyrStates[i].CurPhase = (float)(_rng.NextDouble() * Math.PI * 2);
                _pyrStates[i].TgtPhase = (float)(_rng.NextDouble() * Math.PI * 2);
                _pyrStates[i].NextChange = 1.0 + _rng.NextDouble() * 4.0;
            }
        }

        /// <summary>
        /// Инициализация GL-состояния: загрузка текстуры, настройка света.
        /// Должна вызываться после создания GLControl, когда контекст активен.
        /// </summary>
        public void InitGL()
        {
            if (_initialized) return;
            _initialized = true;

            // Генерируем текстуру логотипа и загружаем в OpenGL
            using (Bitmap bmp = CyberdyneLogo.Generate(256))
            {
                _logoTextureId = LoadTexture(bmp);
            }

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // Простое освещение: чтобы грани были различимы по яркости
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            float[] lightPos = { 5f, 5f, 5f, 1f };
            float[] ambient = { 0.4f, 0.4f, 0.45f, 1f };
            float[] diffuse = { 0.7f, 0.75f, 0.8f, 1f };
            GL.Light(LightName.Light0, LightParameter.Position, lightPos);
            GL.Light(LightName.Light0, LightParameter.Ambient, ambient);
            GL.Light(LightName.Light0, LightParameter.Diffuse, diffuse);

            GL.Enable(EnableCap.Normalize);
            GL.ShadeModel(ShadingModel.Smooth);
        }

        private static int LoadTexture(Bitmap bmp)
        {
            int id;
            GL.GenTextures(1, out id);
            GL.BindTexture(TextureTarget.Texture2D, id);

            BitmapData data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                PixelType.UnsignedByte, data.Scan0);

            bmp.UnlockBits(data);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureBorderColor, new[] { 0f, 0f, 0f, 0f });
            return id;
        }

        /// <summary>
        /// Шаг анимации: обновляет ось/скорость вращения и параметры пирамид.
        /// </summary>
        public void Update(double dt)
        {
            _time += dt;

            // -- Вращение --
            _angle += _curSpeed * (float)dt;

            // Плавная интерполяция к цели
            float lerpFactor = 1f - (float)Math.Pow(0.05, dt); // ~ экспоненциальное сглаживание
            _curAxis = Vector3.Lerp(_curAxis, _tgtAxis, lerpFactor).Normalized();
            _curSpeed += (_tgtSpeed - _curSpeed) * lerpFactor;

            // Время смены оси/скорости
            _nextAxisChange -= dt;
            if (_nextAxisChange <= 0)
            {
                // Новая случайная ось (единичный вектор)
                _tgtAxis = RandomUnitVector(_rng);
                // Новая случайная скорость: 0.2..1.5 рад/с
                _tgtSpeed = 0.2f + (float)_rng.NextDouble() * 1.3f;
                // Следующая смена через 2-5 сек
                _nextAxisChange = 2.0 + _rng.NextDouble() * 3.0;
            }

            // -- Отдаление пирамид --
            for (int i = 0; i < 4; i++)
            {
                var st = _pyrStates[i];
                st.CurAmp += (st.TgtAmp - st.CurAmp) * lerpFactor;
                st.CurFreq += (st.TgtFreq - st.CurFreq) * lerpFactor;
                // Фаза интерполируется по кратчайшему пути
                float phaseDiff = st.TgtPhase - st.CurPhase;
                while (phaseDiff > Math.PI) phaseDiff -= (float)(2 * Math.PI);
                while (phaseDiff < -Math.PI) phaseDiff += (float)(2 * Math.PI);
                st.CurPhase += phaseDiff * lerpFactor;

                st.NextChange -= dt;
                if (st.NextChange <= 0)
                {
                    st.TgtAmp = 0.05f + (float)_rng.NextDouble() * 0.7f;
                    st.TgtFreq = 0.2f + (float)_rng.NextDouble() * 1.2f;
                    st.TgtPhase = (float)(_rng.NextDouble() * Math.PI * 2);
                    st.NextChange = 1.5 + _rng.NextDouble() * 3.5;
                }
                _pyrStates[i] = st;
            }
        }

        private static Vector3 RandomUnitVector(Random rng)
        {
            // Равномерное распределение на сфере (Marsaglia)
            double x, y, z, len;
            do
            {
                x = rng.NextDouble() * 2 - 1;
                y = rng.NextDouble() * 2 - 1;
                z = rng.NextDouble() * 2 - 1;
                len = x * x + y * y + z * z;
            } while (len > 1 || len < 1e-6);
            double inv = 1.0 / Math.Sqrt(len);
            return new Vector3((float)(x * inv), (float)(y * inv), (float)(z * inv));
        }

        /// <summary>
        /// Рендерит сцену. Перед вызовом GLControl должен быть текущим контекстом,
        /// viewport уже установлен.
        /// </summary>
        /// <param name="viewportW">Ширина viewport в пикселях.</param>
        /// <param name="viewportH">Высота viewport в пикселях.</param>
        /// <param name="transparentBackground">Если true — фон прозрачный
        /// (для desktop overlay).</param>
        public void Render(int viewportW, int viewportH, bool transparentBackground)
        {
            GL.Viewport(0, 0, viewportW, viewportH);
            GL.ClearColor(transparentBackground ? Color.FromArgb(0, 0, 0, 0) : Color.FromArgb(15, 18, 28));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // --- Матрица проекции ---
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            float aspect = viewportH > 0 ? (float)viewportW / viewportH : 1f;
            Matrix4 persp = Matrix4.CreatePerspectiveFieldOfView(
                (float)(Math.PI / 4), aspect, 0.1f, 100f);
            GL.LoadMatrix(ref persp);

            // --- Матрица вида ---
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 lookat = Matrix4.LookAt(
                new Vector3(0, 0, 6f),  // камера
                new Vector3(0, 0, 0),    // цель
                new Vector3(0, 1, 0));   // up
            GL.LoadMatrix(ref lookat);

            // --- Общее вращение всей сцены ---
            GL.PushMatrix();
            Matrix4 rot = Matrix4.CreateFromAxisAngle(_curAxis, _angle);
            GL.MultMatrix(ref rot);
            float s = Scale;
            GL.Scale(s, s, s);

            // Рисуем 4 пирамиды
            for (int i = 0; i < 4; i++)
            {
                DrawPyramid(i);
            }

            GL.PopMatrix();
        }

        private void DrawPyramid(int idx)
        {
            var def = Pyramids[idx];
            Vector3 a = TetraVertices[def.A];
            Vector3 b = TetraVertices[def.B];
            Vector3 c = TetraVertices[def.C];

            // Направление отдаления = нормаль внешней грани = направление к OutVertex
            // (центроид грани лежит на прямой от центра тетраэдра к OutVertex).
            Vector3 outDir = TetraVertices[def.OutVertex].Normalized();

            // Текущее отдаление для этой пирамиды (по синусоиде)
            var st = _pyrStates[idx];
            float offset = st.CurAmp * (0.5f + 0.5f * (float)Math.Sin(_time * st.CurFreq * 2 * Math.PI + st.CurPhase));

            Vector3 shift = outDir * offset;
            a += shift;
            b += shift;
            c += shift;

            // Центр пирамиды (её вершина, обращённая внутрь тетраэдра)
            Vector3 apex = Vector3.Zero + shift * 0.0f; // вершина остаётся в (0,0,0)

            // 1) Внешняя грань с логотипом (натянутая текстура)
            DrawTexturedTriangle(a, b, c, outDir);

            // 2) Боковые грани (если включены) — от вершины (0,0,0) к рёбрам внешней грани
            if (ShowInternalFaces)
            {
                DrawInternalFace(apex, a, b);
                DrawInternalFace(apex, b, c);
                DrawInternalFace(apex, c, a);
            }
        }

        /// <summary>
        /// Рисует треугольник с логотипом. uv-координаты: (0,0), (1,0), (0.5, 1)
        /// — равносторонний треугольник вписывается в текстуру.
        /// </summary>
        private void DrawTexturedTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
        {
            // Материал: серебристо-белая база, чтобы логотип читался
            float[] matAmbient = { 0.5f, 0.5f, 0.55f, 1f };
            float[] matDiffuse = { 0.8f, 0.8f, 0.85f, 1f };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, matAmbient);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, matDiffuse);

            GL.BindTexture(TextureTarget.Texture2D, _logoTextureId);
            GL.Color4(1f, 1f, 1f, 1f);

            GL.Begin(PrimitiveType.Triangles);
            GL.Normal3(normal.X, normal.Y, normal.Z);

            // Чтобы логотип смотрел «лицом» наружу, упорядочиваем вершины
            // по часовой стрелке при взгляде снаружи (против нормали).
            // Для грани V1V2V3 (OutVertex=V0) нормаль смотрит в сторону V0.
            // Делаем порядок вершин A → C → B, чтобы нормаль была согласована.
            GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(a.X, a.Y, a.Z);
            GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(c.X, c.Y, c.Z);
            GL.TexCoord2(0.5f, 0.0f); GL.Vertex3(b.X, b.Y, b.Z);
            GL.End();
        }

        private void DrawInternalFace(Vector3 apex, Vector3 p1, Vector3 p2)
        {
            // Полупрозрачная тёмная грань
            float[] matAmbient = { 0.15f, 0.18f, 0.25f, 0.7f };
            float[] matDiffuse = { 0.2f, 0.25f, 0.35f, 0.7f };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, matAmbient);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, matDiffuse);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Color4(0.3f, 0.35f, 0.45f, 0.7f);

            Vector3 normal = Vector3.Cross(p1 - apex, p2 - apex).Normalized();

            GL.Begin(PrimitiveType.Triangles);
            GL.Normal3(normal.X, normal.Y, normal.Z);
            GL.TexCoord2(0, 0); GL.Vertex3(apex.X, apex.Y, apex.Z);
            GL.TexCoord2(1, 0); GL.Vertex3(p1.X, p1.Y, p1.Z);
            GL.TexCoord2(0.5f, 1); GL.Vertex3(p2.X, p2.Y, p2.Z);
            GL.End();
        }

        public void Dispose()
        {
            if (_initialized && _logoTextureId != 0)
            {
                GL.DeleteTexture(_logoTextureId);
                _logoTextureId = 0;
                _initialized = false;
            }
        }
    }
}
