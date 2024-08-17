using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;

namespace Windows_Disk_Analyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Func<double, string> ToHumaanbleSizes = (x) => Analyzer.BytesToString((long)x);

        public MainWindow()
        {
            InitializeComponent();
            Chart.AxisY.Add(new Axis { LabelFormatter = ToHumaanbleSizes });
            SeriesCollection = new SeriesCollection();
            ThreadPool.SetMaxThreads(6, 4);

            //adding values or series will update and animate the chart automatically
            //SeriesCollection.Add(new PieSeries());
            //SeriesCollection[0].Values.Add(5);
            Analyzer disk_analized = new Analyzer("C:/");
            var elements_list = disk_analized.Get_elements_in_dir();
            elements_list.Sort((x, y) => y.size.CompareTo(x.size));
            long system_files_Size = 0;

            for (int i = 0; i < 8 && i < elements_list.Count; i++)
            {
                if (elements_list[i].Attributes.ToString().IndexOf(FileAttributes.System.ToString()) > -1)
                {
                    system_files_Size += elements_list[i].size;
                    continue;
                }

                SeriesCollection.Add(new PieSeries
                {
                    Title = elements_list[i].Name,
                    Stroke = Brushes.Transparent,
                    Values = new ChartValues<ObservableValue> { new ObservableValue(elements_list[i].size) },
                    DataLabels = true
                });
            }

            SeriesCollection.Add(new PieSeries
            {
                Title = "System Files",
                Values = new ChartValues<ObservableValue> { new ObservableValue(system_files_Size) },
                DataLabels = true
            });

            Chart.HideLegend();
            Chart.ToolTip = null;
            Chart.Hoverable = false;

            Size_label.Text = "Size: " + disk_analized.get_Formated_size();
            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }

        private void UpdateAllOnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();

            foreach (var series in SeriesCollection)
            {
                foreach (var observable in series.Values.Cast<ObservableValue>())
                {
                    observable.Value = r.Next(0, 10);
                }
            }
        }

        private void AddSeriesOnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            var c = SeriesCollection.Count > 0 ? SeriesCollection[0].Values.Count : 5;

            var vals = new ChartValues<ObservableValue>();

            for (var i = 0; i < c; i++)
            {
                vals.Add(new ObservableValue(r.Next(0, 10)));
            }

            SeriesCollection.Add(new PieSeries
            {
                Values = vals
            });
        }

        private void RemoveSeriesOnClick(object sender, RoutedEventArgs e)
        {
            if (SeriesCollection.Count > 0)
                SeriesCollection.RemoveAt(0);
        }

        private void AddValueOnClick(object sender, RoutedEventArgs e)
        {
            var r = new Random();
            foreach (var series in SeriesCollection)
            {
                series.Values.Add(new ObservableValue(r.Next(0, 10)));
            }
        }

        private void RemoveValueOnClick(object sender, RoutedEventArgs e)
        {
            foreach (var series in SeriesCollection)
            {
                if (series.Values.Count > 0)
                    series.Values.RemoveAt(0);
            }
        }

        private void RestartOnClick(object sender, RoutedEventArgs e)
        {
            Chart.Update(true, true);
        }
    }
}