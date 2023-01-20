import clr
clr.AddReference("Sudoku.Shared")
clr.AddReference("Sudoku.Recuit")
from Sudoku.Recuit import SudokuRecuitSolver
netSolver = SudokuRecuitSolver()
solvedSudoku = netSolver.Solve(sudoku)