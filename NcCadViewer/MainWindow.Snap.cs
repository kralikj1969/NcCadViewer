using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using NcCadViewer.parser;
using System.Collections.Generic;
using System.Linq;



namespace NcCadViewer
{
    public partial class MainWindow
    {

        // ======================================================================================
        // ==  SEKCE 3 — SNAPOVÁNÍ NA HRANU A DALŠÍ NASTAVENÍ ===================================
        // ======================================================================================


        // PŘEPÍNÁNÍ REŽIMŮ MĚŘENÍ ===============================================================
        private void MeasurementModeChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                if (int.TryParse(rb.Tag.ToString(), out int mode))
                    measurementMode = mode;

                // smazat předchozí měření
                _measurePoints.Clear();
                ClearMeasurement_Click(null!, null!);
            }
        }



        // SNAP NA NEJBLIŽŠÍ HRANU (kompatibilní s HelixToolkit.Wpf 3.1.2) =======================
        private Point3D SnapToNearestEdge(
            Point3D hitPos,
            MeshGeometry3D mesh,
            double maxDistance = 2.0)
        {
            Point3D bestPoint = hitPos;
            double bestDist = double.MaxValue;

            var positions = mesh.Positions;
            var tris = mesh.TriangleIndices;

            for (int i = 0; i < tris.Count; i += 3)
            {
                Point3D a = positions[tris[i]];
                Point3D b = positions[tris[i + 1]];
                Point3D c = positions[tris[i + 2]];

                CheckEdge(a, b);
                CheckEdge(b, c);
                CheckEdge(c, a);
            }

            void CheckEdge(Point3D p1, Point3D p2)
            {
                Vector3D v = p2 - p1;
                double t = Vector3D.DotProduct(hitPos - p1, v) / v.LengthSquared;

                t = Math.Max(0, Math.Min(1, t)); // clamp

                Point3D proj = p1 + v * t;
                double d = (proj - hitPos).Length;

                if (d < bestDist && d < maxDistance)
                {
                    bestDist = d;
                    bestPoint = proj;
                }
            }

            return bestPoint;
        }



        // KONEC TŘÍDY MAINWINDOW ===============================================================

    }
}
