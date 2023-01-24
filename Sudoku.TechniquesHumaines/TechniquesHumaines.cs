using Sudoku.Shared;

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Sudoku.TechniquesHumaines
{
    public class TechniquesHumainesSolver : ISudokuSolver
    {
        public SudokuGrid Solve(SudokuGrid s)
        {
            s = Solve2(s);
            return s;
        }
        public SudokuGrid Solve2(SudokuGrid s)
        {

            bool solved = false;
            bool full = false; // If this is true after a segment, the puzzle is solved and we can break
            bool deadEnd = false;

            List<List<int>> list_cell = new List<List<int>>();
            foreach (var i in System.Linq.Enumerable.Range(0, 9))
            {
                var ligne = new List<int>(9);
                list_cell.Add(ligne);
                foreach (var j in System.Linq.Enumerable.Range(0, 9))
                {
                    ligne.Add(s.Cells[j][ i]);

                }
            }

            var monTableau = list_cell.Select(l => l.ToArray()).ToArray();
            var monPuzzle = new Puzzle(monTableau, false);
            var monSolver = new Solver(monPuzzle);
            //monSolver.DoWork(this, new System.ComponentModel.DoWorkEventArgs(null));

            List<Cell> allCells = null;
            Stack<BackTrackingState> exploredCellValues = null;

            monPuzzle.RefreshCandidates();
            do
            {
                deadEnd = false;

                // First we do human inference
                do
                {
                    full = true;

                    bool changed = false;
                    // Check for naked singles or a completed puzzle
                    for (int x = 0; x < 9; x++)
                    {
                        for (int y = 0; y < 9; y++)
                        {
                            Cell cell = monPuzzle[x, y];
                            if (cell.Value == 0)
                            {
                                full = false;
                                // Check for naked singles
                                int[] a = cell.Candidates.ToArray(); // Copy
                                if (a.Length == 1)
                                {
                                    cell.Set(a[0]);
                                    changed = true;
                                }
                            }
                        }
                    }
                    // Solved or failed to solve
                    if (full || (!changed && !monSolver.RunTechnique()))
                    {
                        break;
                    }
                } while (true);

                full = monPuzzle.Rows.All(row => row.All(c => c.Value != 0));
                //full = s.SetCell(x, y, monPuzzle.Rows[x][y].Value);

                // If puzzle isn't full, we do exploration
                if (!full)
                {
                    // Les Sudokus les plus difficiles ne peuvent pas être résolus avec un stylo bille, c'est à dire en inférence pure.
                    // Il va falloir lacher le stylo bille et prendre le crayon à papier et la gomme pour commencer une exploration fondée sur des hypothèses avec possible retour en arrière
                    if (allCells == null)
                    {
                        allCells = monPuzzle.Rows.SelectMany((row, rowIdx) => row).ToList();
                        exploredCellValues = new Stack<BackTrackingState>();
                    }
                    //puzzle.RefreshCandidates();

                    // Pour accélérer l'exploration et éviter de traverser la feuille en gommant trop souvent, on va utiliser les heuristiques des problèmes à satisfaction de contraintes
                    // cf. les slides et le problème du "coffre de voiture" abordé en cours

                    //heuristique MRV
                    var minCandidates = allCells.Min(cell => cell.Candidates.Count > 0 ? cell.Candidates.Count : int.MaxValue);

                    if (minCandidates != int.MaxValue)
                    {
                        // Utilisation de l'heuristique Deg: de celles qui ont le moins de candidats à égalité, on choisi celle la plus contraignante, celle qui a le plus de voisins (on pourrait faire mieux avec le nombre de candidats en commun avec ses voisins)
                        var candidateCells = allCells.Where(cell => cell.Candidates.Count == minCandidates);
                        //var degrees = candidateCells.Select(candidateCell => new {Cell = candidateCell, Degree = candidateCell.GetCellsVisible().Aggregate(0, (sum, neighbour) => sum + neighbour.Candidates.Count) });
                        var degrees = candidateCells.Select(candidateCell => new { Cell = candidateCell, Degree = candidateCell.GetCellsVisible().Count(c => c.Value == 0) }).ToList();
                        //var targetCell = list_cell.First(cell => cell.Candidates.Count == minCandidates);
                        var maxDegree = degrees.Max(deg1 => deg1.Degree);
                        var targetCell = degrees.First(deg => deg.Degree == maxDegree).Cell;

                        //dernière exploration pour ne pas se mélanger les pinceaux

                        BackTrackingState currentlyExploredCellValues;
                        if (exploredCellValues.Count == 0 || !exploredCellValues.Peek().Cell.Equals(targetCell))
                        {
							int[][] myBoard = new int[9][];
							for (int x = 0; x < 9; x++)
							{
								myBoard[x] = new int[9];
								for (int y = 0; y < 9; y++)
								{
									myBoard[x][y] = monPuzzle._board[x][y].Value;
								}
							}
                            currentlyExploredCellValues = new BackTrackingState() { Board = myBoard, Cell = targetCell, ExploredValues = new List<int>() };
                            exploredCellValues.Push(currentlyExploredCellValues);
                        }
                        else
                        {
                            currentlyExploredCellValues = exploredCellValues.Peek();
                        }


                        //utilisation de l'heuristique LCV: on choisi la valeur la moins contraignante pour les voisins
                        var candidateValues = targetCell.Candidates.Where(i => !currentlyExploredCellValues.ExploredValues.Contains(i));
                        var neighbourood = targetCell.GetCellsVisible();
                        var valueConstraints = candidateValues.Select(v => new
                        {
                            Value = v,
                            ContraintNb = neighbourood.Count(neighboor => neighboor.Candidates.Contains(v))
                        }).ToList();
                        var minContraints = valueConstraints.Min(vc => vc.ContraintNb);
                        var exploredValue = valueConstraints.First(vc => vc.ContraintNb == minContraints).Value;
                        currentlyExploredCellValues.ExploredValues.Add(exploredValue);
                        targetCell.Set(exploredValue);
                        //targetCell.Set(exploredValue, true);

                    }
                    else
                    {
                        //Plus de candidats possibles, on atteint un cul-de-sac
                        if (monPuzzle.IsValid())
                        {
                            solved = true;
                        }
                        else
                        {
                            deadEnd = true;
                        }


                        //deadEnd = true;
                    }
                }
                else
                {
                    //If puzzle is full, it's either solved or a deadend
                    if (monPuzzle.IsValid())
                    {
                        solved = true;
                    }
                    else
                    {
                        deadEnd = true;
                    }
                }


                if (deadEnd)
                {
                    //On se retrouve bloqué, il faut gommer et tenter d'autres hypothèses
                    BackTrackingState currentlyExploredCellValues = exploredCellValues.Peek();
                    //On annule la dernière assignation
                    currentlyExploredCellValues.Backtrack(monPuzzle);
                    var targetCell = currentlyExploredCellValues.Cell;
                    //targetCell.Set(0, true);
                    while (targetCell.Candidates.All(i => currentlyExploredCellValues.ExploredValues.Contains(i)))
                    {
                        //on a testé toutes les valeurs possibles, On est à un cul de sac, il faut revenir en arrière
                        exploredCellValues.Pop();
                        if (exploredCellValues.Count == 0)
                        {
                            Debug.WriteLine("bug in the algorithm techniques humaines");
                        }
                        currentlyExploredCellValues = exploredCellValues.Peek();
                        //On annule la dernière assignation
                        currentlyExploredCellValues.Backtrack(monPuzzle);
                        targetCell = currentlyExploredCellValues.Cell;
                        //targetCell.Set(0, true);
                    }
                    // D'autres valeurs sont possible pour la cellule courante, on les tente
                    //utilisation de l'heuristique LCV
                    var candidateValues = targetCell.Candidates.Where(i => !currentlyExploredCellValues.ExploredValues.Contains(i));
                    var neighbourood = targetCell.GetCellsVisible();
                    var valueConstraints = candidateValues.Select(v => new
                    {
                        Value = v,
                        ContraintNb = neighbourood.Count(neighboor => neighboor.Candidates.Contains(v))
                    }).ToList();
                    var minContraints = valueConstraints.Min(vc => vc.ContraintNb);
                    var exploredValue = valueConstraints.First(vc => vc.ContraintNb == minContraints).Value;
                    currentlyExploredCellValues.ExploredValues.Add(exploredValue);
                    targetCell.Set(exploredValue);
                }


            } while (!solved);



            foreach (var i in System.Linq.Enumerable.Range(0, 9))
            {
                foreach (var j in System.Linq.Enumerable.Range(0, 9))
                {
                    s.Cells[i][j] = monPuzzle.Rows[i][j].Value;
                }
            }
			return s;
        }

    }






internal sealed partial class Solver
{
    public Puzzle Puzzle { get; }

    public Solver(Puzzle puzzle)
    {
        Puzzle = puzzle;
    }

    public void DoWork(object? sender, DoWorkEventArgs e)
    {
        Puzzle.RefreshCandidates();
        Puzzle.LogAction("Begin");

        bool solved; // If this is true after a segment, the puzzle is solved and we can break
        do
        {
            solved = true;
            bool changed = CheckForNakedSinglesOrCompletion(ref solved);

            // Solved or failed to solve
            if (solved
                || (!changed && !RunTechnique()))
            {
                break;
            }
        } while (true);

        e.Result = solved;
    }


    private bool CheckForNakedSinglesOrCompletion(ref bool solved)
    {
        bool changed = false;
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                Cell cell = Puzzle[x, y];
                if (cell.Value == Cell.EMPTY_VALUE)
                {
                    solved = false;
                    // Check for naked singles
                    HashSet<int> a = cell.Candidates;
                    if (a.Count == 1)
                    {
                        int nakedSingle = a.ElementAt(0);
                        cell.Set(nakedSingle);
                        Puzzle.LogAction(Puzzle.TechniqueFormat("Naked single", "{0}: {1}", cell, nakedSingle), cell, (Cell?)null);
                        changed = true;
                    }
                }
            }
        }
        return changed;
    }

    public bool RunTechnique()
    {
        foreach (SolverTechnique t in _techniques)
        {
            if (t.Function.Invoke(Puzzle))
            {
                return true;
            }
        }
        return false;
    }
}
}
