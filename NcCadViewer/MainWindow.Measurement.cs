using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Windows.Controls;
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
        // ==  SEKCE 2 — MĚŘENÍ  ================================================================
        // ======================================================================================


        // RESET MĚŘENÍ =========================================================================
        private void ClearMeasurement_Click(object sender, RoutedEventArgs e)
        {
            foreach (var v in _measureVisuals)
                NcRoot.Children.Remove(v);

            _measureVisuals.Clear();
            _measurePoints.Clear();
        }



        // POSUN TEXTU KE KAMEŘE =================================================================
        private Point3D OffsetTowardCamera(Point3D original, double distance = 5.0)
        {
            if (View?.Camera == null)
                return original;

            Vector3D camDir = View.Camera.LookDirection;
            camDir.Normalize();

            return original - camDir * distance;
        }



        // HLAVNÍ HIT-TEST PRO MĚŘENÍ (3.1.2 KOMPATIBILNÍ) ======================================
        private void View_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (measurementMode == 0)
                return; // měření vypnuto

            var pos = e.GetPosition(View);
            var hits = View.Viewport.FindHits(pos);
            var hit = hits.FirstOrDefault() as PointHitResult;

            if (hit == null)
                return;

            // pozice na povrchu modelu
            Point3D p = hit.Position;

            // SNAP NA HRANU — správné API 3.1.2
            if (hit.Model is GeometryModel3D gm &&
                gm.Geometry is MeshGeometry3D mesh)
            {
                p = SnapToNearestEdge(p, mesh, 2.0);
            }

            // spustit vybraný režim
            switch (measurementMode)
            {
                case 1:  // 2 body
                    HandleTwoPointMeasurement(p);
                    break;

                case 2:  // kružnice 3 body
                    HandleThreePointCircleMeasurement(p);
                    break;

                case 6:  // edge to edge
                    HandleEdgeToEdgeMeasurement(p, hit);
                    break;
            }
        }



        // MĚŘENÍ 2 BODŮ =========================================================================
        private void HandleTwoPointMeasurement(Point3D p)
        {
            var sphere = new SphereVisual3D()
            {
                Center = p,
                Radius = 1.5,
                Fill = Brushes.Yellow
            };
            NcRoot.Children.Add(sphere);
            _measureVisuals.Add(sphere);

            _measurePoints.Add(p);

            if (_measurePoints.Count == 2)
            {
                var p1 = _measurePoints[0];
                var p2 = _measurePoints[1];

                var line = new LinesVisual3D()
                {
                    Color = Colors.Yellow,
                    Thickness = 2,
                    Points = new Point3DCollection { p1, p2 }
                };
                NcRoot.Children.Add(line);
                _measureVisuals.Add(line);

                double len = (p2 - p1).Length;

                var mid = new Point3D(
                    (p1.X + p2.X) / 2,
                    (p1.Y + p2.Y) / 2,
                    (p1.Z + p2.Z) / 2);

                var text = new BillboardTextVisual3D()
                {
                    Text = $"{len:F2} mm",
                    Position = OffsetTowardCamera(mid, 5),
                    Foreground = Brushes.Black,
                    Background = Brushes.White
                };

                NcRoot.Children.Add(text);
                _measureVisuals.Add(text);

                _measurePoints.Clear();
            }
        }



        // MĚŘENÍ KRUŽNICE ZE 3 BODŮ =============================================================
        private void HandleThreePointCircleMeasurement(Point3D p)
        {
            var sphere = new SphereVisual3D()
            {
                Center = p,
                Radius = 1.5,
                Fill = Brushes.Lime
            };
            NcRoot.Children.Add(sphere);
            _measureVisuals.Add(sphere);

            _measurePoints.Add(p);

            if (_measurePoints.Count == 3)
            {
                var p1 = _measurePoints[0];
                var p2 = _measurePoints[1];
                var p3 = _measurePoints[2];

                if (CircleFrom3Points(p1, p2, p3, out var center, out var radius))
                {
                    // střed
                    var cSphere = new SphereVisual3D
                    {
                        Center = center,
                        Radius = 2,
                        Fill = Brushes.Red
                    };
                    NcRoot.Children.Add(cSphere);
                    _measureVisuals.Add(cSphere);

                    // vykreslit kružnici
                    DrawCircle(center, p1, p2, p3, radius);

                    var text = new BillboardTextVisual3D()
                    {
                        Text = $"Ø {radius * 2:F2} mm",
                        Position = OffsetTowardCamera(center, 5),
                        Foreground = Brushes.Black,
                        Background = Brushes.White
                    };
                    NcRoot.Children.Add(text);
                    _measureVisuals.Add(text);
                }

                _measurePoints.Clear();
            }
        }



        // KRUŽNICE Z 3 BODŮ – STABILNÍ VERZE ====================================================
        private bool CircleFrom3Points(Point3D p1, Point3D p2, Point3D p3,
                                       out Point3D center3D, out double radius)
        {
            center3D = new Point3D();
            radius = 0;

            Vector3D v1 = p2 - p1;
            Vector3D v2 = p3 - p1;
            Vector3D normal = Vector3D.CrossProduct(v1, v2);

            if (normal.Length < 1e-6)
                return false;

            normal.Normalize();

            Vector3D xAxis = v1;
            xAxis.Normalize();

            Vector3D yAxis = Vector3D.CrossProduct(normal, xAxis);

            Point p1_2d = new Point(0, 0);

            Point p2_2d = new Point(
                Vector3D.DotProduct(v1, xAxis),
                Vector3D.DotProduct(v1, yAxis));

            Point p3_2d = new Point(
                Vector3D.DotProduct(v2, xAxis),
                Vector3D.DotProduct(v2, yAxis));

            double A = p2_2d.X - p1_2d.X;
            double B = p2_2d.Y - p1_2d.Y;
            double C = p3_2d.X - p1_2d.X;
            double D = p3_2d.Y - p1_2d.Y;

            double E = A * (p1_2d.X + p2_2d.X) + B * (p1_2d.Y + p2_2d.Y);
            double F = C * (p1_2d.X + p3_2d.X) + D * (p1_2d.Y + p3_2d.Y);

            double G = 2 * (A * (p3_2d.Y - p2_2d.Y) - B * (p3_2d.X - p2_2d.X));

            if (Math.Abs(G) < 1e-6)
                return false;

            double cx = (D * E - B * F) / G;
            double cy = (A * F - C * E) / G;

            center3D = p1 + xAxis * cx + yAxis * cy;
            radius = (center3D - p1).Length;

            return true;
        }



        // KRESLENÍ KRUŽNICE =====================================================================
        private void DrawCircle(Point3D center, Point3D p1, Point3D p2, Point3D p3, double radius)
        {
            Vector3D normal = Vector3D.CrossProduct(p2 - p1, p3 - p1);
            normal.Normalize();

            Vector3D xdir = p1 - center;
            xdir.Normalize();

            Vector3D ydir = Vector3D.CrossProduct(normal, xdir);

            var points = new Point3DCollection();
            int N = 72;

            for (int i = 0; i <= N; i++)
            {
                double ang = 2 * Math.PI * i / N;

                Point3D pt =
                    center +
                    xdir * Math.Cos(ang) * radius +
                    ydir * Math.Sin(ang) * radius;

                points.Add(pt);
            }

            var circle = new LinesVisual3D()
            {
                Color = Colors.Cyan,
                Thickness = 2,
                Points = points
            };

            NcRoot.Children.Add(circle);
            _measureVisuals.Add(circle);
        }



        // EDGE–EDGE MĚŘENÍ ======================================================================
        private void HandleEdgeToEdgeMeasurement(Point3D rawHit, PointHitResult hit)
        {
            if (hit.Model is GeometryModel3D gm &&
                gm.Geometry is MeshGeometry3D mesh)
            {
                Point3D edgePoint = SnapToNearestEdge(rawHit, mesh, 5.0);

                var sphere = new SphereVisual3D()
                {
                    Center = edgePoint,
                    Radius = 2,
                    Fill = Brushes.Lime
                };
                NcRoot.Children.Add(sphere);
                _measureVisuals.Add(sphere);

                _measurePoints.Add(edgePoint);

                if (_measurePoints.Count == 2)
                {
                    Point3D p1 = _measurePoints[0];
                    Point3D p2 = _measurePoints[1];

                    var line = new LinesVisual3D()
                    {
                        Color = Colors.Lime,
                        Thickness = 2,
                        Points = new Point3DCollection { p1, p2 }
                    };
                    NcRoot.Children.Add(line);
                    _measureVisuals.Add(line);

                    double len = (p2 - p1).Length;

                    var mid = new Point3D(
                        (p1.X + p2.X) / 2,
                        (p1.Y + p2.Y) / 2,
                        (p1.Z + p2.Z) / 2);

                    var text = new BillboardTextVisual3D()
                    {
                        Text = $"{len:F2} mm",
                        Position = OffsetTowardCamera(mid, 5),
                        Foreground = Brushes.Black,
                        Background = Brushes.White
                    };
                    NcRoot.Children.Add(text);
                    _measureVisuals.Add(text);

                    _measurePoints.Clear();
                }
            }
        }


    }
}
