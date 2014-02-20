using System.Linq;

namespace Trainer.net
{
    public static class Extensions
    {
        private static readonly char[] HexLetters = {'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F'};

        public static bool IsHexLetter(this char c)
        {
            if (HexLetters.Contains(c))
                return true;
            return false;
        }
    }
}