using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using Compze.Contracts;
using JAStudio.Core.SysUtils.IO.Compression;
using JAStudio.Core.SysUtils.Protobuf;
using ZstdSharp;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages.Media;

/// <summary>
/// The map of an Anki package's media, kept in its zip as the entry <c>media</c>: which numbered zip entry holds which media file. The older<br/>
/// packages keep it as JSON, <c>{"0": "show_01.mp3", …}</c>, and their media files as they are; the latest format, that of the packages with a<br/>
/// <c>collection.anki21b</c>, keeps it as a zstd-compressed protobuf message, the n-th file in the zip entry named n, each file zstd-compressed.
/// </summary>
/// <remarks>
/// The protobuf message is Anki's <c>MediaEntries</c>: <c>repeated MediaEntry entries = 1</c>, each <c>MediaEntry</c> with <c>string name = 1</c>,<br/>
/// <c>uint32 size = 2</c> and <c>bytes sha1 = 3</c>. It's read here field by field (<see cref="ProtobufWireReader"/>) rather than with a protobuf<br/>
/// library, for the one message.
/// </remarks>
public sealed class AnkiPackageMediaMap
{
   const string ZipEntryName = "media";
   const int MediaEntriesEntriesField = 1;
   const int MediaEntryNameField = 1;

   static readonly AnkiPackageMediaMap OfAPackageWithNoMedia = new([], filesAreCompressed: false);

   readonly bool _filesAreCompressed;

   AnkiPackageMediaMap(IReadOnlyList<AnkiPackageMediaMapEntry> entries, bool filesAreCompressed)
   {
      Entries = entries;
      _filesAreCompressed = filesAreCompressed;
   }

   /// <summary>The media map in <paramref name="package"/>, an Anki package's zip, in either format: an empty one when it has none, and so holds no media.</summary>
   public static AnkiPackageMediaMap In(ZipArchive package) => package.GetEntry(ZipEntryName) is {} mapEntry ? Of(mapEntry.ReadAllBytes()) : OfAPackageWithNoMedia;

   /// <summary>Each media file's zip entry, and the file's own name, in the map's order.</summary>
   public IReadOnlyList<AnkiPackageMediaMapEntry> Entries { get; }

   /// <summary>
   /// The content of a media file, given <paramref name="zipEntryContent"/>, the content of the zip entry holding it: decompressed when each file<br/>
   /// is zstd-compressed, as in the latest format.
   /// </summary>
   public byte[] FileContentFrom(byte[] zipEntryContent) => _filesAreCompressed ? ZstdDecompressed(zipEntryContent) : zipEntryContent;

   /// <summary>The map the <c>media</c> entry's content, <paramref name="mapContent"/>, holds, in either format.</summary>
   static AnkiPackageMediaMap Of(byte[] mapContent) =>
      IsJson(mapContent)
         ? new AnkiPackageMediaMap(EntriesInJson(mapContent), filesAreCompressed: false)
         : new AnkiPackageMediaMap(EntriesInMediaEntries(ZstdDecompressed(mapContent)), filesAreCompressed: true);

   static bool IsJson(byte[] mapContent) => mapContent.Length > 0 && mapContent[0] == (byte)'{';

   /// <summary>The entries of a map kept as JSON, as the older packages keep it: each file's own name by its zip entry's.</summary>
   static IReadOnlyList<AnkiPackageMediaMapEntry> EntriesInJson(byte[] mapContent) =>
      [.. (JsonSerializer.Deserialize<Dictionary<string, string>>(mapContent) ?? throw new InvalidDataException("The package's media map is JSON null.")).Select(it => new AnkiPackageMediaMapEntry(it.Key, it.Value))];

   /// <summary>The entries of a <c>MediaEntries</c> message, as the latest format keeps the map: the n-th file kept in the zip entry named n.</summary>
   static IReadOnlyList<AnkiPackageMediaMapEntry> EntriesInMediaEntries(byte[] message) =>
      [.. FileNamesInMediaEntries(message).Select((name, index) => new AnkiPackageMediaMapEntry(index.ToString(CultureInfo.InvariantCulture), name))];

   /// <summary>The <c>name</c> of each <c>MediaEntry</c> of a <c>MediaEntries</c> message, <paramref name="message"/>, in its order.</summary>
   static IReadOnlyList<string> FileNamesInMediaEntries(byte[] message)
   {
      var names = new List<string>();
      var reader = new ProtobufWireReader(message);
      while(reader.HasMoreFields)
      {
         var key = reader.ReadFieldKey();
         Contract.State.Assert(key.WireType == ProtobufWireType.LengthDelimited, () => $"A MediaEntries message's field {key.FieldNumber} isn't length-delimited, as its entries are.");
         var value = reader.ReadLengthDelimited();
         if(key.FieldNumber == MediaEntriesEntriesField) names.Add(FileNameInMediaEntry(value));
      }

      return names;
   }

   /// <summary>The <c>name</c> of a <c>MediaEntry</c> message, <paramref name="entry"/>, whose other fields are skipped.</summary>
   static string FileNameInMediaEntry(byte[] entry)
   {
      var name = "";
      var reader = new ProtobufWireReader(entry);
      while(reader.HasMoreFields)
      {
         var key = reader.ReadFieldKey();
         if(key.WireType == ProtobufWireType.Varint)
         {
            reader.ReadVarint();
            continue;
         }

         Contract.State.Assert(key.WireType == ProtobufWireType.LengthDelimited, () => $"A MediaEntry's field {key.FieldNumber} has the wire type {key.WireType}, which Anki's MediaEntry has none of.");
         var value = reader.ReadLengthDelimited();
         if(key.FieldNumber == MediaEntryNameField) name = Encoding.UTF8.GetString(value);
      }

      return name;
   }

   static byte[] ZstdDecompressed(byte[] compressed)
   {
      using var decompressor = new Decompressor();
      return decompressor.Unwrap(compressed).ToArray();
   }
}
