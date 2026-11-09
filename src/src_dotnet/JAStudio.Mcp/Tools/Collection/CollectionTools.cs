using System.ComponentModel;
using JAStudio.Core;
using JAStudio.Core.Note.Collection;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Collection;

/// <summary>The tools about the collection as a whole.</summary>
[McpServerToolType]
public class CollectionTools
{
   readonly JPCollection _collection;
   readonly IEnvironmentPaths _paths;

   public CollectionTools(JPCollection collection, IEnvironmentPaths paths)
   {
      _collection = collection;
      _paths = paths;
   }

   [McpServerTool(Name = "describe_collection", ReadOnly = true, Idempotent = true)]
   [Description("""
                Describes the collection this JAStudio has loaded: the corpus folder it loaded the notes from, and how many notes of each type
                it has, and how many of them are being studied. Being studied comes from Anki, so it shows whether Anki's data was loaded too.
                """)]
   public CollectionDescription DescribeCollection() =>
      new(_paths.DatabaseDir,
          NoteCounts.Of(_collection.Vocab.All()),
          NoteCounts.Of(_collection.Kanji.All()),
          NoteCounts.Of(_collection.Sentences.All()));
}
