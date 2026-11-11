using System.IO.Compression;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>Writing the entries of a zip in a spec, such as the files of an Anki package it reads.</summary>
static class ZipArchiveEntryWriting
{
   /// <summary>Adds the entry <paramref name="name"/> to <paramref name="archive"/>, with <paramref name="content"/>.</summary>
   public static void WriteEntry(this ZipArchive archive, string name, byte[] content)
   {
      using var stream = archive.CreateEntry(name).Open();
      stream.Write(content);
   }
}
