using System.Collections.Generic;

namespace GameCore
{
    public class CharAndLengthComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x[0] == y[0])
            {
                return x.Length.CompareTo(y.Length);
            }
            
            return x[0].CompareTo(y[0]);
        }
    }
}
