using System.Collections.Generic;
using System.Linq;
using RT.Util;

namespace KyudosudokuWebsite
{
    sealed class Example
    {
        public List<KyuConstraint> Constraints = new List<KyuConstraint>();
        public List<int> Cells = new List<int>();
        public List<int> Good = new List<int>();
        public List<int> Bad = new List<int>();
        public bool Wide = false;
        public string Reason;

        public Dictionary<int, int?> GoodGivens => Enumerable.Range(0, Cells.Count).ToDictionary(ix => Cells[ix], ix => Good[ix].Nullable());
        public Dictionary<int, int?> BadGivens => Enumerable.Range(0, Cells.Count).ToDictionary(ix => Cells[ix], ix => Bad[ix].Nullable());
    }
}
