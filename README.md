# Game Theory Solver

This C# application implements game theory algorithms for solving two-player zero-sum games. It provides the following functionality:

## Features

1. **Matrix Input**: Fill a payment matrix of size MxN (M rows, N columns) with values entered by the user
2. **Game Price Calculation**: Find the upper and lower prices of the game (maximin and minimax)
3. **Saddle Point Detection**: Identify all saddle points in the matrix and provide solutions in pure strategies
4. **Matrix Simplification**: Eliminate dominated strategies (rows and columns) to simplify the matrix
5. **Mixed Strategy Solution**: For 2x2 matrices, solve the game in mixed strategies

## Game Theory Concepts

- **Maximin**: The maximum of the minimum payoffs for Player A (row player) - the best guaranteed outcome
- **Minimax**: The minimum of the maximum payoffs for Player B (column player) - the worst case scenario
- **Saddle Point**: An element that is simultaneously the minimum of its row and the maximum of its column
- **Dominated Strategy**: A strategy that is always worse than another strategy regardless of the opponent's choice

## How It Works

1. User inputs matrix dimensions M and N
2. User fills the MxN matrix with numerical values
3. The program calculates maximin and minimax values
4. If maximin equals minimax, saddle points exist and pure strategy solutions are provided
5. If no saddle points exist, the program attempts to simplify the matrix by removing dominated strategies
6. If the simplified matrix is 2x2, it solves for mixed strategies; otherwise, it reports that mixed strategy solution isn't implemented for larger matrices

## Mixed Strategy Calculation

For a 2x2 matrix [[a, b], [c, d]]:
- Optimal mixed strategy for Player A: p = (d-c)/((a+d)-(b+c))
- Optimal mixed strategy for Player B: q = (d-b)/((a+d)-(b+c))
- Value of the game: v = (ad-bc)/((a+d)-(b+c))

## Usage

To run the application:
```bash
dotnet run
```

Then follow the prompts to enter:
1. Number of rows (M)
2. Number of columns (N)
3. Matrix elements row by row

## Example

For a 2x2 matrix:
```
[1 4]
[3 2]
```

- No saddle point exists (maximin=2, minimax=3)
- The program will solve using mixed strategies:
  - Player A: Strategy 1 with probability 0.25, Strategy 2 with probability 0.75
  - Player B: Strategy 1 with probability 0.50, Strategy 2 with probability 0.50
  - Value of the game: 2.5