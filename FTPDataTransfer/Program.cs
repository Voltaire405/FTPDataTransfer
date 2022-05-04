using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace FTPDataTransfer
{
    class Program
    {
        public static IConfiguration configuration;

        static async Task Main(string[] args)
        {
            configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddCommandLine(args)
    .Build();
            try
            {
                Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("\n");
                Console.WriteLine("Press <<Enter>> key to exit!");
                Console.ReadLine();
            }

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

        public static void LoadFile(string domain, string userName, string password, FileInfo file, string port = "21")
        {
            WebClient upload = new WebClient();
            upload.Credentials = new NetworkCredential(userName, password);
            upload.UploadFile($"ftp://{domain}:{port}/{file.Name}", file.FullName);

        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Run()
        {

            // If a directory is not specified, exit program.
            if (!configuration.GetSection("ftp").Exists())
            {
                // Display the proper way to call the program.
                Console.WriteLine("Please, enter valid ftp configuration");
                return;
            }

            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = configuration.GetValue<string>("path");

                Console.WriteLine($"Watching: {watcher.Path}");

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                /*watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;*/
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                // Only watch text files.
                watcher.Filter = "*.xml"; // * to filter all formats

                // Add event handlers.
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                //watcher.Deleted += OnChanged;
                //watcher.Renamed += OnRenamed;

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
                IConfigurationSection ftp = configuration.GetSection("ftp");

                try
                {
                    //LoadFile(ftp.GetValue<string>("domain"), ftp.GetValue<string>("username"), ftp.GetValue<string>("password"), file, ftp.GetValue<string>("port"));
                    Console.WriteLine($"<<Sucess>> File: {file.FullName} sent to upload!");
                    Console.WriteLine("\n");
                }
                catch (WebException ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("\n");
                    Console.WriteLine("<<Error>> File cannot be uploaded. Please, check configuration file and try again");
                }
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e) =>
            // Specify what is done when a file is renamed.
            Console.WriteLine($"File: {e.OldFullPath} renamed to {e.FullPath}");
    }
}
