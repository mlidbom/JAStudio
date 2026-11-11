using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir.Sources.Subtitles;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A collection of subtitles kept as a zip of a folder per show, as the kitsunekko backup is, read for the reservoir (<see cref="SubtitleCollection"/>):<br/>
/// each show a work, its sentences those of the subtitle files in its folder, and of the zips there.
/// </summary>
public abstract class When_a_zipped_collection_of_subtitles_is_read_for_the_reservoir : IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-subtitle-collection-spec-");
   readonly List<string> _toldOfUnreadableArchives = [];

   public void Dispose() => _folder.Delete(recursive: true);

   /// <summary>Reads the collection zipped with <paramref name="filesByPath"/>, each show's sentences as their positions and texts, by show.</summary>
   protected Dictionary<string, string[]> SentencesByShowIn(params (string Path, byte[] Content)[] filesByPath)
   {
      var zip = new FileInfo(Path.Combine(_folder.FullName, "collection.zip"));
      using(var archive = ZipFile.Open(zip.FullName, ZipArchiveMode.Create))
      {
         foreach(var (path, content) in filesByPath)
         {
            using var entry = archive.CreateEntry(path).Open();
            entry.Write(content);
         }
      }

      return SubtitleCollection.WorksInZip("specs", zip, _toldOfUnreadableArchives.Add)
                               .ToDictionary(it => it.Source.Work, it => it.Sentences.Select(sentence => $"{sentence.Position}: {sentence.Text}").ToArray());
   }

   protected string ToldOfTheUnreadableArchive => _toldOfUnreadableArchives.Single();

   protected static byte[] Srt(string line) => Encoding.UTF8.GetBytes($"1\r\n00:00:01,000 --> 00:00:02,000\r\n{line}\r\n");

   protected static byte[] ZipHolding(string fileName, byte[] content)
   {
      using var bytes = new MemoryStream();
      using(var archive = new ZipArchive(bytes, ZipArchiveMode.Create, leaveOpen: true))
      {
         using var entry = archive.CreateEntry(fileName).Open();
         entry.Write(content);
      }

      return bytes.ToArray();
   }

   public class with_a_zip_of_subtitles_in_a_shows_folder : When_a_zipped_collection_of_subtitles_is_read_for_the_reservoir
   {
      readonly Dictionary<string, string[]> _sentencesByShow;

      public with_a_zip_of_subtitles_in_a_shows_folder() =>
         _sentencesByShow = SentencesByShowIn(("Natsume/01.srt", Srt("行くぞ")), ("Natsume/more.zip", ZipHolding("02.srt", Srt("いやだ"))));

      [XF] public void the_subtitles_in_the_zip_are_read_with_the_others() => _sentencesByShow["Natsume"].Must().SequenceEqual(["01.srt 00:00:01,000: 行くぞ", "02.srt 00:00:01,000: いやだ"]);
   }

   public class with_a_corrupt_zip_in_a_shows_folder : When_a_zipped_collection_of_subtitles_is_read_for_the_reservoir
   {
      readonly Dictionary<string, string[]> _sentencesByShow;

      public with_a_corrupt_zip_in_a_shows_folder() =>
         _sentencesByShow = SentencesByShowIn(("Natsume/01.srt", Srt("行くぞ")),
                                              ("Natsume/corrupt.zip", Encoding.ASCII.GetBytes("no zip, as a download cut short leaves one")),
                                              ("Aria/01.srt", Srt("いやだ")));

      [XF] public void the_shows_other_subtitles_are_still_read() => _sentencesByShow["Natsume"].Must().SequenceEqual(["01.srt 00:00:01,000: 行くぞ"]);
      [XF] public void the_other_shows_are_read_as_usual() => _sentencesByShow["Aria"].Must().SequenceEqual(["01.srt 00:00:01,000: いやだ"]);
      [XF] public void it_is_told_of_naming_the_zip() => ToldOfTheUnreadableArchive.Must().Contain("corrupt.zip");
      [XF] public void it_is_told_of_naming_the_show() => ToldOfTheUnreadableArchive.Must().Contain("Natsume");
   }
}
