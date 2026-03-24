
using NcCadViewer.parser;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace NcCadViewer
{
    public partial class MainWindow : Window
    {
        private readonly List<OperationVm> ncOperations = new();
        private readonly List<OperationVm> stlModels = new();

        private int measurementMode = 0;
        private List<MotionSegment> Segments = new();
        private int StepIndex = 0;

        private LinesVisual3D FullPath = null!;
        private LinesVisual3D ActiveSegment = null!;
        private SphereVisual3D ToolMarker = null!;
        private System.Windows.Threading.DispatcherTimer SimTimer = null!;

        public ObservableCollection<OperationVm> Operations { get; } = new();

        // measurement
        private readonly List<Point3D> _measurePoints = new();
        private readonly List<ModelVisual3D> _measureVisuals = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            View.Viewport.MouseDown += View_MouseDown;
            this.KeyDown += MainWindow_KeyDown;

            this.Loaded += (s, e) =>
            {
                SyncAxis();
                ChkAxis.Checked += ChkAxis_CheckedChanged;
                ChkAxis.Unchecked += ChkAxis_CheckedChanged;
            };
        }

        private void ChkAxis_CheckedChanged(object? sender, RoutedEventArgs e) => SyncAxis();

        private void SyncAxis()
        {
            bool show = ChkAxis.IsChecked == true;

            if (show && !View.Children.Contains(Axis))
                View.Children.Add(Axis);

            if (!show && View.Children.Contains(Axis))
                View.Children.Remove(Axis);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Segments == null) return;

            if (e.Key == Key.Right)
            {
                StepIndex++;
                if (StepIndex >= Segments.Count) StepIndex = Segments.Count - 1;
                UpdateStepVisual();
            }
            else if (e.Key == Key.Left)
            {
                StepIndex--;
                if (StepIndex < 0) StepIndex = 0;
                UpdateStepVisual();
            }
            else if (e.Key == Key.Space)
            {
                if (SimTimer.IsEnabled) SimTimer.Stop();
                else SimTimer.Start();
            }
        }

        private void UpdateStepVisual()
        {
            if (Segments == null || StepIndex < 0 || StepIndex >= Segments.Count)
                return;

            var s = Segments[StepIndex];
            ActiveSegment.Points = new Point3DCollection { s.Start, s.End };
        }
    }
}
