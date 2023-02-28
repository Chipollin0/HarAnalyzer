using HarAnalyzer.Model;
using HarAnalyzer.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HarAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                var dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == true)
                {
                    var filePath = dialog.FileName;
                    viewModel.FileName = filePath;
                }
            }
        }

        private void Analyze_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
                viewModel.LoadHarFile();
        }
    }

    public class HarEntryToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HarEntry)
            {
                var harEntry = (HarEntry)value;
                var text = harEntry.Request?.Method + " " + harEntry.Request?.Url;
                return text;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HarEntryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var errorColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#fffc9c9c") ?? Brushes.LightPink);
            var nothingColor = Brushes.White;
            var harEntry = value as HarEntry;
            if (harEntry == null)
                return nothingColor;
            if (harEntry.Response == null)
                return errorColor;
            if (harEntry.Response.Status == null)
                return errorColor;
            var status = harEntry.Response.Status;
            var error = status >= 400 || status <= 0;
            if (error)
                return errorColor;
            return nothingColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
