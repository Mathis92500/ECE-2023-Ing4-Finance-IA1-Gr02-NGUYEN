using System;
using System.Collections.Generic;
using System.Text;

namespace Sudoku.TechniquesHumaines
{
    internal sealed  class BackTrackingState
    {
        public Cell? Cell { get; set; }

        public List<int> ExploredValues { get; set; }

        public int[][] Board { get; set; }


        public void Backtrack(Puzzle objPuzzle)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (objPuzzle[i, j].Value != Board[i][j])
                    {
                        objPuzzle[i, j].Set(Board[i][j]);
                    }
                }
            }
            objPuzzle.RefreshCandidates();
        }
    }
}