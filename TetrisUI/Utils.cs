using Microsoft.FSharp.Collections;
using System.Linq;

namespace TetrisUI
{
    class Utils
    {
        public static FSharpList<FSharpList<int>> ToFSharpList(int[][] array)
        {
            return ListModule.OfSeq(array.Select(row => ListModule.OfSeq(row)));
        }
    }
}