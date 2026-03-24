using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace NcCadViewer.parser
{

    // ======================
    // INTERPRETER
    // ======================

    public static class SinumerikInterpreter
    {
        public static MotionKind? InterpretTokens(
            IEnumerable<(char Letter, string Value)> tokens,
            ParserState state,
            out (double? X, double? Y, double? Z) xyz,
            out ArcData? arcData)
        {
            xyz = (null, null, null);
            arcData = null;

            MotionKind? kind = null;
            double? arcCx = null;
            double? arcCy = null;

            foreach (var t in tokens)
            {
                char L = t.Letter;
                string raw = t.Value;

                switch (L)
                {
                    case 'G':
                        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double gv))
                        {
                            switch ((int)gv)
                            {
                                case 0: kind = MotionKind.Rapid; break;
                                case 1: kind = MotionKind.Linear; break;
                                case 2: kind = MotionKind.ArcCW; break;
                                case 3: kind = MotionKind.ArcCCW; break;
                                case 17: state.Plane = Plane.G17_XY; break;
                                case 18: state.Plane = Plane.G18_ZX; break;
                                case 19: state.Plane = Plane.G19_YZ; break;
                                case 90: state.Absolute = true; break;
                                case 91: state.Absolute = false; break;
                            }
                        }
                        break;

                    case 'X': xyz.X = ParseDouble(raw); break;
                    case 'Y': xyz.Y = ParseDouble(raw); break;
                    case 'Z': xyz.Z = ParseDouble(raw); break;

                    case 'I': arcCx = ParseACValue(raw); break;

                    case 'J':
                        arcCy = double.Parse(raw, CultureInfo.InvariantCulture);
                        break;


                    case 'F': state.Feed = ParseRegisterOrNumber(raw, state); break;
                    case 'S': state.Speed = ParseRegisterOrNumber(raw, state); break;
                }
            }

            if (kind == MotionKind.ArcCW || kind == MotionKind.ArcCCW)
            {
                //if (arcCx.HasValue && arcCy.HasValue)
                //{
                //  var center = new Point3D(arcCx.Value, arcCy.Value, 0);
                // bool cw = (kind == MotionKind.ArcCW);

                // arcData = new ArcData(center, cw, state.Plane);
                //}

                if (arcCx.HasValue && arcCy.HasValue)
                {
                    // FANUC/ISO styl = I, J jsou relativní vůči STARTU
                    var center = new Point3D(
                        state.X + arcCx.Value,   // offset X
                        state.Y + arcCy.Value,   // offset Y
                        state.Z                 // Z beze změny
                    );


                    // Pozor: Sinumerik G3 má opačný směr proti klasickému Atan2
                    bool clockwise = (kind == MotionKind.ArcCW);
                    bool CounterClockwise = (kind == MotionKind.ArcCCW);
                    // clockwise = !clockwise;   // 🔥 invertujeme směr
                    // bool clockwise = (kind == MotionKind.ArcCW);   // G2 = CW, G3 = CCW
                    arcData = new ArcData(center, clockwise, state.Plane);

                }

            }

            if (kind == null)
            {
                kind = state.CurrentMotion;
            }
            else
            {
                state.CurrentMotion = kind.Value;
            }


            // 🔥 DOPLNĚNÍ CHYBĚJÍCÍCH SOUŘADNIC
            if (!xyz.X.HasValue) xyz.X = state.X;
            if (!xyz.Y.HasValue) xyz.Y = state.Y;
            if (!xyz.Z.HasValue) xyz.Z = state.Z;


            // 🔥 DOPLNĚNÍ CHYBĚJÍCÍCH SOUŘADNIC
            if (!xyz.X.HasValue) xyz.X = state.X;
            if (!xyz.Y.HasValue) xyz.Y = state.Y;
            if (!xyz.Z.HasValue) xyz.Z = state.Z;



            return kind;
        }

        private static double ParseDouble(string raw)
            => double.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture);

        private static double ParseACValue(string raw)
        {
            string tmp = raw.Trim();
            if (tmp.StartsWith("AC(", StringComparison.InvariantCultureIgnoreCase))
            {
                tmp = tmp.Substring(3);
                if (tmp.EndsWith(")")) tmp = tmp.Substring(0, tmp.Length - 1);
            }
            return double.Parse(tmp, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static double ParseRegisterOrNumber(string raw, ParserState state)
        {
            if (raw.StartsWith("R", StringComparison.InvariantCultureIgnoreCase))
            {
                if (int.TryParse(raw.Substring(1), out int rIndex))
                {
                    if (state.Registers.TryGetValue(rIndex, out double v))
                        return v;
                }
                return 0.0;
            }

            return ParseDouble(raw);
        }
    }




}
