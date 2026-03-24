using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace NcCadViewer.parser
{

    // ======================
    // ARC BUILDER
    // ======================

    public static class SinumerikArcBuilder
    {
        public static MotionSegment BuildArc(
            ParserState state,
            (double? X, double? Y, double? Z) xyz,
            ArcData arc,
            MotionKind kind)
        {

            //MessageBox.Show($"ARC START = {state.X}, {state.Y}");
            var start = new Point3D(state.X, state.Y, state.Z);

            double x = state.Absolute ? (xyz.X ?? state.X) : state.X + (xyz.X ?? 0);
            double y = state.Absolute ? (xyz.Y ?? state.Y) : state.Y + (xyz.Y ?? 0);
            double z = state.Absolute ? (xyz.Z ?? state.Z) : state.Z + (xyz.Z ?? 0);

            var end = new Point3D(x, y, z);

            state.X = x;
            state.Y = y;
            state.Z = z;

            return new MotionSegment
            {
                Kind = kind,
                Start = start,
                End = end,
                Arc = new ArcData(arc.Center, arc.Clockwise, arc.Plane)
            };
        }
    }
}
