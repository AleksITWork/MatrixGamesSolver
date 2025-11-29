using System;
using System.Collections.Generic;
using System.Linq;

// Простой тест для проверки логики модифицированной программы
class TestModifiedLogic
{
    static void Main(string[] args)
    {
        Console.WriteLine("Тестирование модифицированной логики:");
        Console.WriteLine("==================================");
        
        // Пример матрицы 3x3, где одна стратегия доминируется
        double[,] matrix = {
            {3, 2, 4},  // Стратегия 1 игрока A
            {1, 5, 2},  // Стратегия 2 игрока A (доминируется первой)
            {2, 4, 3}   // Стратегия 3 игрока A
        };
        
        int m = 3, n = 3;
        
        Console.WriteLine("\nИсходная матрица:");
        PrintMatrix(matrix, m, n);
        
        // Найдем доминируемые стратегии
        var (simplifiedMatrix, removedRows, removedCols) = SimplifyMatrix(matrix, m, n);
        
        Console.WriteLine($"\nУпрощенная матрица (размер {simplifiedMatrix.GetLength(0)}x{simplifiedMatrix.GetLength(1)}):");
        PrintMatrix(simplifiedMatrix, simplifiedMatrix.GetLength(0), simplifiedMatrix.GetLength(1));
        
        if (removedRows.Count > 0)
        {
            Console.WriteLine($"Удаленные строки (доминируемые стратегии игрока A): [{string.Join(", ", removedRows.Select(x => x + 1))}]");
        }
        if (removedCols.Count > 0)
        {
            Console.WriteLine($"Удаленные столбцы (доминируемые стратегии игрока B): [{string.Join(", ", removedCols.Select(x => x + 1))}]");
        }
        
        // Решим для упрощенной 2x2 матрицы и вернем вероятности для всех стратегий
        if (simplifiedMatrix.GetLength(0) == 2 && simplifiedMatrix.GetLength(1) == 2)
        {
            SolveMixedStrategy(simplifiedMatrix, m, n, removedRows, removedCols);
        }
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
        
        return (simplifiedMatrix, rowsToRemove, colsToRemove);
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
        
        Console.WriteLine($"\nОптимальная смешанная стратегия для Игрока A (все исходные стратегии):");
        for (int i = 0; i < originalRows; i++)
        {
            Console.WriteLine($"  Стратегия {i + 1}: {playerA_probs[i]:F4}" + 
                (removedRows.Contains(i) ? " (доминируемая)" : ""));
        }
        
        Console.WriteLine($"\nОптимальная смешанная стратегия для Игрока B (все исходные стратегии):");
        for (int j = 0; j < originalCols; j++)
        {
            Console.WriteLine($"  Стратегия {j + 1}: {playerB_probs[j]:F4}" + 
                (removedCols.Contains(j) ? " (доминируемая)" : ""));
        }
        
        Console.WriteLine($"\nЗначение игры: {value:F4}");
    }
}