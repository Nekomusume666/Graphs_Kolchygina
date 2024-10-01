namespace TEST_GRAPH
{
    public class Vertex
    {
        public Point Position { get; set; }
        public int Radius { get; } = 20;
        public string Id { get; }

        // Список соседних вершин
        public List<Vertex> Neighbors { get; private set; }
        public Vertex(Point position, string id)
        {
            Position = position;
            Id = id;
            Neighbors = new List<Vertex>();
        }

        public bool Contains(Point p)
        {
            int dx = p.X - Position.X;
            int dy = p.Y - Position.Y;
            return dx * dx + dy * dy <= Radius * Radius;
        }

        public void Draw(Graphics g, Color? color = null)
        {
            Color fillColor = color ?? Color.LightBlue;
            g.FillEllipse(new SolidBrush(fillColor), Position.X - Radius, Position.Y - Radius, Radius * 2, Radius * 2);
            g.DrawEllipse(Pens.Black, Position.X - Radius, Position.Y - Radius, Radius * 2, Radius * 2);
        }

        // Метод для добавления соседа
        public void AddNeighbor(Vertex neighbor)
        {
            if (!Neighbors.Contains(neighbor))
            {
                Neighbors.Add(neighbor);
            }
        }
    }
}
