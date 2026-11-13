using System;
using System.IO;

namespace JAStudio.Mcp.Discovery;

/// <summary>
/// Which JAStudio a running one is, the same through its restarts: what hosts it, and the corpus checkout it works on. At most one JAStudio runs on a<br/>
/// corpus checkout at a time (<c>JAStudio.Core.Storage.CorpusCheckoutLock</c>), so a JAStudio that announces itself with the identity of one that ran<br/>
/// before, such as Anki's after Anki restarted, is that JAStudio again. Claude's MCP gateway keeps its connection to a JAStudio by it.
/// </summary>
public sealed class JAStudioIdentity : IEquatable<JAStudioIdentity>
{
   /// <summary>How two paths to the same folder compare: ignoring case where the file system does, on Windows and macOS.</summary>
   static readonly StringComparer FolderPathComparer = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

   public JAStudioIdentity(JAStudioHost host, DirectoryInfo corpusCheckout)
   {
      Host = host;
      CorpusCheckout = corpusCheckout;
   }

   public JAStudioHost Host { get; }

   /// <summary>The corpus checkout the JAStudio works on, with links followed.</summary>
   public DirectoryInfo CorpusCheckout { get; }

   /// <summary>Whether the JAStudio works on <paramref name="corpusCheckout"/>, a path with links followed.</summary>
   public bool WorksOn(DirectoryInfo corpusCheckout) => FolderPathComparer.Equals(PathOf(CorpusCheckout), PathOf(corpusCheckout));

   public bool Equals(JAStudioIdentity? other) => other is not null && Host == other.Host && other.WorksOn(CorpusCheckout);

   public override bool Equals(object? other) => other is JAStudioIdentity identity && Equals(identity);

   public override int GetHashCode() => HashCode.Combine(Host, FolderPathComparer.GetHashCode(PathOf(CorpusCheckout)));

   public static bool operator ==(JAStudioIdentity? left, JAStudioIdentity? right) => Equals(left, right);

   public static bool operator !=(JAStudioIdentity? left, JAStudioIdentity? right) => !Equals(left, right);

   /// <summary>The JAStudio as reports name it, such as <c>JAStudio inside Anki on the corpus checkout C:\Dev\JAStudio\src\jas_database</c>.</summary>
   public override string ToString() =>
      $"{Host switch
      {
         JAStudioHost.Anki => "JAStudio inside Anki",
         JAStudioHost.Standalone => "the standalone JAStudio",
         JAStudioHost.Headless => "a headless JAStudio",
         _ => throw new ArgumentOutOfRangeException(nameof(Host), Host, "A host this build doesn't know.")
      }} on the corpus checkout {CorpusCheckout.FullName}";

   static string PathOf(DirectoryInfo folder) => Path.TrimEndingDirectorySeparator(folder.FullName);
}
