using System;
using System.Collections.Generic;
using System.Linq;

namespace GameTheorySolver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Game Theory Solver");
            Console.WriteLine("==================");
            
            // Get matrix dimensions
            Console.Write("Enter number of rows (M): ");
            string? mInput = Console.ReadLine();
            if (string.IsNullOrEmpty(mInput) || !int.TryParse(mInput, out int m))
            {
                Console.WriteLine("Invalid input for number of rows. Using default value 2.");
                m = 2;
            }
            
            Console.Write("Enter number of columns (N): ");
            string? nInput = Console.ReadLine();
            if (string.IsNullOrEmpty(nInput) || !int.TryParse(nInput, out int n))
            {
                Console.WriteLine("Invalid input for number of columns. Using default value 2.");
                n = 2;
            }
            
            // Create and fill the matrix
            double[,] matrix = new double[m, n];
            FillMatrix(matrix, m, n);
            
            // Display the matrix
            Console.WriteLine("\nPayment Matrix:");
            PrintMatrix(matrix, m, n);
            
            // Find upper and lower prices of the game
            var (lowerPrice, upperPrice) = FindGamePrices(matrix, m, n);
            Console.WriteLine($"\nLower price of the game (maximin): {lowerPrice}");
            Console.WriteLine($"Upper price of the game (minimax): {upperPrice}");
            
            // Find saddle points
            var saddlePoints = FindSaddlePoints(matrix, m, n);
            
            if (saddlePoints.Count > 0)
            {
                Console.WriteLine("\nSaddle points found:");
                foreach (var point in saddlePoints)
                {
                    Console.WriteLine($"  Position: ({point.Row}, {point.Col}), Value: {matrix[point.Row, point.Col]}");
                }
                
                Console.WriteLine("\nSolutions in pure strategies:");
                foreach (var point in saddlePoints)
                {
                    Console.WriteLine($"  Player A chooses strategy {point.Row + 1}, Player B chooses strategy {point.Col + 1}");
                }
            }
            else
            {
                Console.WriteLine("\nNo saddle points found.");
                
                // Try to simplify the matrix by eliminating dominated rows and columns
                Console.WriteLine("\nSimplifying matrix by eliminating dominated strategies...");
                var simplifiedMatrix = SimplifyMatrix(matrix, m, n);
                
                if (simplifiedMatrix.GetLength(0) == 2 && simplifiedMatrix.GetLength(1) == 2)
                {
                    Console.WriteLine("\nSimplified matrix is 2x2. Solving in mixed strategies:");
                    PrintMatrix(simplifiedMatrix, simplifiedMatrix.GetLength(0), simplifiedMatrix.GetLength(1));
                    SolveMixedStrategy(simplifiedMatrix);
                }
                else
                {
                    Console.WriteLine($"\nSimplified matrix size: {simplifiedMatrix.GetLength(0)}x{simplifiedMatrix.GetLength(1)}");
                    Console.WriteLine("Matrix is not 2x2, cannot solve in mixed strategies with this implementation.");
                    PrintMatrix(simplifiedMatrix, simplifiedMatrix.GetLength(0), simplifiedMatrix.GetLength(1));
                }
            }
        }
        
        static void FillMatrix(double[,] matrix, int m, int n)
        {
            Console.WriteLine("\nEnter matrix elements row by row:");
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"Enter element [{i+1},{j+1}]: ");
                    string? input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input) || !double.TryParse(input, out double value))
                    {
                        Console.WriteLine("Invalid input. Using default value 0.0.");
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
        
        static double[,] SimplifyMatrix(double[,] matrix, int m, int n)
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
            
            Console.WriteLine($"Matrix simplified from {m}x{n} to {remainingRows.Count}x{remainingCols.Count}");
            
            return simplifiedMatrix;
        }
        
        static void SolveMixedStrategy(double[,] matrix)
        {
            // For a 2x2 matrix [[a, b], [c, d]]
            // Optimal mixed strategy for Player A: p = (d-c)/((a+d)-(b+c))
            // Optimal mixed strategy for Player B: q = (d-b)/((a+d)-(b+c))
            // Value of the game: v = (ad-bc)/((a+d)-(b+c))
            
            double a = matrix[0, 0];
            double b = matrix[0, 1];
            double c = matrix[1, 0];
            double d = matrix[1, 1];
            
            double denominator = (a + d) - (b + c);
            
            if (Math.Abs(denominator) < 1e-9)
            {
                Console.WriteLine("The game has no solution in mixed strategies (denominator is zero).");
                return;
            }
            
            double p = (d - c) / denominator;  // Probability of Player A choosing first strategy
            double q = (d - b) / denominator;  // Probability of Player B choosing first strategy
            double value = (a * d - b * c) / denominator;
            
            Console.WriteLine($"Optimal mixed strategy for Player A:");
            Console.WriteLine($"  Strategy 1: {p:F4}, Strategy 2: {1 - p:F4}");
            Console.WriteLine($"Optimal mixed strategy for Player B:");
            Console.WriteLine($"  Strategy 1: {q:F4}, Strategy 2: {1 - q:F4}");
            Console.WriteLine($"Value of the game: {value:F4}");
        }
    }
    
    struct SaddlePoint
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }
}