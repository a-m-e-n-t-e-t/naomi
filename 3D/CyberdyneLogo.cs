using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;

namespace _3D
{
    /// <summary>
    /// Генерирует полностью симметричный логотип «Cyberdyne Systems» в виде
    /// Bitmap 256×256 с прозрачным фоном для использования как текстуры
    /// на гранях тетраэдра.
    ///
    /// Логотип построен на тройной симметрии (как трискеле/трикветра),
    /// что делает его инвариантным относительно поворота на 120° —
    /// это хорошо смотрится на трёх гранях пирамиды.
    ///
    /// Элементы:
    ///   • Внешний ободок (серебристый)
    ///   • Три расходящихся луча-стрелки под 120° (голубой градиент)
    ///   • Внутренний круг с миниатюрным контуром
    ///   • Тонкая сетка-кант
    /// </summary>
    internal static class CyberdyneLogo
    {
        public static Bitmap Generate(int size = 256)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.Clear(Color.Transparent);

                float cx = size / 2f;
                float cy = size / 2f;
                float outerR = size * 0.46f;   // внешний радиус ободка
                float ringR  = size * 0.36f;   // внутренний радиус ободка
                float beamInnerR = size * 0.18f;
                float beamOuterR = size * 0.34f;
                float beamHalfWidth = size * 0.055f;
                float innerCircleR = size * 0.11f;

                // 1) Внешний ободок — серебристая «шайба»
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(cx - outerR, cy - outerR, outerR * 2, outerR * 2);
                    path.AddEllipse(cx - ringR, cy - ringR, ringR * 2, ringR * 2);
                    using (var brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(220, 220, 230);
                        brush.SurroundColors = new[] { Color.FromArgb(120, 130, 150) };
                        var blend = new Blend(3)
                        {
                            Positions = new[] { 0f, 0.5f, 1f },
                            Factors = new[] { 0f, 0.6f, 1f }
                        };
                        brush.Blend = blend;
                        g.FillPath(brush, path);
                    }
                }

                // 2) Тонкая внешняя граница ободка
                using (var pen = new Pen(Color.FromArgb(40, 50, 70), 1.5f))
                {
                    g.DrawEllipse(pen, cx - outerR, cy - outerR, outerR * 2, outerR * 2);
                    g.DrawEllipse(pen, cx - ringR, cy - ringR, ringR * 2, ringR * 2);
                }

                // 3) Три луча-стрелки под 120°
                //    Каждый луч — трапеция с утолщением к краю (стилизованная «лепешка» Cyberdyne).
                //    Поворот на 120° сохраняет картинку — это и есть симметрия.
                for (int i = 0; i < 3; i++)
                {
                    float angleDeg = i * 120f - 90f; // начинаем сверху, идём по часовой
                    DrawBeam(g, cx, cy, beamInnerR, beamOuterR, beamHalfWidth, angleDeg);
                }

