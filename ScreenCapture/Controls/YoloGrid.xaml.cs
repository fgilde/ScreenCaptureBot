using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Alturos.Yolo.Model;
using WPFCaptureSample.Annotations;

namespace WPFCaptureSample.Controls
{
    /// <summary>
    /// Interaction logic for YoloGrid.xaml
    /// </summary>
    public partial class YoloGrid : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<YoloItem> items;

        public ObservableCollection<YoloItem> Items
        {
            get => items;
            set
            {
                items = value;
                OnPropertyChanged();
            }
        }

        public YoloGrid()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
