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
        private List<Edge> highlightedEdges = new List<Edge>();


        public Form1()
        {
            InitializeComponent();


            panelGraph.MouseDown += PanelGraph_MouseDown;
            panelGraph.MouseUp += PanelGraph_MouseUp;
            panelGraph.MouseMove += PanelGraph_MouseMove;
            panelGraph.Paint += PanelGraph_Paint;
            InitContextMenu();
        }

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

        // Пример вызова метода сброса в контекстном меню
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
                // Проверяем, существует ли дуга от предыдущей к выбранной вершине
                var existingEdge = edges.FirstOrDefault(edge => edge.Start == previousSelectedVertex && edge.End == selectedVertex);

                if (existingEdge == null)
                {
                    // Если дуги нет, добавляем новую дугу
                    edges.Add(new Edge(previousSelectedVertex, selectedVertex, weight));
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


    }
}
