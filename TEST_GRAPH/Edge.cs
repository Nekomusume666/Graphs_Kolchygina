using System.Drawing.Drawing2D;

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

        public void Draw(Graphics g, Color? color = null)
        {
            Color fillColor = color ?? Color.Black;
            Pen pen = new Pen(fillColor, 2);
            AdjustableArrowCap bigArrow = new AdjustableArrowCap(5, 5);
            pen.CustomEndCap = bigArrow;

            g.DrawLine(pen, Start.Position, End.Position);

            Point weightPosition = new Point((Start.Position.X + End.Position.X) / 2, (Start.Position.Y + End.Position.Y) / 2);
            g.DrawString(Weight.ToString(), SystemFonts.DefaultFont, Brushes.Black, weightPosition);
        }
    }
}
