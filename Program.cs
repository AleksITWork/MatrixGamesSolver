using System;
using System.Collections.Generic;
using System.Linq;

namespace GameTheorySolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Решатель теории игр");
            Console.WriteLine("==================");
            
            // Получение размеров матрицы
            Console.Write("Введите количество строк (M): ");
            string? mInput = Console.ReadLine();
            if (string.IsNullOrEmpty(mInput) || !int.TryParse(mInput, out int m))
            {
                Console.WriteLine("Некорректный ввод количества строк. Используется значение по умолчанию 2.");
                m = 2;
            }
            
            Console.Write("Введите количество столбцов (N): ");
            string? nInput = Console.ReadLine();
            if (string.IsNullOrEmpty(nInput) || !int.TryParse(nInput, out int n))
            {
                Console.WriteLine("Некорректный ввод количества столбцов. Используется значение по умолчанию 2.");
                n = 2;
            }
            
            // Создание и заполнение матрицы
            double[,] matrix = new double[m, n];
            FillMatrix(matrix, m, n);
            
            // Отображение матрицы
            Console.WriteLine("\nМатрица платежей:");
            PrintMatrix(matrix, m, n);
            
            // Найти верхнюю и нижнюю цены игры
            var (lowerPrice, upperPrice) = FindGamePrices(matrix, m, n);
            Console.WriteLine($"\nНижняя цена игры (максимин): {lowerPrice}");
            Console.WriteLine($"Верхняя цена игры (минимакс): {upperPrice}");
            
            // Найти седловые точки
            var saddlePoints = FindSaddlePoints(matrix, m, n);
            
            if (saddlePoints.Count > 0)
            {
                Console.WriteLine("\nНайдены седловые точки:");
                foreach (var point in saddlePoints)
                {
                    Console.WriteLine($"  Позиция: ({point.Row}, {point.Col}), Значение: {matrix[point.Row, point.Col]}");
                }
                
                Console.WriteLine("\nРешения в чистых стратегиях:");
                foreach (var point in saddlePoints)
                {
                    Console.WriteLine($"  Игрок A выбирает стратегию {point.Row + 1}, Игрок B выбирает стратегию {point.Col + 1}");
                }
            }
            else
            {
                Console.WriteLine("\nСедловые точки не найдены.");
                
                // Попытка упростить матрицу, исключив доминируемые строки и столбцы
                Console.WriteLine("\nУпрощение матрицы путем исключения доминируемых стратегий...");
                var (simplifiedMatrix, removedRows, removedCols) = SimplifyMatrix(matrix, m, n);
                
                if (simplifiedMatrix.GetLength(0) == 2 && simplifiedMatrix.GetLength(1) == 2)
                {
                    Console.WriteLine("\nУпрощенная матрица 2x2. Решение в смешанных стратегиях:");
                    PrintMatrix(simplifiedMatrix, simplifiedMatrix.GetLength(0), simplifiedMatrix.GetLength(1));
                    SolveMixedStrategy(simplifiedMatrix, m, n, removedRows, removedCols);
                }
                else
                {
                    Console.WriteLine($"\nРазмер упрощенной матрицы: {simplifiedMatrix.GetLength(0)}x{simplifiedMatrix.GetLength(1)}");
                    Console.WriteLine("Матрица не 2x2, невозможно решить в смешанных стратегиях с этой реализацией.");
                    PrintMatrix(simplifiedMatrix, simplifiedMatrix.GetLength(0), simplifiedMatrix.GetLength(1));
                }
            }
        }
        
        static void FillMatrix(double[,] matrix, int m, int n)
        {
            Console.WriteLine("\nВведите элементы матрицы по строкам:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"Введите элемент [{i+1},{j+1}]: ");
                    string? input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input) || !double.TryParse(input, out double value))
                    {
                        Console.WriteLine("Некорректный ввод. Используется значение по умолчанию 0.0.");
                        value = 0.0;
                    }
                    matrix[i, j] = value;
                }
            }
        }
        
        static void PrintMatrix(double[,] matrix, int m, int n)
        {
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{matrix[i, j],8:F2}");
                }
                Console.WriteLine();
            }
        }
        
        static (double lowerPrice, double upperPrice) FindGamePrices(double[,] matrix, int m, int n)
        {
            // Find maximin (lower price)
            double maximin = double.NegativeInfinity;
            for (int i = 0; i < m; i++)
            {
                double minInRow = double.PositiveInfinity;
                for (int j = 0; j < n; j++)
                {
                    if (matrix[i, j] < minInRow)
                        minInRow = matrix[i, j];
                }
                if (minInRow > maximin)
                    maximin = minInRow;
            }
            
            // Find minimax (upper price)
            double minimax = double.PositiveInfinity;
            for (int j = 0; j < n; j++)
            {
                double maxInCol = double.NegativeInfinity;
                for (int i = 0; i < m; i++)
                {
                    if (matrix[i, j] > maxInCol)
                        maxInCol = matrix[i, j];
                }
                if (maxInCol < minimax)
                    minimax = maxInCol;
            }
            
            return (maximin, minimax);
        }
        
        static List<SaddlePoint> FindSaddlePoints(double[,] matrix, int m, int n)
        {
            List<SaddlePoint> saddlePoints = new List<SaddlePoint>();
            
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    double element = matrix[i, j];
                    
                    // Check if it's minimum in its row
                    bool isMinInRow = true;
                    for (int k = 0; k < n; k++)
                    {
                        if (matrix[i, k] < element)
                        {
                            isMinInRow = false;
                            break;
                        }
                    }
                    
                    // Check if it's maximum in its column
                    bool isMaxInCol = true;
                    for (int k = 0; k < m; k++)
                    {
                        if (matrix[k, j] > element)
                        {
                            isMaxInCol = false;
                            break;
                        }
                    }
                    
                    if (isMinInRow && isMaxInCol)
                    {
                        saddlePoints.Add(new SaddlePoint { Row = i, Col = j });
                    }
                }
            }
            
            return saddlePoints;
        }
        
        static (double[,] simplifiedMatrix, List<int> removedRows, List<int> removedCols) SimplifyMatrix(double[,] matrix, int m, int n)
        {
            // Convert to jagged array for easier manipulation
            double[][] jaggedMatrix = new double[m][];
            for (int i = 0; i < m; i++)
            {
                jaggedMatrix[i] = new double[n];
                for (int j = 0; j < n; j++)
                {
                    jaggedMatrix[i][j] = matrix[i, j];
                }
            }
            
            List<int> rowsToRemove = new List<int>();
            List<int> colsToRemove = new List<int>();
            
            bool changed;
            do
            {
                changed = false;
                
                // Find dominated rows
                List<int> currentRows = new List<int>();
                for (int i = 0; i < m; i++)
                {
                    if (!rowsToRemove.Contains(i))
                        currentRows.Add(i);
                }
                
                for (int i = 0; i < currentRows.Count; i++)
                {
                    for (int k = i + 1; k < currentRows.Count; k++)
                    {
                        int row1 = currentRows[i];
                        int row2 = currentRows[k];
                        
                        if (!rowsToRemove.Contains(row1) && !rowsToRemove.Contains(row2))
                        {
                            // Check if row1 dominates row2 (row1 >= row2 for all elements)
                            bool row1DominatesRow2 = true;
                            bool row2DominatesRow1 = true;
                            
                            for (int j = 0; j < n; j++)
                            {
                                if (!colsToRemove.Contains(j))
                                {
                                    if (jaggedMatrix[row1][j] < jaggedMatrix[row2][j])
                                        row1DominatesRow2 = false;
                                    if (jaggedMatrix[row1][j] > jaggedMatrix[row2][j])
                                        row2DominatesRow1 = false;
                                }
                            }
                            
                            if (row1DominatesRow2 && !row2DominatesRow1)
                            {
                                if (!rowsToRemove.Contains(row2))
                                {
                                    rowsToRemove.Add(row2);
                                    changed = true;
                                }
                            }
                            else if (row2DominatesRow1 && !row1DominatesRow2)
                            {
                                if (!rowsToRemove.Contains(row1))
                                {
                                    rowsToRemove.Add(row1);
                                    changed = true;
                                }
                            }
                        }
                    }
                }
                
                // Find dominated columns
                List<int> currentCols = new List<int>();
                for (int j = 0; j < n; j++)
                {
                    if (!colsToRemove.Contains(j))
                        currentCols.Add(j);
                }
                
                for (int j = 0; j < currentCols.Count; j++)
                {
                    for (int k = j + 1; k < currentCols.Count; k++)
                    {
                        int col1 = currentCols[j];
                        int col2 = currentCols[k];
                        
                        if (!colsToRemove.Contains(col1) && !colsToRemove.Contains(col2))
                        {
                            // Check if col1 dominates col2 (col1 <= col2 for all elements, because B wants to minimize)
                            bool col1DominatesCol2 = true;
                            bool col2DominatesCol1 = true;
                            
                            for (int i = 0; i < m; i++)
                            {
                                if (!rowsToRemove.Contains(i))
                                {
                                    if (jaggedMatrix[i][col1] > jaggedMatrix[i][col2])
                                        col1DominatesCol2 = false;
                                    if (jaggedMatrix[i][col1] < jaggedMatrix[i][col2])
                                        col2DominatesCol1 = false;
                                }
                            }
                            
                            if (col1DominatesCol2 && !col2DominatesCol1)
                            {
                                if (!colsToRemove.Contains(col2))
                                {
                                    colsToRemove.Add(col2);
                                    changed = true;
                                }
                            }
                            else if (col2DominatesCol1 && !col1DominatesCol2)
                            {
                                if (!colsToRemove.Contains(col1))
                                {
                                    colsToRemove.Add(col1);
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            } while (changed);
            
            // Create new simplified matrix
            List<int> remainingRows = new List<int>();
            List<int> remainingCols = new List<int>();
            
            for (int i = 0; i < m; i++)
            {
                if (!rowsToRemove.Contains(i))
                    remainingRows.Add(i);
            }
            
            for (int j = 0; j < n; j++)
            {
                if (!colsToRemove.Contains(j))
                    remainingCols.Add(j);
            }
            
            double[,] simplifiedMatrix = new double[remainingRows.Count, remainingCols.Count];
            
            for (int i = 0; i < remainingRows.Count; i++)
            {
                for (int j = 0; j < remainingCols.Count; j++)
                {
                    simplifiedMatrix[i, j] = matrix[remainingRows[i], remainingCols[j]];
                }
            }
            
            Console.WriteLine($"Матрица упрощена с {m}x{n} до {remainingRows.Count}x{remainingCols.Count}");
            if (rowsToRemove.Count > 0)
            {
                Console.WriteLine($"Удаленные строки (доминируемые стратегии игрока A): [{string.Join(", ", rowsToRemove.Select(x => x + 1))}]");
            }
            if (colsToRemove.Count > 0)
            {
                Console.WriteLine($"Удаленные столбцы (доминируемые стратегии игрока B): [{string.Join(", ", colsToRemove.Select(x => x + 1))}]");
            }
            
            return (simplifiedMatrix, rowsToRemove, colsToRemove);
        }
        
        static void SolveMixedStrategy(double[,] matrix, int originalRows, int originalCols, List<int> removedRows, List<int> removedCols)
        {
            // For a 2x2 matrix [[a, b], [c, d]]
            // Optimal mixed strategy for Player A: p = (d-c)/((a+d)-(b+c))
            // Optimal mixed strategy for Player B: q = (d-b)/((a+d)-(b+c))
            // Value of the game: v = (ad-bc)/((a+d)-(b+c))
            
            if (matrix.GetLength(0) != 2 || matrix.GetLength(1) != 2)
            {
                Console.WriteLine("Метод работает только для 2x2 матриц.");
                return;
            }
            
            double a = matrix[0, 0];
            double b = matrix[0, 1];
            double c = matrix[1, 0];
            double d = matrix[1, 1];
            
            double denominator = (a + d) - (b + c);
            
            if (Math.Abs(denominator) < 1e-9)
            {
                Console.WriteLine("Игра не имеет решения в смешанных стратегиях (знаменатель равен нулю).");
                return;
            }
            
            double p = (d - c) / denominator;  // Probability of Player A choosing first strategy in simplified matrix
            double q = (d - b) / denominator;  // Probability of Player B choosing first strategy in simplified matrix
            double value = (a * d - b * c) / denominator;
            
            // Create arrays to store probabilities for all original strategies
            double[] playerA_probs = new double[originalRows];
            double[] playerB_probs = new double[originalCols];
            
            // Initialize all probabilities to 0
            for (int i = 0; i < originalRows; i++)
            {
                playerA_probs[i] = 0.0;
            }
            for (int j = 0; j < originalCols; j++)
            {
                playerB_probs[j] = 0.0;
            }
            
            // Assign probabilities to removed strategies (0)
            foreach (int removedRow in removedRows)
            {
                playerA_probs[removedRow] = 0.0;
            }
            
            foreach (int removedCol in removedCols)
            {
                playerB_probs[removedCol] = 0.0;
            }
            
            // Assign probabilities to remaining strategies
            // Find positions of remaining strategies in original matrix
            List<int> remainingRows = new List<int>();
            List<int> remainingCols = new List<int>();
            
            for (int i = 0; i < originalRows; i++)
            {
                if (!removedRows.Contains(i))
                    remainingRows.Add(i);
            }
            
            for (int j = 0; j < originalCols; j++)
            {
                if (!removedCols.Contains(j))
                    remainingCols.Add(j);
            }
            
            // Assign calculated probabilities to the appropriate positions
            if (remainingRows.Count >= 2)
            {
                playerA_probs[remainingRows[0]] = p;
                playerA_probs[remainingRows[1]] = 1 - p;
            }
            
            if (remainingCols.Count >= 2)
            {
                playerB_probs[remainingCols[0]] = q;
                playerB_probs[remainingCols[1]] = 1 - q;
            }
            
            Console.WriteLine($"Оптимальная смешанная стратегия для Игрока A (все исходные стратегии):");
            for (int i = 0; i < originalRows; i++)
            {
                Console.WriteLine($"  Стратегия {i + 1}: {playerA_probs[i]:F4}" + 
                    (removedRows.Contains(i) ? " (доминируемая)" : ""));
            }
            
            Console.WriteLine($"Оптимальная смешанная стратегия для Игрока B (все исходные стратегии):");
            for (int j = 0; j < originalCols; j++)
            {
                Console.WriteLine($"  Стратегия {j + 1}: {playerB_probs[j]:F4}" + 
                    (removedCols.Contains(j) ? " (доминируемая)" : ""));
            }
            
            Console.WriteLine($"Значение игры: {value:F4}");
        }
    }
    
    struct SaddlePoint
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }
}