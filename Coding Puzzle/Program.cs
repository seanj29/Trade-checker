using dotenv.net;
using FileHelpers;
using Microsoft.Data.Sqlite;
using Serilog;

namespace Coding_Puzzle;

class Program
{
    private static void Main(string[] args)
    {
        using var log = new LoggerConfiguration()
        .WriteTo.File("process-logs.txt")
        .CreateLogger();

        // Load .env with path you want
        log.Information("Loading .ENV...");
        DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false));

        string path = DotEnv.Read()["PATH"];

        log.Information(" Opening SQL Connection....");

        using var connection = new SqliteConnection(@$"Data Source={path}Trade.db");

        connection.Open();
        log.Information(" Connection Open!");

        // Creates table
        log.Information(" Creating Table....");
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS Trade (
                    TradeID TEXT, 
                    ISIN TEXT, 
                    Notional INTEGER)
               ";
            int rows = command.ExecuteNonQuery();
        }

        log.Information("Table Created!");

        // <summary>
        // File system watcher checks for any changes in the directory
        // </summary
        log.Information(@"Monitoring for changes in the C:\temp directory");
        log.Dispose();
        using var watcher = new FileSystemWatcher(@"C:\temp");

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
        using var log2 = new LoggerConfiguration()
        .WriteTo.File("process-logs.txt")
        .CreateLogger();
        log2.Information("Closing Connection...");
        log2.Information("Exiting Program...");
        log2.Dispose();
    }

    // Event for when file is created/added to a directory
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        using var log = new LoggerConfiguration()
       .WriteTo.File("process-logs.txt")
       .CreateLogger();

        log.Information("Trades.csv file added or created!");

        string? parentDirectory = Path.GetDirectoryName(e.FullPath);

        var engine = new FileHelperEngine<Trade>();

        using var connection = new SqliteConnection(@$"Data Source={parentDirectory}\Trade.db");
        connection.Open();
        using (SqliteCommand command = connection.CreateCommand())
        {
            // Deletes any current entries in the db before creating new ones.
            command.CommandText =
            @"DELETE FROM Trade";
            log.Information("Clearing Trade.db");
        }

        var results = engine.ReadFile(@$"{parentDirectory}\Trades.csv");
        log.Information("Iterating over Trade.csv...");
        foreach (Trade trades in results)
        {
            using (SqliteCommand command = connection.CreateCommand())
            {
                command.CommandText =
                @"INSERT INTO Trade (TradeID, ISIN, Notional) 
                    VALUES (@tradeid,@isin,@notional)";
                command.Parameters.AddWithValue("@tradeid", trades.TradeID);
                command.Parameters.AddWithValue("@isin", trades.ISIN);
                command.Parameters.AddWithValue("@notional", trades.Notional);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Console.WriteLine($"Trade '{trades.TradeID}' inserted successfully!");
                    log.Information($"Trade '{trades.TradeID}' inserted successfully!");
                }
                else
                {
                    Console.WriteLine($"Failed to insert Trade '{trades.TradeID}'.");
                    log.Information($"Failed to insert Trade '{trades.TradeID}'.");
                }
            }
        }

        // Moving the file to the archive
        string sourceFilePath = e.FullPath;
        string destinationFilePath = @"C:\temp\archive\Trades " + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv";
        log.Information("Moving Trades.csv to archive...");
        log.Information($"New File Path: {destinationFilePath} ");
        File.Move(sourceFilePath, destinationFilePath);
        log.Dispose();
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
            Log.Error("Error: ", ex.Message);
            Log.Error("Stacktrace: ", ex.StackTrace.ToString());
        }
    }
}