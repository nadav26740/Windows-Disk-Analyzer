using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Windows_Disk_Analyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Func<double, string> ToHumaanbleSizes = (x) => Analyzer.BytesToString((long)x);
        List<Drive_presentor> root_drives = new List<Drive_presentor>();
        Analyzer Current_analyze;

        public class Drive_presentor
        {
            public Drive_presentor(DriveInfo info)
            {
                drive_info = info;
            }

            public DriveInfo drive_info;

            public override string ToString()
            {
                try
                {
                    return drive_info.Name + " [" + Analyzer.BytesToString(drive_info.TotalSize) + " / " + Analyzer.BytesToString(drive_info.TotalSize - drive_info.TotalFreeSpace) + "]";
                }
                catch
                {
                    return "Error Failed to represent drive";
                }
            }
        }

        class file_list_presentor
        {
            public file_list_presentor(string name, long size, Files_presentor files_Presentor)
            {
                this.file_name = name;
                this.file_size = Analyzer.BytesToString(size);
                this.files_Presentor = files_Presentor;
                file_attributes = files_Presentor.Attributes.ToString();
            }
            public string file_name { get; private set; }
            public string file_size { get; private set; }
            public string file_attributes { get; private set; }
            public Files_presentor files_Presentor { get; private set; }
        }

        List<file_list_presentor> file_presentors = new List<file_list_presentor>();

        public MainWindow()
        {

            InitializeComponent();
            File_Chart.AxisY.Add(new Axis { LabelFormatter = ToHumaanbleSizes });
            SeriesCollection = new SeriesCollection();
            ThreadPool.SetMaxThreads(6, 4);
            foreach (DriveInfo driver in DriveInfo.GetDrives())
            {
                root_drives.Add(new Drive_presentor(driver));
            }

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
            try
            {
                Current_analyze = new Analyzer(dir_path);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SeriesCollection.Clear();


            var elements_list = Current_analyze.Get_elements_in_dir();
            elements_list.Sort((x, y) => y.size.CompareTo(x.size));
            long system_files_Size = 0;
            long Other_files_size = 0;

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
                    Other_files_size += elements_list[i].size;
                }
            }

            file_presentors.Clear();
            for (int i = 0; i < elements_list.Count; i++)
            {
                file_presentors.Add(new file_list_presentor(elements_list[i].Name, elements_list[i].size, elements_list[i]));
            }
            FileList.ItemsSource = file_presentors;
            FileList.Items.Refresh();

            if (system_files_Size > 0)
                SeriesCollection.Add(new PieSeries
                {
                    Title = "System Files",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(system_files_Size) },
                    DataLabels = true
                });

            if (Other_files_size > 0)
                SeriesCollection.Add(new PieSeries
                {
                    Title = "Other",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(Other_files_size) },
                    DataLabels = true
                });


            File_Chart.HideLegend();
            File_Chart.ToolTip = null;
            File_Chart.Hoverable = false;

            Size_label.Text = "Size: " + Current_analyze.get_Formated_size();
            Current_path_label.Text = Current_analyze.GetCurrentPath();
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
            LoadDrive((Disk_ComboBox.SelectedItem as Drive_presentor).drive_info.Name);
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            LoadDrive(Current_analyze.GetParent());
        }

        private void File_Chart_DataClick(object sender, ChartPoint chartPoint)
        {
            // Bug cause event appear before chart loaded
            if (chartPoint == null)
            {
                return;
            }

            try
            {
                int pressed_column_index = (int)chartPoint.X;

                if (file_presentors[pressed_column_index].files_Presentor.dir_info != null)
                {
                    LoadDrive(file_presentors[pressed_column_index].files_Presentor.dir_info.FullName);
                }
            }
            catch
            {
                return;
            }            
        }

        /// <summary>
        /// Opening the 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenInExplorerPressed(object sender, RoutedEventArgs e)
        {
            var sender_btn = sender as Button;
            Process.Start("explorer.exe", (sender_btn.DataContext as file_list_presentor).files_Presentor.dir_info.FullName);
        }
    }
}