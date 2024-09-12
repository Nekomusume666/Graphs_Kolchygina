namespace TEST_GRAPH
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Windows.Forms;
    using Microsoft.VisualBasic;
    using System.Drawing.Drawing2D;

    public partial class Form1 : Form
    {
        private List<Vertex> vertices = new List<Vertex>();
        private List<Edge> edges = new List<Edge>();
        private Vertex selectedVertex = null;
        private bool firstClick = true;
        private int vertexCounter = 1; 
        private Vertex previousSelectedVertex = null; 

        public Form1()
        {
            InitializeComponent();


            panelGraph.MouseDown += PanelGraph_MouseDown;
            panelGraph.MouseUp += PanelGraph_MouseUp;
            panelGraph.MouseMove += PanelGraph_MouseMove;
            panelGraph.Paint += PanelGraph_Paint;
            InitContextMenu();
        }

        // Обработчик изменения значений в таблице (матрице смежности)
        private void DgvIncidenceMatrix_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что изменение произошло не в заголовках, а в ячейках с данными
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            // Получаем идентификаторы вершин (строки и столбца)
            string rowVertexId = dgvIncidenceMatrix.Rows[e.RowIndex].HeaderCell.Value.ToString();
            string colVertexId = dgvIncidenceMatrix.Columns[e.ColumnIndex].HeaderText;

            // Пробуем получить новое значение из ячейки
            if (int.TryParse(dgvIncidenceMatrix.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out int newWeight))
            {
                // Ищем вершины по идентификаторам
                var vertexRow = vertices.FirstOrDefault(v => v.Id == rowVertexId);
                var vertexCol = vertices.FirstOrDefault(v => v.Id == colVertexId);

                if (vertexRow == null || vertexCol == null) return;

                // Проверяем, есть ли уже существующая дуга от vertexRow к vertexCol
                var edge = edges.FirstOrDefault(ed => ed.Start == vertexRow && ed.End == vertexCol);
                var reverseEdge = edges.FirstOrDefault(ed => ed.Start == vertexCol && ed.End == vertexRow);

                if (newWeight != 0)
                {
                    if (edge != null)
                    {
                        // Если дуга существует, обновляем её вес
                        edge.Weight = newWeight;
                    }
                    else
                    {
                        // Если дуги нет, добавляем новую дугу
                        edges.Add(new Edge(vertexRow, vertexCol, newWeight));
                    }

                    // Обновляем значение в противоположной ячейке для симметрии
                    dgvIncidenceMatrix.Rows[e.ColumnIndex].Cells[e.RowIndex].Value = newWeight;

                    // Если есть обратная дуга, тоже обновляем её вес
                    if (reverseEdge != null)
                    {
                        reverseEdge.Weight = newWeight;
                    }
                }
                else
                {
                    // Если значение 0, удаляем существующую дугу (если она есть)
                    if (edge != null)
                    {
                        edges.Remove(edge);
                    }

                    // Если есть обратная дуга, удаляем её тоже
                    if (reverseEdge != null)
                    {
                        edges.Remove(reverseEdge);
                    }

                    // Обновляем противоположную ячейку
                    dgvIncidenceMatrix.Rows[e.ColumnIndex].Cells[e.RowIndex].Value = 0;
                }

                // Обновляем отображение графа
                panelGraph.Invalidate();
            }
        }


        // Обновление матрицы смежности
        private void UpdateIncidenceMatrix()
        {
            dgvIncidenceMatrix.Rows.Clear();
            dgvIncidenceMatrix.Columns.Clear();

            // Добавляем столбцы для каждой вершины
            foreach (var vertex in vertices)
            {
                dgvIncidenceMatrix.Columns.Add(vertex.Id, vertex.Id);
            }

            // Добавляем строки для каждой вершины
            foreach (var vertexRow in vertices)
            {
                var row = new DataGridViewRow();
                row.HeaderCell.Value = vertexRow.Id;

                foreach (var vertexCol in vertices)
                {
                    // Ищем дугу между вершинами
                    var edge = edges.FirstOrDefault(e => e.Start == vertexRow && e.End == vertexCol);

                    // Если дуга существует, добавляем её вес, иначе 0
                    int cellValue = edge != null ? edge.Weight : 0;

                    var cell = new DataGridViewTextBoxCell { Value = cellValue };
                    row.Cells.Add(cell);
                }

                dgvIncidenceMatrix.Rows.Add(row);
            }

            // Установка заголовков строк
            dgvIncidenceMatrix.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;

            // Добавляем обработчик события для изменения значений в таблице
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
            contextMenu.Items.Add("Найти кратчайший путь", null, (s, e) => FindShortestPath());
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

        // Поиск самого короткого пути
        private void FindShortestPath()
        {
            if (previousSelectedVertex == null || selectedVertex == null)
            {
                MessageBox.Show("Не выбраны начальная и конечная вершины!");
                return;
            }

            var (distance, path) = Dijkstra(previousSelectedVertex, selectedVertex);
            if (distance == int.MaxValue)
            {
                MessageBox.Show("Пути не существует.");
            }
            else
            {
                string pathStr = string.Join(" -> ", path.Select(v => v.Id));
                MessageBox.Show($"Кратчайший путь: {pathStr}\nДлина пути: {distance}");
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
            while (step != null)
            {
                path.Insert(0, step);
                step = previous[step];
            }

            return (distances[end], path);
        }

        // Обработка события нажатия мыши на графе
        private void PanelGraph_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) return;

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
                // Проверяем, существует ли уже дуга между этими вершинами
                var existingEdge = edges.FirstOrDefault(edge => edge.Start == previousSelectedVertex && edge.End == selectedVertex);
                var reverseEdge = edges.FirstOrDefault(edge => edge.Start == selectedVertex && edge.End == previousSelectedVertex);

                if (existingEdge == null)
                {
                    // Если дуги нет, добавляем новую дугу
                    edges.Add(new Edge(previousSelectedVertex, selectedVertex, weight));
                    dgvIncidenceMatrix.Rows[vertices.IndexOf(previousSelectedVertex)].Cells[vertices.IndexOf(selectedVertex)].Value = weight;
                }
                else
                {
                    // Если дуга уже существует, обновляем её вес
                    existingEdge.Weight = weight;
                    dgvIncidenceMatrix.Rows[vertices.IndexOf(previousSelectedVertex)].Cells[vertices.IndexOf(selectedVertex)].Value = weight;
                }

                if (reverseEdge == null)
                {
                    // Добавляем обратную дугу (если её нет)
                    edges.Add(new Edge(selectedVertex, previousSelectedVertex, weight));
                    dgvIncidenceMatrix.Rows[vertices.IndexOf(selectedVertex)].Cells[vertices.IndexOf(previousSelectedVertex)].Value = weight;
                }
                else
                {
                    // Обновляем вес обратной дуги
                    reverseEdge.Weight = weight;
                    dgvIncidenceMatrix.Rows[vertices.IndexOf(selectedVertex)].Cells[vertices.IndexOf(previousSelectedVertex)].Value = weight;
                }
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

            // Отрисовка всех дуг
            foreach (var edge in edges)
            {
                edge.Draw(g);
            }

            // Отрисовка всех вершин
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
        }

        // Удаление выбранной вершины
        private void RemoveVertex()
        {
            if (selectedVertex == null)
            {
                MessageBox.Show("Выберите вершину для удаления.");
                return;
            }

            // Удаляем все дуги, связанные с выбранной вершиной
            edges.RemoveAll(edge => edge.Start == selectedVertex || edge.End == selectedVertex);

            // Удаляем саму вершину
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

            // Находим дугу между выбранными вершинами
            var edgeToChange = edges.FirstOrDefault(edge => edge.Start == previousSelectedVertex && edge.End == selectedVertex);

            if (edgeToChange != null)
            {
                // Ожидаем новый вес дуги
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

    }
}
