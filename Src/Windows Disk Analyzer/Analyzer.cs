using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;

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
        static public float BIG_DIRECTORY_SIZE_GB = 3 ;

        static private Dictionary<string, Files_presentor> HeavyScanned = new Dictionary<string, Files_presentor>();

        private static long DeepSizeScan(DirectoryInfo dir_info)
        {
            long size = 0;
            List<Task<long>> tasks = new List<Task<long>>();
            if (!dir_info.Exists)
                return 0;

            try
            {
                dir_info.EnumerateFiles();
            }
            catch (UnauthorizedAccessException err)
            {
                return 0;
            }


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

                    last_tested = "[FILE]" + file.Name;
                    if ((file.UnixFileMode & UnixFileMode.UserRead) != 0 || (file.UnixFileMode & UnixFileMode.GroupRead) != 0)
                    {
                        size += file.Length;

                    }
                }
                catch (Exception err)
                {
                    Debug.WriteLine("!" + last_tested + " >> " + err.Message);
                    return 0;
                }
            }
            
            try
            {
                foreach (var dir in dir_info.EnumerateDirectories())
                {
                    last_tested = "[DIR]" + dir.FullName;
                    if (!dir.Exists || dir.Attributes.ToString().IndexOf(FileAttributes.System.ToString()) != -1)
                    {
                        Debug.WriteLine(dir.FullName + " Dir not exists" + (dir.Attributes.ToString().IndexOf("system")));
                        continue ;
                    }

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
                    task.Wait();
                    size += task.Result;
                }
                catch (Exception err) { Debug.WriteLine("ERROR in Deepsearch [" + dir_info.FullName + "]" + err.Message); }
            }

            if (size > BIG_DIRECTORY_SIZE_GB * Math.Pow(1024, 3) && !HeavyScanned.ContainsKey(dir_info.FullName))
            {
                HeavyScanned.Add(dir_info.FullName, new Files_presentor 
                    {   Attributes=dir_info.Attributes.ToString(),
                        dir_info = dir_info,
                        Name = dir_info.Name,
                        size = size
                    });
            }

            return size;
        }

        string path = string.Empty;
        DirectoryInfo dir_info;
        long dirsize = 0;

        List<Files_presentor> files_in_dir = new List<Files_presentor>();

        public Analyzer(string path)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
            {
                throw new Exception("Error path doesn't exists!");
            }

            dir_info = dirInfo;
            dirsize = dir_info.EnumerateFiles().Sum(f => f.Length);
            foreach (var file in dir_info.EnumerateFiles())
            {
                Debug.Write("$$> " + file.FullName + " = " + BytesToString(file.Length));
                Debug.WriteLine(" - Attributes: " + file.Attributes.ToString());
                files_in_dir.Add(new Files_presentor { Name = file.Name, Attributes = file.Attributes.ToString(), size=file.Length });
            }
            foreach (var direc in dir_info.EnumerateDirectories())
            {
                

                Debug.Write("DIR> " + direc.FullName);
                try
                {
                    long dir_size = HeavyScanned.ContainsKey(direc.FullName) ? HeavyScanned[direc.FullName].size : DeepSizeScan(direc);
                    

                    dirsize += dir_size;
                    files_in_dir.Add(new Files_presentor { Name = direc.Name, Attributes = direc.Attributes.ToString(), size = dir_size, dir_info=direc });
                    if (dir_size > 3 * Math.Pow(1024, 3) && !HeavyScanned.ContainsKey(dir_info.FullName))
                    {
                        HeavyScanned.Add(direc.FullName, files_in_dir.Last());
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

        public long get_Dir_Size()
        {
            return dirsize; 
        }

        public string get_Formated_size()
        {
            return BytesToString(dirsize);
        }

        public string GetParent()
        {
            return dir_info.Parent == null ? dir_info.FullName : dir_info.Parent.FullName; 
        }

        public string GetCurrentPath()
        {
            return dir_info.FullName;
        }

        public IEnumerable<FileInfo> GetFiles() => dir_info.EnumerateFiles();

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

        public List<Files_presentor> Get_elements_in_dir()
        {
            return files_in_dir;
        }

    }
}
