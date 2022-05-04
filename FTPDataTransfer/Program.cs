using System;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace FTPDataTransfer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Run();
        }

        public static async Task Load(string domain, string userName, string password, FileInfo file, string port = "21")
        {
            if (!file.Exists) return;

            string filename = file.Name;

            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create($"ftp://{domain}:{port }/{filename}");
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            //string userName = "testingftp@sucomunicacion.com", password = "sucomunicacion2022";
            request.Credentials = new NetworkCredential(userName, password);


            // Copy the contents of the file to the request stream.
            await using FileStream fileStream = File.Open(file.FullName, FileMode.Open, FileAccess.Read);
            await using Stream requestStream = request.GetRequestStream();
            await fileStream.CopyToAsync(requestStream);

            //var response = (FtpWebResponse)request.GetResponse();
            //Console.WriteLine($"Upload File Complete, status {response.StatusDescription}");
        }

        public static async Task LoadFile()
        {
            WebClient upload = new WebClient();
            upload.Credentials = new NetworkCredential("testingftp@sucomunicacion.com", "sucomunicacion2022");
            upload.UploadFile("ftp://sucomunicacion.com/testfile.txt", "testfile.txt");
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Run()
        {
            string[] args = Environment.GetCommandLineArgs();

            // If a directory is not specified, exit program.
            if (args.Length != 2)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: Watcher.exe (directory)");
                return;
            }

            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = args[1];

                Console.WriteLine($"Watching: {watcher.Path}");

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                // Only watch text files.
                watcher.Filter = "*.xml"; // * to filter all formats

                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Deleted += OnChanged;
                watcher.Renamed += OnRenamed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' to quit the program.");
                while (Console.Read() != 'q') ;
            }
        }

        // Define the event handlers.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

            if (WatcherChangeTypes.Created.Equals(e.ChangeType) || WatcherChangeTypes.Changed.Equals(e.ChangeType))
            {
                FileInfo file = new FileInfo(e.FullPath);

                Console.WriteLine($"File: {file.FullName} detected!");
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e) =>
            // Specify what is done when a file is renamed.
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
    }
}
