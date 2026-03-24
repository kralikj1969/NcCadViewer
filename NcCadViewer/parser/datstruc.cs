using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace NcCadViewer.parser
{
    // ======================
    // DATOVÉ STRUKTURY
    // ======================

    public enum MotionKind
    {
        Rapid,
        Linear,
        ArcCW,
        ArcCCW
    }

    public enum Plane
    {
        G17_XY,
        G18_ZX,
        G19_YZ
    }

    public class MotionSegment
    {
        public MotionKind Kind;
        public Point3D Start;
        public Point3D End;
        public ArcData? Arc;
    }

    public class ArcData
    {
        public Point3D Center;
        public bool Clockwise;
        public Plane Plane;

        public ArcData(Point3D center, bool clockwise, Plane plane)
        {
            Center = center;
            Clockwise = clockwise;
            Plane = plane;
        }
    }

    public class ParserState
    {
        public double X = 0.0;
        public double Y = 0.0;
        public double Z = 0.0;

        public MotionKind CurrentMotion = MotionKind.Linear;

        public double Feed = 0.0;
        public double Speed = 0.0;

        public bool Absolute = true;
        public Plane Plane = Plane.G17_XY;

        public Dictionary<int, double> Registers = new();
    }


}
