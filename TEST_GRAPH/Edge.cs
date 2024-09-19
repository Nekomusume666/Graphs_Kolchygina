using System.Drawing.Drawing2D;
using System.Windows.Forms.VisualStyles;

namespace TEST_GRAPH
{
    public class Edge
    {
        public Vertex Start { get; }
        public Vertex End { get; }
        public int Weight { get; set; }

        public Edge(Vertex start, Vertex end, int weight)
        {
            Start = start;
            End = end;
            Weight = weight;
        }

        public bool IsReverseOf(Edge other)
        {
            return this.Start == other.End && this.End == other.Start;
        }

        public void Draw(Graphics g, List<Edge> edges, int weight, Color? color = null)
        {
            Pen pen = new Pen(color ?? Color.Black, 2);

            Edge reverseEdge = edges.FirstOrDefault(e => e.IsReverseOf(this));
            if (reverseEdge != null)
            {
                DrawCurvedArrow(g, pen, Start.Position, End.Position, weight);
            }
            else
            {
                DrawArrow(g, pen, Start.Position, End.Position, weight);
            }

            pen.Dispose();
        }

        private void DrawArrow(Graphics g, Pen pen, PointF start, PointF end, int weight)
        {
            float vertexRadius = 20;  // Радиус вершины
            float offset = 10;  // Смещение веса от линии

            // Рассчитываем направление от start к end
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0)
            {
                dx /= distance;
                dy /= distance;

                // Корректируем точки начала и конца стрелки с учетом радиусов вершин
                PointF adjustedStart = new PointF(start.X + dx * vertexRadius, start.Y + dy * vertexRadius);
                PointF adjustedEnd = new PointF(end.X - dx * vertexRadius, end.Y - dy * vertexRadius);

                g.DrawLine(pen, adjustedStart, adjustedEnd);

                DrawArrowHead(g, pen, adjustedStart, adjustedEnd);

                PointF middlePoint = GetMiddlePoint(adjustedStart, adjustedEnd);
                PointF weightPosition = new PointF(middlePoint.X, middlePoint.Y - offset); 

                string weightText = weight.ToString();
                Font font = SystemFonts.DefaultFont;
                SizeF textSize = g.MeasureString(weightText, font);
                
                RectangleF textBackground = new RectangleF(
                    weightPosition.X - textSize.Width / 2,
                    weightPosition.Y - textSize.Height / 2,
                    textSize.Width,
                    textSize.Height
                );

                g.FillRectangle(Brushes.White, textBackground);

                g.DrawString(weightText, font, Brushes.Black, weightPosition.X - textSize.Width / 2, weightPosition.Y - textSize.Height / 2);
            }
        }

        private void DrawCurvedArrow(Graphics g, Pen pen, PointF start, PointF end, int weight)
        {
            float vertexRadius = 20;  // Радиус вершины
            float offset = 15;  // Смещение веса от кривой

            // Вычисляем направление и расстояние
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0)
            {
                dx /= distance;
                dy /= distance;

                // Корректируем точки начала и конца стрелки с учетом радиусов вершин
                PointF adjustedStart = new PointF(start.X + dx * vertexRadius, start.Y + dy * vertexRadius);
                PointF adjustedEnd = new PointF(end.X - dx * vertexRadius, end.Y - dy * vertexRadius);

                // Вычисляем контрольную точку для кривой
                PointF controlPoint = GetControlPoint(adjustedStart, adjustedEnd);

                GraphicsPath path = new GraphicsPath();
                path.AddBezier(adjustedStart, controlPoint, controlPoint, adjustedEnd);
                g.DrawPath(pen, path);

                // Рисуем наконечник стрелки
                DrawArrowHead(g, pen, controlPoint, adjustedEnd);

                // Отображаем вес дуги, смещая его от кривой
                PointF weightPosition = new PointF(controlPoint.X, controlPoint.Y - offset); 

                string weightText = weight.ToString();
                Font font = SystemFonts.DefaultFont;
                SizeF textSize = g.MeasureString(weightText, font);

                RectangleF textBackground = new RectangleF(
                    weightPosition.X - textSize.Width / 2,
                    weightPosition.Y - textSize.Height / 2,
                    textSize.Width,
                    textSize.Height
                );

                g.FillRectangle(Brushes.White, textBackground);

                g.DrawString(weightText, font, Brushes.Black, weightPosition.X - textSize.Width / 2, weightPosition.Y - textSize.Height / 2);
            }
        }

        private void DrawArrowHead(Graphics g, Pen pen, PointF start, PointF end)
        {
            const float headSize = 10;
            float angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X); // Угол направления стрелки

            // Точки, определяющие треугольник стрелки
            PointF arrowPoint1 = new PointF(
                end.X - headSize * (float)Math.Cos(angle - Math.PI / 6),
                end.Y - headSize * (float)Math.Sin(angle - Math.PI / 6));

            PointF arrowPoint2 = new PointF(
                end.X - headSize * (float)Math.Cos(angle + Math.PI / 6),
                end.Y - headSize * (float)Math.Sin(angle + Math.PI / 6));

            g.DrawLine(pen, end, arrowPoint1);
            g.DrawLine(pen, end, arrowPoint2);
        }

        private PointF GetControlPoint(PointF start, PointF end)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float offset = 20; 

            float nx = -dy / length;
            float ny = dx / length;

            return new PointF((start.X + end.X) / 2 + nx * offset, (start.Y + end.Y) / 2 + ny * offset);
        }

        private PointF GetMiddlePoint(PointF start, PointF end)
        {
            return new PointF((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        }

    }
}
