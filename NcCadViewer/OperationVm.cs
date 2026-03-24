using System.ComponentModel;
using System.Windows.Media.Media3D;

namespace NcCadViewer
{
    public class OperationVm : INotifyPropertyChanged
    {
        private bool _isVisible = true;

        public string Name { get; set; }

        public ModelVisual3D Visual { get; set; }

        // odkaz na rodiče, do kterého se Visual vkládá
        public ModelVisual3D ParentRoot { get; set; }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    Notify(nameof(IsVisible));

                    if (Visual != null && ParentRoot != null)
                    {
                        if (_isVisible)
                        {
                            if (!ParentRoot.Children.Contains(Visual))
                                ParentRoot.Children.Add(Visual);
                        }
                        else
                        {
                            if (ParentRoot.Children.Contains(Visual))
                                ParentRoot.Children.Remove(Visual);
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Notify(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}