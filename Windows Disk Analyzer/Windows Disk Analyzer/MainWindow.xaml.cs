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
        DriveInfo[] root_drives;
        Analyzer Current_analyze;



        class file_list_presentor
        {
            public file_list_presentor(string name, long size , Files_presentor files_Presentor)
            {
                this.file_name = name;
                this.file_size = Analyzer.BytesToString(size);
                this.files_Presentor = files_Presentor;
            }
            public string file_name { get; private set; }
            public string file_size { get; private set; }
            public Files_presentor files_Presentor { get; private set; }
        }

        List<file_list_presentor> file_presentors = new List<file_list_presentor>();

        public MainWindow()
        {
            
            InitializeComponent();
            File_Chart.AxisY.Add(new Axis { LabelFormatter = ToHumaanbleSizes });
            SeriesCollection = new SeriesCollection();
            ThreadPool.SetMaxThreads(6, 4);
            root_drives = DriveInfo.GetDrives();
            //adding values or series will update and animate the chart automatically
            //SeriesCollection.Add(new PieSeries());
            //SeriesCollection[0].Values.Add(5);
            Disk_ComboBox.Items.Clear();
            foreach (var diskdrive in root_drives)
            {
                Disk_ComboBox.Items.Add(diskdrive);
            }
            Disk_ComboBox.SelectedIndex = 0;

        }

        public SeriesCollection SeriesCollection { get; set; }
        
        void LoadDrive(string dir_path)
        {
            Current_analyze = new Analyzer(dir_path);
            SeriesCollection.Clear();


            var elements_list = Current_analyze.Get_elements_in_dir();
            elements_list.Sort((x, y) => y.size.CompareTo(x.size));
            long system_files_Size = 0;
            long Other_files = 0;

            for (int i = 0; i < 8 && i < elements_list.Count; i++)
            {
                SeriesCollection.Add(new PieSeries
                {
                    Title = elements_list[i].Name,
                    Stroke = Brushes.Transparent,
                    Values = new ChartValues<ObservableValue> { new ObservableValue(elements_list[i].size) },
                    DataLabels = true
                });
            }

            for (int i = 8; i < elements_list.Count; i++)
            {
                if (elements_list[i].Attributes.ToString().IndexOf(FileAttributes.System.ToString()) > -1)
                {
                    system_files_Size += elements_list[i].size;
                }
                else
                {
                    Other_files += elements_list[i].size;
                }
            }

            file_presentors.Clear();
            for (int i = 0; i < elements_list.Count; i++)
            {
                file_presentors.Add(new file_list_presentor(elements_list[i].Name, elements_list[i].size, elements_list[i]));
            }
            FileList.ItemsSource = file_presentors;
            FileList.Items.Refresh();

            SeriesCollection.Add(new PieSeries
            {
                Title = "System Files",
                Values = new ChartValues<ObservableValue> { new ObservableValue(system_files_Size) },
                DataLabels = true
            });
            SeriesCollection.Add(new PieSeries
            {
                Title = "Other",
                Values = new ChartValues<ObservableValue> { new ObservableValue(Other_files) },
                DataLabels = true
            });


            File_Chart.HideLegend();
            File_Chart.ToolTip = null;
            File_Chart.Hoverable = false;

            Size_label.Text = "Size: " + Current_analyze.get_Formated_size();
            DataContext = this;
        }

        private void FileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FileList.SelectedItem == null)
            {
                return;
            }

            file_list_presentor selected_item = (FileList.SelectedItem as file_list_presentor);
            if (selected_item.files_Presentor.dir_info == null)
                return;

            LoadDrive(selected_item.files_Presentor.dir_info.FullName);
        }

        private void AnalyzeButton(object sender, RoutedEventArgs e)
        {
            LoadDrive((Disk_ComboBox.SelectedItem as DriveInfo).Name);
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            LoadDrive(Current_analyze.GetParent());
        }
    }
}