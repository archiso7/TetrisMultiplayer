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

        public static int[][] FSharpListToArray(FSharpList<FSharpList<int>> fsharpList)
        {
            return fsharpList.Select(row => ListModule.ToArray(row)).ToArray();
        }
    }
}