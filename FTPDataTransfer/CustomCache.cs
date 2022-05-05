using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Runtime.Caching;

namespace FTPDataTransfer
{
    class CustomCache
    {        
        public readonly MemoryCache _memCache;
        public readonly CacheItemPolicy _cacheItemPolicy;        

        public CustomCache() {
            _memCache = MemoryCache.Default;

            _cacheItemPolicy = new CacheItemPolicy()
            {
                RemovedCallback = OnRemovedFromCache
            };
        }

        // Handle cache item expiring
        private void OnRemovedFromCache(CacheEntryRemovedArguments args)
        {
            if (args.RemovedReason != CacheEntryRemovedReason.Expired) return;

            // Now actually handle file event
            var e = (FileSystemEventArgs)args.CacheItem.Value;

            // Specify what is done when a file is changed, created, or deleted.
            //Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");

            if (WatcherChangeTypes.Created.Equals(e.ChangeType) || WatcherChangeTypes.Changed.Equals(e.ChangeType))
            {
                FileInfo file = new FileInfo(e.FullPath);
                IConfigurationSection ftp = Program.configuration.GetSection("ftp");

                try
                {
                    Console.WriteLine("\n");
                    Console.WriteLine($"File: {e.Name} {e.ChangeType}");
                    Console.WriteLine("\n");
                    Program.LoadFile(ftp.GetValue<string>("domain"), ftp.GetValue<string>("username"), ftp.GetValue<string>("password"), file, ftp.GetValue<string>("port"));
                    Console.WriteLine("\n");
                    Console.WriteLine($"<<Sucess>> File: {file.Name} sent to upload!");
                    Console.WriteLine("\n");
                }
                catch (WebException ex)
                {
                    Console.WriteLine("\n");                    
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("<<Error>> File cannot be uploaded. Please, check configuration file and try again");
                    Console.WriteLine("\n");
                    
                }
            }
        }
    }
}
