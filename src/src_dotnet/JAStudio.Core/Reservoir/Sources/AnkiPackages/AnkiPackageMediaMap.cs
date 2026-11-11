using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Compze.Contracts;
using ZstdSharp;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// The map of an Anki package's media, kept in its zip as the entry <c>media</c>: which numbered zip entry holds which media file. The older<br/>
/// packages keep it as JSON, <c>{"0": "show_01.mp3", …}</c>, and their media files as they are; the latest format, that of the packages with a<br/>
/// <c>collection.anki21b</c>, keeps it as a zstd-compressed protobuf message, the n-th file in the zip entry named n, each file zstd-compressed.
/// </summary>
/// <remarks>
/// The protobuf message is Anki's <c>MediaEntries</c>: <c>repeated MediaEntry entries = 1</c>, each <c>MediaEntry</c> with <c>string name = 1</c>,<br/>
/// <c>uint32 size = 2</c> and <c>bytes sha1 = 3</c>. It's read here field by field rather than with a protobuf library, for the one message.
/// </remarks>
public sealed class AnkiPackageMediaMap
{
   const int ProtobufLengthDelimited = 2;
   const int ProtobufVarint = 0;
   const int MediaEntriesEntriesField = 1;
   const int MediaEntryNameField = 1;

   AnkiPackageMediaMap(IReadOnlyList<(string ZipEntryName, string FileName)> files, bool filesAreCompressed)
   {
      Files = files;
      FilesAreCompressed = filesAreCompressed;
   }

   /// <summary>The map the <c>media</c> entry's bytes, <paramref name="mapEntry"/>, hold, in either format.</summary>
   public static AnkiPackageMediaMap Of(byte[] mapEntry) =>
      IsJson(mapEntry) ? new AnkiPackageMediaMap(FilesInJson(mapEntry), filesAreCompressed: false) : new AnkiPackageMediaMap(FilesInProtobuf(Decompressed(mapEntry)), filesAreCompressed: true);

   /// <summary>Each media file's zip entry, by the name the zip keeps it under, and the file's own name, in the map's order.</summary>
   public IReadOnlyList<(string ZipEntryName, string FileName)> Files { get; }

   /// <summary>Whether each media file is zstd-compressed in its zip entry, as in the latest format.</summary>
   public bool FilesAreCompressed { get; }

   static bool IsJson(byte[] mapEntry) => mapEntry.Length > 0 && mapEntry[0] == (byte)'{';

   static IReadOnlyList<(string, string)> FilesInJson(byte[] mapEntry) =>
      [.. (JsonSerializer.Deserialize<Dictionary<string, string>>(mapEntry) ?? throw new InvalidDataException("The package's media map is JSON null.")).Select(it => (it.Key, it.Value))];

   static byte[] Decompressed(byte[] compressed)
   {
      using var decompressor = new Decompressor();
      return decompressor.Unwrap(compressed).ToArray();
   }

   /// <summary>The files of a <c>MediaEntries</c> message, the n-th kept in the zip entry named n.</summary>
   static IReadOnlyList<(string, string)> FilesInProtobuf(byte[] message)
   {
      var names = new List<string>();
      var position = 0;
      while(position < message.Length)
      {
         var (field, entry) = LengthDelimitedField(message, ref position);
         if(field == MediaEntriesEntriesField) names.Add(NameIn(entry));
      }

      return [.. names.Select((name, index) => (index.ToString(CultureInfo.InvariantCulture), name))];
   }

   /// <summary>The <c>name</c> of a <c>MediaEntry</c> message, <paramref name="entry"/>, whose other fields are skipped.</summary>
   static string NameIn(ReadOnlySpan<byte> entry)
   {
      var name = "";
      var position = 0;
      while(position < entry.Length)
      {
         var key = Varint(entry, ref position);
         var (field, wireType) = ((int)(key >> 3), (int)(key & 7));
         if(wireType == ProtobufVarint)
         {
            Varint(entry, ref position);
            continue;
         }

         Contract.State.Assert(wireType == ProtobufLengthDelimited, () => $"A MediaEntry's field {field} has the wire type {wireType}, which Anki's MediaEntry has none of.");
         var length = (int)Varint(entry, ref position);
         if(field == MediaEntryNameField) name = Encoding.UTF8.GetString(entry.Slice(position, length));
         position += length;
      }

      return name;
   }

   static (int Field, byte[] Value) LengthDelimitedField(byte[] message, ref int position)
   {
      var key = Varint(message, ref position);
      Contract.State.Assert((key & 7) == ProtobufLengthDelimited, () => $"A MediaEntries message's field {key >> 3} isn't length-delimited, as its entries are.");
      var length = (int)Varint(message, ref position);
      var value = message[position..(position + length)];
      position += length;
      return ((int)(key >> 3), value);
   }

   /// <summary>The protobuf varint at <paramref name="position"/> in <paramref name="bytes"/>, which is moved past it: seven bits a byte, the least significant first.</summary>
   static ulong Varint(ReadOnlySpan<byte> bytes, ref int position)
   {
      var value = 0UL;
      for(var shift = 0;; shift += 7)
      {
         var current = bytes[position++];
         value |= (ulong)(current & 0x7F) << shift;
         if(current < 0x80) return value;
      }
   }
}
