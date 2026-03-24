using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NcCadViewer.parser
{
    // ======================
    // TOKENIZER
    // ======================




    public static class SinumerikTokenizer
    {
        public static string StripComments(string line)
        {
            int idx = line.IndexOf(';');
            if (idx >= 0)
                line = line.Substring(0, idx);
            return line.Trim();
        }

        public static IEnumerable<(char Letter, string Value)> Tokenize(string line)
        {
            line = line.Trim();
            int i = 0;

            while (i < line.Length)
            {
                char c = char.ToUpperInvariant(line[i]);

                // Neznak? Pokračuj
                if (!char.IsLetter(c))
                {
                    i++;
                    continue;
                }

                // Písmeno X,Y,Z,I,J,G,F,S,R,N
                int p = i + 1;

                // případ X=40
                if (p < line.Length && line[p] == '=')
                    p++;

                // teď čteme ČÍSLO
                int q = p;

                while (q < line.Length &&
                      (char.IsDigit(line[q]) ||
                       line[q] == '.' ||
                       line[q] == '-' ||
                       line[q] == '+'))
                {
                    q++;
                }

                // hodnota
                string raw = line.Substring(p, q - p);

                // Pokud nic – přeskočit
                if (string.IsNullOrWhiteSpace(raw))
                {
                    i = q;
                    continue;
                }

                yield return (c, raw);
                i = q;
            }
        }
    }

}
