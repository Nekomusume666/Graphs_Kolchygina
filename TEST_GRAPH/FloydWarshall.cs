using System;

namespace TEST_GRAPH
{
    public static class FloydWarshall
    {
        public static void ComputeShortestPaths(int[,] graph, out int[,] distances, out int[,] predecessors)
        {
            int n = graph.GetLength(0);
            distances = new int[n, n];
            predecessors = new int[n, n];

            // Инициализация матриц
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        distances[i, j] = 0;
                        predecessors[i, j] = -1;
                    }
                    else if (graph[i, j] != int.MaxValue)
                    {
                        distances[i, j] = graph[i, j];
                        predecessors[i, j] = i;
                    }
                    else
                    {
                        distances[i, j] = int.MaxValue;
                        predecessors[i, j] = -1;
                    }
                }
            }

            // Основной цикл алгоритма Флойда
            for (int k = 0; k < n; k++)
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (distances[i, k] != int.MaxValue && distances[k, j] != int.MaxValue &&
                            distances[i, j] > distances[i, k] + distances[k, j])
                        {
                            distances[i, j] = distances[i, k] + distances[k, j];
                            predecessors[i, j] = predecessors[k, j];
                        }
                    }
                }
            }

            // Проверка на наличие отрицательных циклов
            for (int i = 0; i < n; i++)
            {
                if (distances[i, i] < 0)
                {
                    throw new Exception("Граф содержит отрицательный цикл.");
                }
            }
        }
    }
}
