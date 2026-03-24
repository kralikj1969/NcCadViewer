using NcCadViewer.parser;
using HelixToolkit.Wpf;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace NcCadViewer
{
    public partial class MainWindow
    {
        private void OpenNc_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "NC code|*.mpf;*.iso;*.nc;*.tap;*.txt",
                Multiselect = true
            };
            if (dlg.ShowDialog() != true)
                return;

            // smazat staré NC
            foreach (var op in ncOperations)
                NcRoot.Children.Remove(op.Visual);
            ncOperations.Clear();

            // obnovit STL položky
            Operations.Clear();
            foreach (var stl in stlModels)
                Operations.Add(stl);

            foreach (var file in dlg.FileNames)
            {
                var lines = File.ReadAllLines(file);
                var parser = new GCodeParser_Sinumerik();
                var segments = parser.Parse(lines);

                Segments = segments;
                StepIndex = 0;

                var op = new OperationVm
                {
                    Name = System.IO.Path.GetFileName(file),
                    IsVisible = true,
                    Visual = new ModelVisual3D(),
                    ParentRoot = NcRoot
                };

                NcRoot.Children.Add(op.Visual);
                Operations.Add(op);

                foreach (var seg in segments)
                {
                    Color col = seg.Kind switch
                    {
                        MotionKind.Rapid => Colors.Red,
                        MotionKind.Linear => Colors.DodgerBlue,
                        MotionKind.ArcCW => Colors.DodgerBlue,
                        MotionKind.ArcCCW => Colors.DodgerBlue,
                        _ => Colors.Gray
                    };

                    Point3DCollection pts;
                    if (seg.Arc != null)
                    {
                        pts = new Point3DCollection();
                        foreach (var p in ArcSampler_Sinumerik.SampleArc(
                                 seg.Start, seg.End, seg.Arc.Center, seg.Arc.Clockwise, 1.0))
                            pts.Add(p);
                    }
                    else
                    {
                        pts = new Point3DCollection { seg.Start, seg.End };
                    }

                    op.Visual.Children.Add(new LinesVisual3D
                    {
                        Color = col,
                        Thickness = 2,
                        Points = pts
                    });
                }
            }

            View.ZoomExtents();
        }
    }
}