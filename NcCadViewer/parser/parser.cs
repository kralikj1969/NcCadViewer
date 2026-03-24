using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NcCadViewer.parser
{
    // ======================
    // PARSER ENTRY POINT
    // ======================

    public class GCodeParser_Sinumerik
    {
        public List<MotionSegment> Parse(string[] lines)
        {
            var list = new List<MotionSegment>();
            var state = new ParserState();

            foreach (var rawLine in lines)
            {
                string line = SinumerikTokenizer.StripComments(rawLine);
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var tokens = SinumerikTokenizer.Tokenize(line).ToList();
                if (tokens.Count == 0)
                    continue;

                var kind = SinumerikInterpreter.InterpretTokens(
                    tokens, state, out var xyz, out ArcData? arcData);

                if (kind == null)
                    continue;

                // 🔥 DEBUG VÝPIS — přesně tady

                // MessageBox.Show($"{kind}: START=({state.X},{state.Y},{state.Z}) END=({xyz.X},{xyz.Y},{xyz.Z}) ARC={arcData != null}");


                // Lineární G0 / G1
                if (kind == MotionKind.Rapid || kind == MotionKind.Linear)
                {
                    // OPRAVA PRVNÍ LINIE PŘED BuildLinear
                    if (list.Count == 0 &&
                        state.X == 0.0 && state.Y == 0.0 && state.Z == 0.0)
                    {
                        // první pohyb → přeskočit vytváření segmentu
                        state.X = xyz.X ?? state.X;
                        state.Y = xyz.Y ?? state.Y;
                        state.Z = xyz.Z ?? state.Z;
                        continue;
                    }

                    var seg = SinumerikLinearBuilder.BuildLinear(state, xyz, kind.Value);
                    list.Add(seg);
                    continue;
                }


                // Oblouky G2 / G3
                if (kind == MotionKind.ArcCW || kind == MotionKind.ArcCCW)
                {
                    if (arcData != null)
                        list.Add(SinumerikArcBuilder.BuildArc(state, xyz, arcData, kind.Value));
                    continue;
                }
            }

            return list;
        }
    }

}
