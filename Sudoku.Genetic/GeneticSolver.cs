using Sudoku.Shared;

namespace Sudoku.Genetic;
public class GeneticSolver: ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            //A faire: j'ai rajouter les packages GeneticSharp et GeneticSharp.Extensions,
            //il devrait être possible de reprendre une bonne partie du code documenté notamment dans les tests unitaire de GeneticSharp 
            return s.CloneSudoku();
        }

    }
