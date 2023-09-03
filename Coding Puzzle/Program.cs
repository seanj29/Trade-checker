using System.IO;
using dotenv.net;
using FileHelpers;
namespace Coding_Puzzle;

class Program
    {
        static void Main(string[] args)
        {   
            DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));
            var path = DotEnv.Read()["PATH"];
            var engine = new FileHelperEngine<Trade>();
            if (File.Exists(path)) {
               
                var result = engine.ReadFile(path);
                foreach (Trade trades in result) {
                    Console.WriteLine($"Trade = {trades.TradeID} {trades.ISIN} {trades.Notional}");
                }
            }

            else {
            Console.WriteLine("Specified file does not "+
                      "exist in the current directory.");
            }
            using var watcher = new FileSystemWatcher(@"D:\temp");

                watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.Error += OnError;

                watcher.Filter = "*.txt";
                watcher.IncludeSubdirectories = false;
                watcher.EnableRaisingEvents = true;

                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();          
        }
         private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
        }
         private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }
         private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
    }
}