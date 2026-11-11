using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using ZstdSharp;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// The media of an Anki package in the latest format (<see cref="AnkiPackageMedia"/>), as the packages with a <c>collection.anki21b</c> keep it: the<br/>
/// media map a zstd-compressed protobuf <c>MediaEntries</c> message, each file zstd-compressed in the zip entry named by its place in the map.
/// </summary>
public class When_the_media_of_a_package_of_the_latest_format_is_read : IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-package-media-spec-");
   readonly IReadOnlyList<PackagedMediaFile> _media;

   public When_the_media_of_a_package_of_the_latest_format_is_read()
   {
      var package = new FileInfo(Path.Combine(_folder.FullName, "deck.apkg"));
      using(var archive = ZipFile.Open(package.FullName, ZipArchiveMode.Create))
      {
         Write(archive, "media", Compressed(MediaEntries("show_01.mp3", "show_01.jpg")));
         Write(archive, "0", Compressed(Encoding.ASCII.GetBytes("the audio")));
         Write(archive, "1", Compressed(Encoding.ASCII.GetBytes("the screenshot")));
      }

      _media = [.. AnkiPackageMedia.In(package)];
   }

   [XF] public void each_file_has_the_name_the_map_gives_it() => _media.Select(it => it.Name).Must().SequenceEqual(["show_01.mp3", "show_01.jpg"]);

   [XF] public void each_files_content_is_decompressed() => _media.Select(it => Encoding.ASCII.GetString(it.Content)).Must().SequenceEqual(["the audio", "the screenshot"]);

   public void Dispose() => _folder.Delete(recursive: true);

   /// <summary>A <c>MediaEntries</c> message: a <c>MediaEntry</c> (field 1) for each name, with the name (field 1) and a size (field 2), which isn't read.</summary>
   static byte[] MediaEntries(params string[] names) =>
      [.. names.SelectMany(name => LengthDelimited(1, [.. LengthDelimited(1, Encoding.UTF8.GetBytes(name)), 0x10, 0x09]))];

   static byte[] LengthDelimited(int field, byte[] value) => [(byte)(field << 3 | 2), (byte)value.Length, .. value];

   static byte[] Compressed(byte[] content)
   {
      using var compressor = new Compressor();
      return compressor.Wrap(content).ToArray();
   }

   static void Write(ZipArchive archive, string name, byte[] content)
   {
      using var stream = archive.CreateEntry(name).Open();
      stream.Write(content);
   }
}
