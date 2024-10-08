using System.Diagnostics;
using System.IO;

namespace Windows_Disk_Analyzer
{
    public struct Files_presentor
    {
        public string Name;
        public string Attributes;
        public long size;
        public DirectoryInfo dir_info;
    }

    public class Analyzer
    {
        // Dictonary that contains all the heavy folders that have been scanned so we won't rescan them
        private static Dictionary<string, Files_presentor> m_HeavyScanned = new Dictionary<string, Files_presentor>();
        
        // current running analyze progress from 0 to 100
        public static float Scan_Progress { get; private set; }

        // varibales about the current Folder
        private string m_path = string.Empty;
        private DirectoryInfo m_dir_info;
        private long m_dirsize = 0;

        // List of all the files in the current dir
        private List<Files_presentor> m_files_in_dir = new List<Files_presentor>();

        /// <summary>
        /// The Method scanning the tree of the directory it gets and summing it up to get the directory size
        /// </summary>
        /// <param name="dir_info">The Directory To scan</param>
        /// <param name="progress_part">How much should it add to progress after task finished</param>
        /// <returns>Total Size of the directory tree</returns>
        private static long DeepSizeScan(DirectoryInfo dir_info, float progress_part = 0)
        {
            // checking if the directory actully exists
            if (!dir_info.Exists)
                return 0;

            long size = 0;
            // List for all the sub folder task scan it will do
            List<Task<long>> tasks = new List<Task<long>>();
            
            try
            {
                // Trying to Get all the files in this directory
                dir_info.EnumerateFiles();
            }
            catch (UnauthorizedAccessException err)
            {
                return 0;
            }

            // For debug
            string last_tested = "";

            foreach (var file in dir_info.EnumerateFiles())
            {
                try
                {
                    if (!file.Exists)
                    {
                        Debug.WriteLine(file.FullName + " File not exists");
                        continue;
                    }

                    // For debug
                    last_tested = "[FILE]" + file.Name;
                    
                    // Checking if we have access to this folder
                    if ((file.UnixFileMode & UnixFileMode.UserRead) != 0 || (file.UnixFileMode & UnixFileMode.GroupRead) != 0)
                    {
                        size += file.Length;

                    }
                }
                catch (Exception err)
                {
                    // For Debug
                    Debug.WriteLine("!" + last_tested + " >> " + err.Message);
                    return 0;
                }
            }

            try
            {
                foreach (var dir in dir_info.EnumerateDirectories())
                {
                    // For Debuging!
                    last_tested = "[DIR]" + dir.FullName;
                    
                    // Checking if the Folder exists and isn't system file
                    if (!dir.Exists || dir.Attributes.ToString().IndexOf(FileAttributes.System.ToString()) != -1)
                    {
                        Debug.WriteLine(dir.FullName + " Dir not exists" + (dir.Attributes.ToString().IndexOf("system")));
                        continue;
                    }

                    // Creating new task for this Directory deep scan

                    // FIX BUG SCANNING DIR that already have been scanned!
                    tasks.Add(new Task<long>(() => DeepSizeScan(dir)));
                    tasks.Last().Start();
                }

            }
            catch (Exception err)
            {
                Debug.WriteLine("!" + last_tested + " >> " + err.Message);
                return 0;
            }

            foreach (var task in tasks)
            {
                try
                {
                    // Summing all the Tasks
                    task.Wait();
                    size += task.Result;
                }
                catch (Exception err) { Debug.WriteLine("ERROR in deepsearch [" + dir_info.FullName + "]" + err.Message); }
            }

            // Checking if the folder is bigger than GB
            // if it is than adding it to the Heavy Dictonary so we won't rescan it
            if (size > 3 * Math.Pow(1024, 3) && !m_HeavyScanned.ContainsKey(dir_info.FullName))
            {
                m_HeavyScanned.Add(dir_info.FullName, new Files_presentor
                {
                    Attributes = dir_info.Attributes.ToString(),
                    dir_info = dir_info,
                    Name = dir_info.Name,
                    size = size
                });
            }

            // adding folder part to the progress bar
            Add_To_Progress(progress_part);

            return size;
        }

        public Analyzer(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                throw new Exception("Error m_path doesn't exists!");
            }

            m_dir_info = dirInfo;
            m_dirsize = m_dir_info.EnumerateFiles().Sum(f => f.Length);

            foreach (var file in m_dir_info.EnumerateFiles())
            {
                Debug.Write("$$> " + file.FullName + " = " + BytesToString(file.Length));
                Debug.WriteLine(" - Attributes: " + file.Attributes.ToString());
                m_files_in_dir.Add(new Files_presentor { Name = file.Name, Attributes = file.Attributes.ToString(), size = file.Length });
            }

            foreach (var direc in m_dir_info.EnumerateDirectories())
            {
                Debug.Write("DIR> " + direc.FullName);
                try
                {
                    long dir_size = m_HeavyScanned.ContainsKey(direc.FullName) ? m_HeavyScanned[direc.FullName].size : DeepSizeScan(direc);


                    m_dirsize += dir_size;
                    m_files_in_dir.Add(new Files_presentor { Name = direc.Name, Attributes = direc.Attributes.ToString(), size = dir_size, dir_info = direc });
                    if (dir_size > 3 * Math.Pow(1024, 3) && !m_HeavyScanned.ContainsKey(m_dir_info.FullName))
                    {
                        m_HeavyScanned.Add(direc.FullName, m_files_in_dir.Last());
                    }

                    Debug.Write(" = " + BytesToString(dir_size));
                }
                catch (Exception err)
                {
                    Debug.Write(" [Error] ");
                }
                finally
                {
                    Debug.WriteLine(" - Attributes: " + direc.Attributes.ToString());
                }
            }


            path = dirInfo.FullName;
        }

        private static void Add_To_Progress(float amount)
        {
            Scan_Progress += amount;
        }


        // Gets Methods
        public long get_Dir_Size() => m_dirsize;

        public string get_Formated_size() => BytesToString(m_dirsize);

        public string GetCurrentPath() => m_dir_info.FullName;

        public IEnumerable<FileInfo> GetFiles() => m_dir_info.EnumerateFiles();

        public List<Files_presentor> Get_elements_in_dir() => m_files_in_dir;

        public string GetParent()
        {
            return m_dir_info.Parent == null ? m_dir_info.FullName : m_dir_info.Parent.FullName;
        }

        // Formating method
        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        

    }
}
