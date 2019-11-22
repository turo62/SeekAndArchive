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
        static DirectoryInfo homeDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\" + "Archive");
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

            if (!homeDir.Exists)
            {
                Directory.SetCurrentDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                Directory.CreateDirectory("Archive");
            }

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

            Console.Read();
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            FileSystemWatcher actWatcher = (FileSystemWatcher)sender;
            int index = watcherList.IndexOf(actWatcher);

            if (e.ChangeType == WatcherChangeTypes.Changed )
            {
                Console.WriteLine("{0} was changed.", e.FullPath);
                ArchiveFile(collectedFiles[index]);
            }
        }

        private static void ArchiveFile(FileInfo fileName)
        {
            DirectoryInfo actArchive = SetActArchiveDir(fileName.FullName);
            Directory.SetCurrentDirectory(actArchive.FullName);
            Console.WriteLine(actArchive.ToString());

            int dotIndex = fileName.ToString().LastIndexOf('.');

            FileStream inStream = fileName.OpenRead();
            FileStream outStream = File.Create(fileName.ToString() + ".gz");
            GZipStream gZIP = new GZipStream(outStream, CompressionMode.Compress);

            int b = inStream.ReadByte();

            while (b != -1)
            {
                gZIP.WriteByte((byte)b);
                b = inStream.ReadByte();
            }

            gZIP.Close(); outStream.Close(); inStream.Close();
        }

        private static DirectoryInfo SetActArchiveDir(string fileName)
        {
            string tempDir = Path.GetFullPath(fileName).Substring(3);
            Console.WriteLine(tempDir);
            int tempIndex = tempDir.LastIndexOf(@"\");
            Console.WriteLine(tempIndex);
            DirectoryInfo pathToFile = new DirectoryInfo(tempDir.Substring(0, tempIndex));
            Console.WriteLine(pathToFile);

            foreach (DirectoryInfo actDir in homeDir.GetDirectories())
            {
                if (actDir.Equals(pathToFile))
                {
                    Console.WriteLine(actDir.ToString());
                    return actDir;
                }
            }

            DirectoryInfo actArchive = Directory.CreateDirectory(homeDir.ToString() + @"\" + pathToFile.ToString());

            Console.WriteLine(actArchive.ToString());

            return actArchive;
        }

        private static void RecursiveAlgorithm(List<FileInfo> actFiles, DirectoryInfo actDirectory, string providedName)
        {
            
            foreach (FileInfo item in actDirectory.GetFiles())
            {

                if (item.Name.Substring(0, 4).Equals(providedName.Substring(0, 4)) && item.Name.Substring(item.Name.Length - 3).Equals(providedName.Substring(providedName.Length - 3)))
                {
                    actFiles.Add(item);
                    Console.WriteLine(item.Name);
                }
               /* string pattern = Regex.Escape(providedName).Replace("\\*", ".*");

                if (Regex.IsMatch(item.Name, pattern))
                {
                    actFiles.Add(item);
                    Console.WriteLine(item.Name);
                }*/
            }

            foreach (DirectoryInfo tempDir in actDirectory.GetDirectories())
            {
                RecursiveAlgorithm(actFiles, tempDir, providedName);
            }
            
        }

        /*private static string ReplaceWildcard(string myFile)
        {
            string myPattern = Regex.Escape(myFile).Replace("\\?", ".").Replace("\\*", ".*?");
            return myPattern;
        }*/
    }
}
