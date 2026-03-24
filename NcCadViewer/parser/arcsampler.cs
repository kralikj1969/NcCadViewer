using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace NcCadViewer.parser
{
    public static class ArcSampler_Sinumerik
    {

        public static IEnumerable<Point3D> SampleArc(
     Point3D start,
     Point3D end,
     Point3D center,
     bool clockwise,
     double chordTol = 1.0)
        {
            double cx = center.X;
            double cy = center.Y;

            double xs = start.X - cx;
            double ys = start.Y - cy;
            double xe = end.X - cx;
            double ye = end.Y - cy;

            double r = Math.Sqrt(xs * xs + ys * ys);

            // OBRATIT Y (protože tvoje +Y je dolů)
            double ang0 = Math.Atan2(-(start.Y - center.Y), start.X - center.X);
            double ang1 = Math.Atan2(-(end.Y - center.Y), end.X - center.X);

            // rozdíl
            double d = ang1 - ang0;

            // normalizace
            while (d > Math.PI) d -= 2 * Math.PI;
            while (d < -Math.PI) d += 2 * Math.PI;

            int steps = Math.Max(40, (int)(Math.Abs(d) * r));

            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;


                double a;

                if (clockwise)          // G2 – červený (správně)
                {
                    a = ang1 + d * t;
                }
                else                    // G3 – zelený (otočit!)
                {
                    a = ang0 - d * t;
                }


                yield return new Point3D(
                    cx + r * Math.Cos(a),
                    cy + r * Math.Sin(a),
                    start.Z + (end.Z - start.Z) * t
                );
            }
        }




    }
}