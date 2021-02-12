using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Mvvm;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace UWPSample.ViewModels
{
    public class MainViewModel : ViewModel
    {
        private ObservableCollection<FrameworkElement> _elements;

        public ObservableCollection<FrameworkElement> Elements
        {
            get { return _elements; }
            set { _elements = value; NotifyPropertyChanged(nameof(Elements)); }
        }

        public MainViewModel()
        {
            var items = new ObservableCollection<FrameworkElement>();

            items.Add(new Border()
            {
                Width = 200,
                Height = 400,
                Background = new SolidColorBrush(Colors.Green),
            });

            items.Add(new Border()
            {
                Width = 400,
                Height = 200,
                Background = new SolidColorBrush(Colors.Red),
            });

            items.Add(new Border()
            {
                Width = 200,
                Height = 200,
                Background = new SolidColorBrush(Colors.Gray),
            });


            items.Add(new Border()
            {
                Width = 200,
                Height = 200,
                Background = new SolidColorBrush(Colors.Gray),
            });

            items.Add(new Border()
            {
                Width = 400,
                Height = 400,
                Background = new SolidColorBrush(Colors.Orange),
            });

            Elements = items;
        }
    }
}
