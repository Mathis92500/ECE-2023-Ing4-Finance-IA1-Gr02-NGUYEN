using Sudoku.Shared;

int bonjour;

namespace Sudoku.TechniquesHumaines
{
    public class TechniquesHumainesEmptySolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            return s.CloneSudoku();
            
        }

    }
}
