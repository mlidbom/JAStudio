using System;
using System.IO;
using System.Text.Json;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// The access file: the file in which the Anki add-on says how to reach the HTTP server it runs for JAStudio (<see cref="AnkisHttpServerAccess"/>).<br/>
/// The add-on writes it when it starts serving, and removes it when it stops (Python's <c>ServingJAStudio</c>), so it's there only while Anki<br/>
/// serves JAStudio.
/// </summary>
/// <remarks>
/// The port and the token are new each time the add-on starts serving, so JAStudio reads the file each time it looks for Anki.<br/>
/// An Anki that ended without stopping leaves its file behind: the file then names a server that no longer answers.
/// </remarks>
public sealed class AnkisHttpServerAccessFile(FileInfo file)
{
   /// <summary>
   /// The access file every JAStudio of this user reads, wherever the JAStudio was started from: in the user's own folder for JAStudio's data.<br/>
   /// The add-on names the same file (Python's <c>the_access_file_of_this_user</c>).
   /// </summary>
   public static AnkisHttpServerAccessFile OfThisUser => new(new FileInfo(Path.Combine(TheUsersFolderForLocalData, "JAStudio", "anki-http-server-access.json")));

   /// <summary>The folder's path whether or not the folder exists yet: .NET gives an empty path for one that doesn't, unless told not to check.</summary>
   static string TheUsersFolderForLocalData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);

   public FileInfo File { get; } = file;

   /// <summary>How to reach the server, or null while no Anki serves JAStudio: the file isn't there.</summary>
   public AnkisHttpServerAccess? AccessOrNullWhileNoAnkiServes()
   {
      if(TheFilesTextOrNullWhenItIsNotThere() is not {} text) return null;

      using var access = JsonDocument.Parse(text);
      return new AnkisHttpServerAccess(Port: access.RootElement.GetProperty("port").GetInt32(),
                                       Token: access.RootElement.GetProperty("token").GetString() ?? throw new InvalidDataException($"{File.FullName} holds no token."));
   }

   /// <summary>Reads the file without first asking whether it's there: the add-on may remove it at any time.</summary>
   string? TheFilesTextOrNullWhenItIsNotThere()
   {
      try
      {
         return System.IO.File.ReadAllText(File.FullName);
      }
      catch(Exception notThere) when(notThere is FileNotFoundException or DirectoryNotFoundException)
      {
         return null;
      }
   }
}
