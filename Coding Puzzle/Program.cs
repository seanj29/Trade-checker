using System;
using System.IO;
using Microsoft.Data.Sqlite;
 using FileHelpers;
using Microsoft.VisualBasic.FileIO;
namespace HelloWorld
{

[DelimitedRecord(",")]
[IgnoreEmptyLines]
[IgnoreFirst]
public class Trade
{
	public string? TradeID;

	public string? ISIN;
/// <summary>
/// Converts Notional to int 32
/// </summary>
    [FieldConverter(ConverterKind.Int32)]
	public int Notional;
}
class Program
    {
        static void Main(string[] args)
        {
            var engine = new FileHelperEngine<Trade>();
            if (File.Exists(@"C:\Users\seano\OneDrive\Documents\Coding\C Sharp\Trade-checker\Trades.csv")) {
            Console.WriteLine("Specified file exists.");
            var result = engine.ReadFile(@"C:\Users\seano\OneDrive\Documents\Coding\C Sharp\Trade-checker\Trades.csv");
            foreach (Trade trades in result) {
                Console.WriteLine($"Trade = {trades.TradeID} {trades.ISIN} {trades.Notational}");
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
}