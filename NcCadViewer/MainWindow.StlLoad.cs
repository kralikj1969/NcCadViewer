using HelixToolkit.Wpf;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Media3D;

namespace NcCadViewer
{
    public partial class MainWindow
    {
        private void LoadStl_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "STL (*.stl)|*.stl"
            };
            if (dlg.ShowDialog() != true)
                return;

            var reader = new StLReader();
            var model = reader.Read(dlg.FileName);

            var visual = new ModelVisual3D { Content = model };

            var op = new OperationVm
            {
                Name = System.IO.Path.GetFileName(dlg.FileName),
                IsVisible = true,
                Visual = visual,
                ParentRoot = NcRoot
            };

            NcRoot.Children.Add(op.Visual);
            Operations.Add(op);
            stlModels.Add(op);
        }
    }
}