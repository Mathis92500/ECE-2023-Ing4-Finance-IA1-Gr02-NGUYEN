using Sudoku.Shared;
namespace Sudoku.DlxLib;
public class DlxLibSolver:ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            //A faire: récupérer les cellules du Masque de sudoku à résoudre dans s.Cells et utiliser le code fourni dans le sujet pour résoudre le sudoku 
            return s.CloneSudoku();
        }
}
 