using Microsoft.Data.Sqlite;
using dotenv.net;
using FileHelpers;

namespace Coding_Puzzle;

class Program
    {
        static void Main(string[] args)
        {   
           
            DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));

            string path = DotEnv.Read()["PATH"];

            using var connection = new SqliteConnection(@$"Data Source={path}Trade.db");

            connection.Open();

            // Creates table
            using (SqliteCommand command = connection.CreateCommand()){

               command.CommandText = 
               @"
                CREATE TABLE IF NOT EXISTS Trade (
                    TradeID TEXT, 
                    ISIN TEXT, 
                    Notional INTEGER)
               ";
                int rows = command.ExecuteNonQuery();
               }

            // <summary>
            // File system watcher checks for any changes in the directory
            // </summary
            using var watcher = new FileSystemWatcher(@"D:\temp");

                watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

                watcher.Created += OnCreated;
                watcher.Error += OnError;

                watcher.Filter = "Trades.csv";
                watcher.IncludeSubdirectories = false;
                watcher.EnableRaisingEvents = true;


                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();          
        }

        //Event for when file is created/added to a directory
         private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string? parentDirectory = Path.GetDirectoryName(e.FullPath);

            var engine = new FileHelperEngine<Trade>();

            using var connection = new SqliteConnection(@$"Data Source={parentDirectory}\Trade.db");
            connection.Open();
            using (SqliteCommand command = connection.CreateCommand()){
                // Deletes any current entries in the db before creating new ones.
                command.CommandText = 
                @"DELETE FROM Trade";
             }
            var results = engine.ReadFile(@$"{parentDirectory}\Trades.csv");
            foreach(Trade trades in results){
                
               using (SqliteCommand command = connection.CreateCommand()){

                    command.CommandText = 
                    @"INSERT INTO Trade (TradeID, ISIN, Notional) 
                    VALUES (@tradeid,@isin,@notional)";
                    command.Parameters.AddWithValue("@tradeid",trades.TradeID);
                    command.Parameters.AddWithValue("@isin",trades.ISIN);
                    command.Parameters.AddWithValue("@notional",trades.Notional);  

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                        {
                            Console.WriteLine($"Trade '{trades.TradeID}' inserted successfully!");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to insert product '{trades.TradeID}'.");
                        }
                }


            }
            File.Move(e.FullPath, $@"{parentDirectory}\archive\Trades.csv {DateTime.Now}");
            Console.WriteLine($@"{parentDirectory}\archive\Trades.csv {DateTime.Now}");
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