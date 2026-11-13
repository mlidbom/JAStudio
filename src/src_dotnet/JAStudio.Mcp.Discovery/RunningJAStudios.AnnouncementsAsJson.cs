using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Compze.InterprocessObject;

namespace JAStudio.Mcp.Discovery;

public sealed partial class RunningJAStudios
{
   /// <summary>
   /// The announcements as the shared object stores them: as JSON, which every build of JAStudio reads, and which an older build passes on with what a<br/>
   /// newer one added (<see cref="RunningJAStudio.WhatANewerBuildAnnouncedToo"/>).
   /// </summary>
   sealed class AnnouncementsAsJson : IInterprocessObjectSerializer<List<RunningJAStudio>>
   {
      static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter(), new FolderAsItsPath() } };

      public byte[] Serialize(List<RunningJAStudio> instance) => JsonSerializer.SerializeToUtf8Bytes(instance, JsonOptions);

      public List<RunningJAStudio> Deserialize(byte[] data) =>
         JsonSerializer.Deserialize<List<RunningJAStudio>>(data, JsonOptions) ?? throw new InvalidOperationException("The running JAStudios' announcements are null.");

      /// <summary>A folder, such as a corpus checkout, as the JSON string of its full path.</summary>
      sealed class FolderAsItsPath : JsonConverter<DirectoryInfo>
      {
         public override DirectoryInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            new(reader.GetString() ?? throw new JsonException("A folder's path is null."));

         public override void Write(Utf8JsonWriter writer, DirectoryInfo value, JsonSerializerOptions options) => writer.WriteStringValue(value.FullName);
      }
   }
}
