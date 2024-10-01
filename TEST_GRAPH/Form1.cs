namespace TEST_GRAPH
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;
    using System.Drawing.Drawing2D;
    using System.Net.Sockets;

    public partial class Form1 : Form
    {
        private List<Vertex> vertices = new List<Vertex>();
        private List<Edge> edges = new List<Edge>();
        private Vertex selectedVertex = null;
        private bool firstClick = true;
        private int vertexCounter = 1;
        private Vertex previousSelectedVertex = null;
        private List<Edge> highlightedEdges = new List<Edge>();

        private List<Packet> packets; // Коллекция всех пакетов
        private Random random = new Random();
        private Timer moveTimer;

        private void RunTestData()
        {
            // Очищаем текущие данные
            vertices.Clear();
            edges.Clear();
            dgvIncidenceMatrix.Rows.Clear();
            dgvIncidenceMatrix.Columns.Clear();

            Random rnd = new Random();

            // Генерируем случайное количество вершин
            int vertexCount = rnd.Next(4, 9);

            // Создаем вершины с рандомными координатами
            for (int i = 0; i < vertexCount; i++)
            {
                AddVertex(new Point(rnd.Next(50, 500), rnd.Next(50, 500)));
            }


            // Генерация ребер: случайные веса между вершинами
            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    if (i != j && rnd.Next(0, 2) == 1) // Генерируем ребра случайно
                    {
                        dgvIncidenceMatrix.Rows[i].Cells[j].Value = rnd.Next(1, 10); // Ребро i -> j с рандомным весом
                    }
                }
            }
        }

        public Form1()
        {
            InitializeComponent();


            panelGraph.MouseDown += PanelGraph_MouseDown;
            panelGraph.MouseUp += PanelGraph_MouseUp;
            panelGraph.MouseMove += PanelGraph_MouseMove;
            panelGraph.Paint += PanelGraph_Paint;
            InitContextMenu();


            packets = new List<Packet>(); // Инициализация списка пакетов
            InitMoveTimer();
        }

        private void InitMoveTimer()
        {
            moveTimer = new Timer();
            moveTimer.Interval = 20;
            moveTimer.Tick += MoveTimer_Tick;
            moveTimer.Start();
        }

        public void CreatePacket(Vertex startVertex, Vertex targetVertex, Func<Packet, Vertex> routingStrategy, string routeString, RichTextBox textBox)
        {
            Packet newPacket = new Packet(startVertex, targetVertex, routingStrategy, routeString, textBox);
            packets.Add(newPacket); // Добавляем пакет в список активных пакетов
        }


        private void MoveTimer_Tick(object sender, EventArgs e)
        {
            // Новый список для активных пакетов
            List<Packet> remainingPackets = new List<Packet>();

            // Копии пакетов, которые нужно будет добавить в основной список
            List<Packet> newPackets = new List<Packet>();

            foreach (var packet in packets)
            {
                packet.Route(); // Выбор следующей вершины для маршрута


                    if (packet.IsMoving)
                    {
                        bool reachedTargetOrDied = packet.MoveTowardsTarget(); // Пакет движется

                        // Если пакет достиг цели или "умер", он не добавляется в новый список
                        if (!reachedTargetOrDied)
                        {
                            remainingPackets.Add(packet); // Пакет еще в пути
                        }
                    }
                    else
                    {
                        // Если пакет не двигается, проверяем, достиг ли он целевой вершины
                        if (packet.CurrentVertex != packet.TargetVertex)
                        {
                            remainingPackets.Add(packet); // Если не достиг, оставляем в списке
                        }
                    }

                if (packet.routingType == "Лавинная маршрутизация" && !packet.IsMoving)
                {
                    // Создаем новые пакеты для лавинной маршрутизации
                    List<Packet> avalanchePackets = AvalancheRouting(packet);

                    // Логгируем создание новых пакетов
                    packet.Log($"Родительский пакет {packet.Id} создал {avalanchePackets.Count} дочерних пакетов.");

                    // Убиваем родительский пакет после создания дочерних пакетов
                    packet.Kill();

                    // Добавляем новые пакеты в список
                    newPackets.AddRange(avalanchePackets);

                    remainingPackets.Remove(packet);
                }


            }

            // Добавляем новые пакеты в основной список
            remainingPackets.AddRange(newPackets);

            // Обновляем список пакетов
            packets = remainingPackets;

            // Перерисовка панели для обновления позиций пакетов
            panelGraph.Invalidate();
        }

        public Vertex RandomRouting(Packet packet)
        {
            List<Edge> availableEdges = edges.Where(edge => edge.Start == packet.CurrentVertex).ToList();
            if (availableEdges.Count > 0)
            {
                Edge randomEdge = availableEdges[random.Next(availableEdges.Count)];
                return randomEdge.End;
            }
            return null; // Пакет застрял, если нет исходящих рёбер
        }
        public List<Packet> AvalancheRouting(Packet originalPacket)
        {
            List<Packet> newPackets = new List<Packet>();

            // Получаем всех соседних вершин, кроме той, откуда пришел пакет
            List<Vertex> neighbors = originalPacket.CurrentVertex.Neighbors
                                                 .Where(v => v != originalPacket.PreviousVertex)
                                                 .ToList();

            if (neighbors.Count == 0)
            {
                originalPacket.Log($"Packet {originalPacket.Id} ({originalPacket.routingType}) не нашел соседей для лавинной маршрутизации.");
            }

            // Для каждого соседа создаем новый пакет
            foreach (Vertex neighbor in neighbors)
            {
                // Создаем новый пакет-копию, который движется в каждую соседнюю вершину
                Packet newPacket = new Packet(
                    originalPacket.CurrentVertex,        // Стартовая вершина
                    originalPacket.FinalTargetVertex,    // Конечная цель (не меняется)
                    AvalancheRoutingStrategy,            // Стратегия маршрутизации
                    "Лавинная маршрутизация",            // Тип маршрутизации
                    richTextBox1                         // Для логирования
                );

                newPacket.PreviousVertex = originalPacket.CurrentVertex; // Текущая вершина станет предыдущей для нового пакета
                newPacket.TargetVertex = neighbor; // Цель - соседняя вершина
                newPacket.IsMoving = true;         // Новый пакет сразу начинает движение

                // Логгируем создание нового пакета
                newPacket.Log($"Новый пакет {newPacket.Id} ({newPacket.routingType}) создан для вершины {neighbor.Id}.");

                newPackets.Add(newPacket); // Добавляем новый пакет в список новых пакетов
            }

            return newPackets; // Возвращаем список новых пакетов
        }


        public Vertex AvalancheRoutingStrategy(Packet packet)
        {
            // Выбираем всех соседей, кроме той вершины, откуда пришел пакет
            List<Vertex> neighbors = packet.CurrentVertex.Neighbors
                                            .Where(v => v != packet.PreviousVertex)
                                            .ToList();

            // Для лавинной маршрутизации - возвращаем первого доступного соседа
            if (neighbors.Count > 0)
            {
                Vertex newVertex = neighbors.First(); // Берем первого доступного соседа
                return newVertex; // Возвращаем нового соседа
            }
            else
            {
                return null; // Если нет доступных соседей
            }
        }







        ///////////////////////////////////////////////////////////////////////////////////////////


        private void DgvIncidenceMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string rowVertexId = dgvIncidenceMatrix.Rows[e.RowIndex].HeaderCell.Value.ToString();
            string colVertexId = dgvIncidenceMatrix.Columns[e.ColumnIndex].HeaderText;

            if (int.TryParse(dgvIncidenceMatrix.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out int newWeight))
            {
                var vertexRow = vertices.FirstOrDefault(v => v.Id == rowVertexId);
                var vertexCol = vertices.FirstOrDefault(v => v.Id == colVertexId);

                if (vertexRow == null || vertexCol == null) return;

                var edge = edges.FirstOrDefault(ed => ed.Start == vertexRow && ed.End == vertexCol);

                if (newWeight != 0)
                {
                    if (edge != null)
                    {
                        edge.Weight = newWeight;
                    }
                    else
                    {
                        edges.Add(new Edge(vertexRow, vertexCol, newWeight));
                        vertexRow.AddNeighbor(vertexCol);
                    }

                    //dgvIncidenceMatrix.Rows[e.ColumnIndex].Cells[e.RowIndex].Value = newWeight;

                }
                else
                {
                    if (edge != null)
                    {
                        edges.Remove(edge);
                    }

                    //dgvIncidenceMatrix.Rows[e.ColumnIndex].Cells[e.RowIndex].Value = 0;
                }

                panelGraph.Invalidate();
            }
        }


        // Обновление матрицы смежности
        private void UpdateIncidenceMatrix()
        {
            dgvIncidenceMatrix.Rows.Clear();
            dgvIncidenceMatrix.Columns.Clear();

            foreach (var vertex in vertices)
            {
                dgvIncidenceMatrix.Columns.Add(vertex.Id, vertex.Id);
            }

            foreach (var vertexRow in vertices)
            {
                var row = new DataGridViewRow();
                row.HeaderCell.Value = vertexRow.Id;

                foreach (var vertexCol in vertices)
                {
                    var edge = edges.FirstOrDefault(ed => ed.Start == vertexRow && ed.End == vertexCol);
                    int cellValue = edge != null ? edge.Weight : 0;

                    var cell = new DataGridViewTextBoxCell { Value = cellValue };
                    row.Cells.Add(cell);
                }

                dgvIncidenceMatrix.Rows.Add(row);
            }

            dgvIncidenceMatrix.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

            dgvIncidenceMatrix.CellValueChanged += DgvIncidenceMatrix_CellValueChanged;
        }

        // Инициализация контекстного меню
        private void InitContextMenu()
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Добавить вершину", null, (s, e) => AddVertex(MousePosition));
            contextMenu.Items.Add("Удалить вершину", null, (s, e) => RemoveVertex());
            contextMenu.Items.Add("Добавить дугу", null, (s, e) => AddEdge());
            contextMenu.Items.Add("Изменить вес дуги", null, (s, e) => ChangeEdgeWeight());
            contextMenu.Items.Add("Удалить дугу", null, (s, e) => RemoveEdge());
            contextMenu.Items.Add("Найти кратчайший путь (Дейкстра)", null, (s, e) => FindShortestPath());
            contextMenu.Items.Add("Найти кратчайший путь (Флойд)", null, (s, e) => FindShortestPathFloyd());
            contextMenu.Items.Add("Сравнить время выполнения (Дейкстра и Флойд)", null, (s, e) => CompareAlgorithms());
            contextMenu.Items.Add("Сгенерировать граф", null, (s, e) => RunTestData());

            panelGraph.ContextMenuStrip = contextMenu;
        }


        // Добавление вершины
        private void AddVertex(Point position)
        {
            Point localPoint = panelGraph.PointToClient(position);
            vertices.Add(new Vertex(localPoint, "X" + vertexCounter++));
            panelGraph.Invalidate();
            UpdateIncidenceMatrix();

        }

        // Метод для сброса выбранных вершин
        private void ResetSelectedVertices()
        {
            previousSelectedVertex = null;
            selectedVertex = null;
            panelGraph.Invalidate();
            UpdateIncidenceMatrix();
        }

        private void FindShortestPathFloyd()
        {
            if (previousSelectedVertex == null || selectedVertex == null)
            {
                MessageBox.Show("Не выбраны начальная и конечная вершины!");
                return;
            }

            int n = vertices.Count;
            int[,] graph = new int[n, n];
            int[,] distances;
            int[,] predecessors;

            // Инициализация матрицы весов графа
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    graph[i, j] = int.MaxValue;
                    if (i == j)
                    {
                        graph[i, j] = 0;
                    }
                }
            }

            foreach (var edge in edges)
            {
                int startIndex = vertices.IndexOf(edge.Start);
                int endIndex = vertices.IndexOf(edge.End);
                graph[startIndex, endIndex] = edge.Weight;
            }

            var startTime = DateTime.Now;
            FloydWarshall.ComputeShortestPaths(graph, out distances, out predecessors);
            var endTime = DateTime.Now;

            int startIdx = vertices.IndexOf(previousSelectedVertex);
            int endIdx = vertices.IndexOf(selectedVertex);

            if (distances[startIdx, endIdx] != int.MaxValue)
            {
                string pathStr = "";
                var path = new List<Vertex>();
                PrintPath(predecessors, startIdx, endIdx, ref pathStr, path);

                // Обновляем список highlightedEdges
                highlightedEdges.Clear();
                for (int i = 0; i < path.Count - 1; i++)
                {
                    var startVertex = path[i];
                    var endVertex = path[i + 1];

                    var edge = edges.FirstOrDefault(e => e.Start == startVertex && e.End == endVertex);
                    if (edge != null)
                    {
                        highlightedEdges.Add(edge);
                    }
                }

                MessageBox.Show($"Кратчайший путь (Флойд) от {previousSelectedVertex.Id} до {selectedVertex.Id}: {pathStr}\nДлина пути: {distances[startIdx, endIdx]}\nВремя выполнения: {endTime - startTime}");
            }
            else
            {
                MessageBox.Show("Пути не существует.");
            }

            panelGraph.Invalidate(); // Перерисовываем граф с выделенными путями
            ResetSelectedVertices();
        }


        private void CompareAlgorithms()
        {
            // Инициализация для Дейкстры
            int n = vertices.Count;
            int[,] distancesDijkstra = new int[n, n];
            List<Vertex>[,] pathsDijkstra = new List<Vertex>[n, n];
            var startTimeDijkstra = DateTime.Now;

            // Применение алгоритма Дейкстры n раз для каждой вершины
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        var (distance, path) = Dijkstra(vertices[i], vertices[j]);
                        distancesDijkstra[i, j] = distance;
                        pathsDijkstra[i, j] = path;
                    }
                    else
                    {
                        distancesDijkstra[i, j] = 0;
                        pathsDijkstra[i, j] = new List<Vertex> { vertices[i] };
                    }
                }
            }
            var endTimeDijkstra = DateTime.Now;

            // Инициализация для алгоритма Флойда
            int[,] graph = new int[n, n];
            int[,] distancesFloyd;
            int[,] predecessors;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    graph[i, j] = (i == j) ? 0 : int.MaxValue;
                }
            }

            foreach (var edge in edges)
            {
                int startIndex = vertices.IndexOf(edge.Start);
                int endIndex = vertices.IndexOf(edge.End);
                graph[startIndex, endIndex] = edge.Weight;
            }

            // Выполнение алгоритма Флойда
            var startTimeFloyd = DateTime.Now;
            FloydWarshall.ComputeShortestPaths(graph, out distancesFloyd, out predecessors);
            var endTimeFloyd = DateTime.Now;

            // Подготовка строк для вывода результата
            string resultsDijkstra = "Результаты метода Дейкстры:\n";
            string resultsFloyd = "Результаты метода Флойда:\n";

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                    {
                        string pathStrDijkstra = string.Join(" -> ", pathsDijkstra[i, j].Select(v => v.Id));
                        resultsDijkstra += $"Путь {vertices[i].Id} -> {vertices[j].Id}: {pathStrDijkstra}, длина: {distancesDijkstra[i, j]}\n";

                        string pathStrFloyd = "";
                        var pathFloyd = new List<Vertex>();
                        if (distancesFloyd[i, j] != int.MaxValue)
                        {
                            PrintPath(predecessors, i, j, ref pathStrFloyd, pathFloyd);
                        }
                        resultsFloyd += $"Путь {vertices[i].Id} -> {vertices[j].Id}: {pathStrFloyd}, длина: {distancesFloyd[i, j]}\n";
                    }
                }
            }

            // Вывод результатов в одном окне
            var message = $"{resultsDijkstra}\nВремя выполнения Дейкстры: {endTimeDijkstra - startTimeDijkstra}\n\n" +
                          $"{resultsFloyd}\nВремя выполнения Флойда: {endTimeFloyd - startTimeFloyd}";

            //MessageBox.Show(message);

            richTextBox1.Text = message;

            panelGraph.Invalidate(); // Перерисовываем граф
            ResetSelectedVertices();
        }



        private void PrintPath(int[,] predecessors, int start, int end, ref string pathStr, List<Vertex> path)
        {
            if (start == end)
            {
                path.Add(vertices[start]);
                pathStr = vertices[start].Id;
            }
            else if (predecessors[start, end] == -1)
            {
                path.Add(vertices[start]);
                path.Add(vertices[end]);
                pathStr = vertices[start].Id + " -> " + vertices[end].Id;
            }
            else
            {
                PrintPath(predecessors, start, predecessors[start, end], ref pathStr, path);
                pathStr += " -> " + vertices[end].Id;
                path.Add(vertices[end]);
            }
        }

        // Пример вызова метода сброса в контекстном меню
        private void FindShortestPath()
        {
            if (previousSelectedVertex == null || selectedVertex == null)
            {
                MessageBox.Show("Не выбраны начальная и конечная вершины!");
                return;
            }

            var startTime = DateTime.Now;
            var (distance, path) = Dijkstra(previousSelectedVertex, selectedVertex);
            var endTime = DateTime.Now;

            if (distance == int.MaxValue)
            {
                MessageBox.Show("Пути не существует.");
            }
            else
            {
                string pathStr = string.Join(" -> ", path.Select(v => v.Id));
                MessageBox.Show($"Кратчайший путь (Дейкстры) от {previousSelectedVertex.Id} до {selectedVertex.Id}: {pathStr}" +
                    $"\nДлина пути: {distance}\nВремя выполнения: {endTime - startTime}");

                highlightedEdges.Clear();
                for (int i = 0; i < path.Count - 1; i++)
                {
                    var startVertex = path[i];
                    var endVertex = path[i + 1];

                    var edge = edges.FirstOrDefault(e => e.Start == startVertex && e.End == endVertex);
                    if (edge != null)
                    {
                        highlightedEdges.Add(edge);
                    }
                }

                panelGraph.Invalidate();
            }

            ResetSelectedVertices();
        }

        // Алгоритм Дейкстры
        private (int, List<Vertex>) Dijkstra(Vertex start, Vertex end)
        {
            var distances = new Dictionary<Vertex, int>();
            var previous = new Dictionary<Vertex, Vertex>();
            var unvisited = new List<Vertex>(vertices);

            foreach (var vertex in vertices)
            {
                distances[vertex] = int.MaxValue;
                previous[vertex] = null;
            }
            distances[start] = 0;

            while (unvisited.Count > 0)
            {
                Vertex current = null;
                foreach (var vertex in unvisited)
                {
                    if (current == null || distances[vertex] < distances[current])
                    {
                        current = vertex;
                    }
                }

                if (current == end) break;  // Если достигли конечной вершины

                unvisited.Remove(current);

                foreach (var edge in edges)
                {
                    if (edge.Start == current)
                    {
                        int alt = distances[current] + edge.Weight;
                        if (alt < distances[edge.End])
                        {
                            distances[edge.End] = alt;
                            previous[edge.End] = current;
                        }
                    }
                }
            }

            if (distances[end] == int.MaxValue)
            {
                return (int.MaxValue, new List<Vertex>());
            }
            List<Vertex> path = new List<Vertex>();
            Vertex step = end;

            if (!previous.ContainsKey(end))
            {
                // Если конечная вершина не имеет предшественника, значит путь не найден
                return (int.MaxValue, new List<Vertex>());
            }

            while (step != null)
            {
                if (!previous.ContainsKey(step))
                {
                    // Если предшественник для текущей вершины не определен, прерываем цикл
                    break;
                }
                path.Insert(0, step);
                step = previous[step];
            }

            return (distances[end], path);

        }

        // Обработка события нажатия мыши на графе
        private void PanelGraph_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                highlightedEdges.Clear();
                return;
            }

            foreach (var vertex in vertices)
            {
                if (vertex.Contains(e.Location))
                {
                    if (firstClick)
                    {
                        previousSelectedVertex = null;
                        selectedVertex = vertex;
                        firstClick = false;
                    }
                    else
                    {
                        previousSelectedVertex = selectedVertex;
                        selectedVertex = vertex;
                    }
                    panelGraph.Invalidate();
                    return;
                }
            }

            // Если ни одна вершина не выбрана, сбрасываем все
            firstClick = true;
            previousSelectedVertex = null;
            selectedVertex = null;
            panelGraph.Invalidate();
        }

        private void PanelGraph_MouseUp(object sender, MouseEventArgs e)
        {
        }

        private void PanelGraph_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedVertex != null && e.Button == MouseButtons.Left)
            {
                selectedVertex.Position = e.Location;
                panelGraph.Invalidate();
            }
        }

        private void AddEdge()
        {
            // Проверяем, выбраны ли начальная и конечная вершины
            if (previousSelectedVertex == null || selectedVertex == null)
            {
                MessageBox.Show("Пожалуйста, выберите начальную и конечную вершины.");
                return;
            }

            // Ожидаем ввод веса дуги
            string input = Interaction.InputBox("Введите вес дуги", "Вес дуги", "1");

            if (int.TryParse(input, out int weight))
            {
                // Проверяем, существует ли дуга от предыдущей к выбранной вершине
                var existingEdge = edges.FirstOrDefault(edge => edge.Start == previousSelectedVertex && edge.End == selectedVertex);

                if (existingEdge == null)
                {
                    // Если дуги нет, добавляем новую дугу
                    edges.Add(new Edge(previousSelectedVertex, selectedVertex, weight));
                    previousSelectedVertex.AddNeighbor(selectedVertex);
                }
                else
                {
                    // Если дуга уже существует, обновляем её вес
                    existingEdge.Weight = weight;
                }

                // Обновляем значение в таблице смежности для направления previousSelectedVertex -> selectedVertex
                dgvIncidenceMatrix.Rows[vertices.IndexOf(previousSelectedVertex)].Cells[vertices.IndexOf(selectedVertex)].Value = weight;

                // Перерисовываем граф
                ResetSelectedVertices();
                panelGraph.Invalidate();
            }
            else
            {
                MessageBox.Show("Некорректный ввод веса дуги. Пожалуйста, введите целое число.");
            }
        }

        // Метод для отрисовки графа
        private void PanelGraph_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var edge in edges)
            {
                if (highlightedEdges.Contains(edge))
                {
                    edge.Draw(g, edges, edge.Weight, Color.Red);
                }
                else
                {
                    edge.Draw(g, edges, edge.Weight);
                }
            }

            foreach (var vertex in vertices)
            {
                if (vertex == previousSelectedVertex)
                {
                    vertex.Draw(g, Color.Green);
                }
                else if (vertex == selectedVertex)
                {
                    vertex.Draw(g, Color.Red);
                }
                else
                {
                    vertex.Draw(g);
                }
                g.DrawString(vertex.Id, SystemFonts.DefaultFont, Brushes.Black, vertex.Position.X - 10, vertex.Position.Y - 10);
            }

            // Рисуем все пакеты
            foreach (var packet in packets)
            {
                packet.Draw(g);
            }
        }



        // Удаление выбранной вершины
        private void RemoveVertex()
        {
            if (selectedVertex == null)
            {
                MessageBox.Show("Выберите вершину для удаления.");
                return;
            }

            edges.RemoveAll(edge => edge.Start == selectedVertex || edge.End == selectedVertex);

            vertices.Remove(selectedVertex);

            ResetSelectedVertices();
            UpdateIncidenceMatrix();
        }

        // Удаление дуги между выбранными вершинами
        private void RemoveEdge()
        {
            if (previousSelectedVertex == null || selectedVertex == null)
            {
                MessageBox.Show("Пожалуйста, выберите начальную и конечную вершины для удаления дуги.");
                return;
            }

            var edgeToRemove = edges.FirstOrDefault(edge => edge.Start == previousSelectedVertex && edge.End == selectedVertex);

            if (edgeToRemove != null)
            {
                edges.Remove(edgeToRemove);
                MessageBox.Show("Дуга удалена.");
                UpdateIncidenceMatrix();
            }
            else
            {
                MessageBox.Show("Дуга между этими вершинами не найдена.");
            }

            ResetSelectedVertices();
        }


        // Изменение веса дуги
        private void ChangeEdgeWeight()
        {
            if (previousSelectedVertex == null || selectedVertex == null)
            {
                MessageBox.Show("Пожалуйста, выберите начальную и конечную вершины для изменения веса дуги.");
                return;
            }

            var edgeToChange = edges.FirstOrDefault(edge => edge.Start == previousSelectedVertex && edge.End == selectedVertex);

            if (edgeToChange != null)
            {
                string input = Interaction.InputBox("Введите новый вес дуги", "Изменение веса дуги", edgeToChange.Weight.ToString());

                if (int.TryParse(input, out int newWeight))
                {
                    edgeToChange.Weight = newWeight;
                    MessageBox.Show("Вес дуги изменен.");
                    UpdateIncidenceMatrix();
                }
                else
                {
                    MessageBox.Show("Некорректный ввод веса.");
                }
            }
            else
            {
                MessageBox.Show("Дуга между этими вершинами не найдена.");
            }

            ResetSelectedVertices();
        }

        private void panelGraph_Paint_1(object sender, PaintEventArgs e)
        {

        }

        private void btn_random_routing_Click(object sender, EventArgs e)
        {
            // Проверяем, что начальная и конечная вершины выбраны
            if (previousSelectedVertex != null && selectedVertex != null)
            {
                // Создаем пакет с начальной вершиной и стратегией случайной маршрутизации
                CreatePacket(previousSelectedVertex, selectedVertex, RandomRouting, "Случайная маршрутизация", richTextBox1);
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите начальную и конечную вершины.");
            }
        }

        private void btn_avalanche_routing_Click(object sender, EventArgs e)
        {
            // Проверяем, что начальная и конечная вершины выбраны
            if (previousSelectedVertex != null && selectedVertex != null)
            {
                // Создаем пакет с начальной вершиной и стратегией лавинной маршрутизации
                Packet startPacket = new Packet(previousSelectedVertex, selectedVertex, AvalancheRoutingStrategy, "Лавинная маршрутизация", richTextBox1);

                // Добавляем начальный пакет в список
                packets.Add(startPacket);

                // Логируем создание первого пакета
                richTextBox1.AppendText($"Создан пакет {startPacket.Id} с типом Лавинная маршрутизация\n");
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите начальную и конечную вершины.");
            }
        }

    }
}
