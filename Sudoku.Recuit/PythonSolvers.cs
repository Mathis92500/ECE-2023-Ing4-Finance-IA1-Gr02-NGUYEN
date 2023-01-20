using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;
using Sudoku.Shared;

namespace Sudoku.Recuit
{




    public class RecuitPythonDotNetSolver : PythonSolverBase
    {

        public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
        {
            //System.Diagnostics.Debugger.Break();

            //For some reason, the Benchmark runner won't manage to get the mutex whereas individual execution doesn't cause issues
            //using (Py.GIL())
            //{
            // create a Python scope
            using (PyModule scope = Py.CreateScope())
            {
                // convert the Person object to a PyObject
                PyObject pySudoku = s.ToPython();

                // create a Python variable "person"
                scope.Set("sudoku", pySudoku);

                // the person object may now be used in Python
                string code = Resources.SelfCallSolver_py;
                scope.Exec(code);
                var result = scope.Get("solvedSudoku");
                var toReturn = result.As<Shared.SudokuGrid>();
                return toReturn;
            }
            //}

        }

    }

    public class RecuitPythonNativeSolver : PythonSolverBase
    {


        public override Shared.SudokuGrid Solve(Shared.SudokuGrid s)
        {

            //using (Py.GIL())
            //{
            // create a Python scope
            using (PyModule scope = Py.CreateScope())
            {
                // convert the Person object to a PyObject
                PyObject pyCells = s.Cells.ToPython();

                // create a Python variable "person"
                scope.Set("instance", pyCells);


				string numpyConverterCode = Resources.numpy_converter_py;
				scope.Exec(numpyConverterCode);

				string recuitSolverCode = Resources.RecuitSolver_py;
                scope.Exec(recuitSolverCode);
                var result = scope.Get("r");
                var managedResult = result.As<int[,]>().ToJaggedArray();
                //var convertedResult = managedResult.Select(objList => objList.Select(o => o.As<int>()).ToArray()).ToArray();
                return new Shared.SudokuGrid() { Cells = managedResult };
            }
            //}

        }

        protected override void InitializePythonComponents()
        {
			InstallPipModule("numpy");
			//InstallPipModule("simanneal");
			base.InitializePythonComponents();
        }



    }


}