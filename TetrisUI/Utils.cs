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

        public static void PrettyPrint(int[][] array)
        {
            if (array == null || array.Length == 0)
            {
                Console.WriteLine("Array is empty or null.");
                return;
            }

            // Find the maximum number of digits in the array for formatting
            int maxDigits = 0;
            foreach (var row in array)
            {
                foreach (var item in row)
                {
                    int digits = item.ToString().Length;
                    if (digits > maxDigits)
                    {
                        maxDigits = digits;
                    }
                }
            }

            // Print each row
            foreach (var row in array)
            {
                foreach (var item in row)
                {
                    Console.Write(item.ToString().PadLeft(maxDigits + 1));
                }
                Console.WriteLine();
            }
        }

    }
}