                // 4) Внутренний круг — «ядро» логотипа (голубой с серебряной каймой)
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(cx - innerCircleR, cy - innerCircleR, innerCircleR * 2, innerCircleR * 2);
                    using (var brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(220, 245, 255);
                        brush.SurroundColors = new[] { Color.FromArgb(0, 110, 200) };
                        g.FillPath(brush, path);
                    }
                    using (var pen = new Pen(Color.FromArgb(80, 100, 130), 1.5f))
                    {
                        g.DrawEllipse(pen, cx - innerCircleR, cy - innerCircleR, innerCircleR * 2, innerCircleR * 2);
                    }
                }

                // 5) Центральная точка — маленький тёмно-синий круг в самом центре
                float dotR = size * 0.025f;
                using (var brush = new SolidBrush(Color.FromArgb(20, 30, 60)))
                {
                    g.FillEllipse(brush, cx - dotR, cy - dotR, dotR * 2, dotR * 2);
                }

                // 6) Декоративные точки на ободке под каждым лучом (по 3 симметрично)
                for (int i = 0; i < 3; i++)
                {
                    float angleDeg = i * 120f - 90f;
                    float rad = (float)(angleDeg * System.Math.PI / 180.0);
                    float dotOuterR = size * 0.41f;
                    float dx = cx + (float)(dotOuterR * System.Math.Cos(rad));
                    float dy = cy + (float)(dotOuterR * System.Math.Sin(rad));
                    float dR = size * 0.015f;
                    using (var brush = new SolidBrush(Color.FromArgb(20, 30, 60)))
                    {
                        g.FillEllipse(brush, dx - dR, dy - dR, dR * 2, dR * 2);
                    }
                }
            }
            return bmp;
        }

        /// <summary>
        /// Рисует один луч-стрелку под заданным углом.
        /// Луч — трапеция с расширяющимся концом, в стиле логотипа Cyberdyne.
        /// </summary>
        private static void DrawBeam(Graphics g, float cx, float cy,
            float innerR, float outerR, float halfWidth, float angleDeg)
        {
            double rad = angleDeg * System.Math.PI / 180.0;
            // Направление луча (от центра наружу)
            float dirX = (float)System.Math.Cos(rad);
            float dirY = (float)System.Math.Sin(rad);
            // Перпендикуляр к направлению
            float perpX = -dirY;
            float perpY = dirX;

            // Четыре угла трапеции (расширяется к внешнему краю)
            float inRX = cx + dirX * innerR + perpX * halfWidth * 0.5f;
            float inRY = cy + dirY * innerR + perpY * halfWidth * 0.5f;
            float inLX = cx + dirX * innerR - perpX * halfWidth * 0.5f;
            float inLY = cy + dirY * innerR - perpY * halfWidth * 0.5f;
            float outRX = cx + dirX * outerR + perpX * halfWidth;
            float outRY = cy + dirY * outerR + perpY * halfWidth;
            float outLX = cx + dirX * outerR - perpX * halfWidth;
            float outLY = cy + dirY * outerR - perpY * halfWidth;

            // Градиент: голубой у центра → ярко-серебристый у края
            var path = new GraphicsPath();
            path.AddPolygon(new[] {
                new PointF(inRX, inRY),
                new PointF(outRX, outRY),
                new PointF(outLX, outLY),
                new PointF(inLX, inLY),
            });

            using (var brush = new LinearGradientBrush(
                new PointF(cx + dirX * innerR, cy + dirY * innerR),
                new PointF(cx + dirX * outerR, cy + dirY * outerR),
                Color.FromArgb(0, 140, 220),
                Color.FromArgb(180, 220, 255)))
            {
                g.FillPath(brush, path);
            }

            // Тонкая тёмная окантовка луча
            using (var pen = new Pen(Color.FromArgb(60, 80, 120), 1.0f))
            {
                g.DrawPath(pen, path);
            }

            // Стрелка-наконечник: маленький треугольник на конце луча
            float tipR = outerR + halfWidth * 1.4f;
            float tipHalf = halfWidth * 1.6f;
            float tipX = cx + dirX * tipR;
            float tipY = cy + dirY * tipR;
            // Основание стрелки (на уровне outR)
            float baseRX = outRX;
            float baseRY = outRY;
            float baseLX = outLX;
            float baseLY = outLY;
            var tipPath = new GraphicsPath();
            tipPath.AddPolygon(new[] {
                new PointF(baseRX, baseRY),
                new PointF(tipX, tipY),
                new PointF(baseLX, baseLY),
            });
            using (var brush = new SolidBrush(Color.FromArgb(220, 240, 255)))
            {
                g.FillPath(brush, tipPath);
            }
            using (var pen = new Pen(Color.FromArgb(60, 80, 120), 1.0f))
            {
                g.DrawPath(pen, tipPath);
            }

            path.Dispose();
            tipPath.Dispose();
        }
    }
}
