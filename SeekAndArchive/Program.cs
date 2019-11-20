using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SeekAndArchive
{
    class Program
    {
        static List<FileInfo> collectedFiles;
        static List<FileSystemWatcher> watcherList;
        static List<DirectoryInfo> directoryList;
        static void Main(string[] args)
        {
            collectedFiles = new List<FileInfo>();
            watcherList = new List<FileSystemWatcher>();
            directoryList = new List<DirectoryInfo>();

            string specifiedFile = args[0];
            string specifiedDir = Path.GetFullPath(args[1]);
            DirectoryInfo directoryInfo = new DirectoryInfo(specifiedDir);

            if (!directoryInfo.Exists)
            {
                Console.WriteLine("Directory does not exist");
                Console.Read();
                return;
            }
            
            RecursiveAlgorithm(collectedFiles, directoryInfo, specifiedFile);

            Console.WriteLine("Found {0} files at given {1} directory.", collectedFiles.Count, specifiedDir);

            foreach (FileInfo item in collectedFiles)
            {
                FileSystemWatcher Watcher = new FileSystemWatcher(item.DirectoryName, item.Name);
                {
                    Watcher.NotifyFilter = NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName;
                    Watcher.Filter = item.Name;

                    Watcher.Changed += new FileSystemEventHandler(Watcher_Changed);
                    Watcher.Created += new FileSystemEventHandler(Watcher_Changed);
                    Watcher.Renamed += new RenamedEventHandler(Watcher_Changed);
                    Watcher.Deleted += new FileSystemEventHandler(Watcher_Changed);

                    Watcher.EnableRaisingEvents = true;

                    watcherList.Add(Watcher);
                }
                
            }

            /*for (int i = 0; i < collectedFiles.Count; i++)
            {
                Directory.SetCurrentDirectory(specifiedDir);
                directoryList.Add(Directory.CreateDirectory("archive" + i.ToString()));
            }*/

            Console.Read();
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileSystemWatcher actWatcher = (FileSystemWatcher)sender;
            int index = watcherList.IndexOf(actWatcher);

            if (e.ChangeType == WatcherChangeTypes.Changed )
            {
                Console.WriteLine("{0} was changed.", e.FullPath);
                ArchiveFile(directoryList[index], collectedFiles[index].Name);
            }
        }

        private static void ArchiveFile(DirectoryInfo directoryList, string fileName)
        {
            FileStream inStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            FileStream outStream = new FileStream(directoryList.FullName + @"" +fileName + ".gz", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            GZipStream gZIP = new GZipStream(outStream, CompressionMode.Compress);
            inStream.CopyTo(gZIP);

            gZIP.Close(); outStream.Close(); inStream.Close();
        }

        private static void RecursiveAlgorithm(List<FileInfo> actFiles, DirectoryInfo actDirectory, string providedName)
        {
            //bool myMatch;
            foreach (FileInfo item in actDirectory.GetFiles())
            {

                if (item.Name.Substring(0, 4).Equals(providedName.Substring(0, 4)) && item.Name.Substring(item.Name.Length - 3).Equals(providedName.Substring(providedName.Length - 3)))
                {
                    actFiles.Add(item);
                    Console.WriteLine(item.Name);
                }
            }

            foreach (DirectoryInfo tempDir in actDirectory.GetDirectories())
            {
                RecursiveAlgorithm(actFiles, tempDir, providedName);
            }
            
        }
    }
}
