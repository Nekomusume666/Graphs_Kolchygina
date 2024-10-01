using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TEST_GRAPH
{
    public class Packet
    {
        public static int PacketCounter = 0;
        public int Id { get; private set; }
        public Vertex CurrentVertex { get; set; }
        public Vertex TargetVertex { get; set; } // Промежуточная целевая вершина
        public Vertex FinalTargetVertex { get; set; } // Конечная вершина
        public PointF Position { get; set; }
        public bool IsMoving { get; set; }

        public Vertex PreviousVertex { get; set; }

        private float speed = 2f; // Скорость передвижения пакета
        private Func<Packet, Vertex> routingStrategy; // Стратегия маршрутизации
        private int maxVisitedVertices = 10; // Максимальное количество посещенных вершин до гибели
        private int visitedVerticesCount = 0; // Счетчик посещенных вершин
        private HashSet<Vertex> visitedVertices = new HashSet<Vertex>(); // Уникальные посещенные вершины
        private RichTextBox logBox; // Поле для хранения ссылки на RichTextBox для логирования
        public string routingType; // Тип маршрутизации (например, "Случайная", "Кратчайший путь" и т.д.)

        // Конструктор пакета с передачей ссылки на RichTextBox и тип маршрутизации
        public Packet(Vertex startVertex, Vertex finalTargetVertex, Func<Packet, Vertex> routingStrategy, string routingType, RichTextBox logBox)
        {
            Id = PacketCounter++;
            CurrentVertex = startVertex;
            FinalTargetVertex = finalTargetVertex;
            Position = startVertex.Position;
            this.routingStrategy = routingStrategy;
            this.routingType = routingType; // Сохраняем тип маршрутизации
            IsMoving = false;
            visitedVertices.Add(startVertex); // Начальную вершину считаем посещенной
            visitedVerticesCount = 1; // Стартовая вершина - первая посещенная
            this.logBox = logBox; // Сохраняем ссылку на RichTextBox

            // Логирование создания пакета
            Log($"Packet {Id} ({routingType}) создан. Стартовая вершина: {startVertex.Id}");
        }

        // Метод для "убийства" пакета
        public void Kill()
        {
            // Логгируем гибель пакета
            Log($"Packet {Id} ({routingType}) погиб.");

            // Если нужно выполнить дополнительные действия при удалении пакета, можно добавить их сюда.
            // Например, вы можете остановить его движение или убрать его из списка отображаемых объектов.
            IsMoving = false;
            TargetVertex = null;

            // Если нужно, можно также вызывать специальные действия или события при гибели пакета
        }

        // Движение пакета к целевой вершине
        public bool MoveTowardsTarget()
        {
            // Проверка на количество посещенных вершин
            if (visitedVerticesCount > maxVisitedVertices)
            {
                // Убиваем пакет, так как он превысил лимит посещенных вершин
                Kill();
                return true; // Пакет должен быть удален
            }

            // Проверка на достижение конечной цели
            if (CurrentVertex == FinalTargetVertex)
            {
                // Логирование достижения цели
                Log($"Packet {Id} ({routingType}) достиг конечной цели.");
                return true; // Пакет достиг своей конечной цели
            }

            // Проверяем расстояние до промежуточной цели
            if (TargetVertex != null)
            {
                float dx = TargetVertex.Position.X - Position.X;
                float dy = TargetVertex.Position.Y - Position.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                // Если пакет достиг промежуточной цели
                if (distance < speed)
                {
                    Position = TargetVertex.Position; // Пакет "встал" на позицию вершины
                    CurrentVertex = TargetVertex; // Пакет теперь находится в этой вершине
                    IsMoving = false;
                    Log($"Packet {Id} ({routingType}) достиг вершины: {TargetVertex.Id}");
                    return false; // Пакет должен продолжить движение
                }
                else
                {
                    // Пакет движется к промежуточной цели
                    Position = new PointF(Position.X + dx / distance * speed, Position.Y + dy / distance * speed);
                    return false;
                }
            }

            return false;
        }

        // Метод для маршрутизации пакета
        public void Route()
        {
            if (IsMoving) return; // Пакет уже в движении, ничего не делаем

            // Если текущая вершина является конечной, завершаем маршрут
            if (CurrentVertex == FinalTargetVertex)
            {
                Log($"Packet {Id} ({routingType}) достиг конечной вершины.");
                return; // Пакет достиг конечной цели
            }

            // Используем переданную стратегию маршрутизации для выбора следующей вершины
            Vertex nextVertex = routingStrategy(this);
            this.PreviousVertex = CurrentVertex;

            if (nextVertex != null && !visitedVertices.Contains(nextVertex))
            {
                Log($"Packet {Id} ({routingType}) направляется в вершину: {nextVertex.Id}");
                TargetVertex = nextVertex; // Назначаем новую промежуточную целевую вершину
                IsMoving = true;
                visitedVertices.Add(nextVertex); // Добавляем вершину в список посещенных
                visitedVerticesCount = visitedVertices.Count; // Обновляем количество посещенных вершин
            }
            else
            {
                Log($"Packet {Id} ({routingType}) не смог найти доступную вершину для маршрутизации.");
            }
        }

        // Метод для логирования в RichTextBox
        public void Log(string message)
        {
            // Проверяем необходимость использования Invoke для потока интерфейса
            if (logBox.InvokeRequired)
            {
                logBox.Invoke(new Action(() => logBox.AppendText(message + Environment.NewLine)));
            }
            else
            {
                logBox.AppendText(message + Environment.NewLine);
            }
        }

        // Рисование пакета
        public void Draw(Graphics g)
        {
            float radius = 10;
            g.FillEllipse(Brushes.Green, Position.X - radius, Position.Y - radius, radius * 2, radius * 2);
            g.DrawEllipse(Pens.Black, Position.X - radius, Position.Y - radius, radius * 2, radius * 2);
            g.DrawString($"P{Id}", SystemFonts.DefaultFont, Brushes.Black, Position.X + radius, Position.Y + radius);
        }
    }
}
