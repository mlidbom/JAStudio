using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JAStudio.Mcp.Gateway.Headless;

/// <summary>The log of a run of the headless JAStudio: everything it wrote, to standard output and standard error, in a file of its own.</summary>
sealed class HeadlessJAStudioLog : IAsyncDisposable
{
   readonly TextWriter _writer;

   HeadlessJAStudioLog(FileInfo file)
   {
      File = file;
      _writer = TextWriter.Synchronized(new StreamWriter(file.FullName, append: false, Encoding.UTF8) { AutoFlush = true });
   }

   /// <summary>A new log, in a file of its own named by when it started: see <see cref="HeadlessJAStudioFolders.NewLogFile"/>.</summary>
   public static HeadlessJAStudioLog New() => new(HeadlessJAStudioFolders.NewLogFile());

   public FileInfo File { get; }

   /// <summary>Adds a line the headless JAStudio wrote. Standard output and standard error write at the same time, from threads of their own.</summary>
   public void Write(string line) => _writer.WriteLine(line);

   /// <summary>The last <paramref name="count"/> lines of the log, for reporting why the headless JAStudio failed.</summary>
   public string LastLines(int count = 40)
   {
      using var readerSharingTheFileWithTheWriter = new StreamReader(new FileStream(File.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8);
      return string.Join(Environment.NewLine, readerSharingTheFileWithTheWriter.ReadToEnd().Split(Environment.NewLine).TakeLast(count));
   }

   public ValueTask DisposeAsync() => _writer.DisposeAsync();
}
