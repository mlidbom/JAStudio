using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.Teventive.StateStore;
using Compze.xUnitBDD;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Tools.Collection;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Collection;

/// <summary>
/// A live JAStudio's MCP server starts before its collection has loaded, so that the tools that diagnose a load that fails are there when one does<br/>
/// (<see cref="Live.LiveJAStudioMcpServer"/>). Until a load has succeeded, the tools that read the notes answer with why there are none to use yet,<br/>
/// and the tools marked <see cref="WorksWithoutTheNotesAttribute"/> run, such as reload_the_collection and describe_collection.
/// </summary>
public class When_the_collection_has_no_notes_to_use_yet() : McpServerSpecification(TheCollection.NotLoadedYet)
{
   const string NotLoadedYet = "JAStudio hasn't loaded the collection yet.";

   protected CallToolResult SearchEveryNote() =>
      CallTool("search_notes", new Dictionary<string, object?> { ["search"] = new JsonObject { ["allOf"] = new JsonArray() } });

   protected CollectionDescription DescribeTheCollection() => ResultOf<CollectionDescription>(CallTool("describe_collection", new Dictionary<string, object?>()));

   protected CallToolResult ReloadTheCollection() => CallTool("reload_the_collection", new Dictionary<string, object?>());

   public class because_JAStudio_has_not_loaded_it_yet : When_the_collection_has_no_notes_to_use_yet
   {
      [XF] public void search_notes_answers_with_an_error_saying_JAStudio_has_not_loaded_the_collection_yet() =>
         new { SearchEveryNote().IsError, Text = TextOf(SearchEveryNote()) }.Must().Be(new { IsError = (bool?)true, Text = NotLoadedYet });

      [XF] public void describe_collection_says_JAStudio_has_not_loaded_the_collection_yet() => DescribeTheCollection().WhyItHasNoNotesToUseYet.Must().Be(NotLoadedYet);
   }

   public class because_its_load_failed_on_a_vocab_file_that_is_not_JSON : When_the_collection_has_no_notes_to_use_yet
   {
      const string TheBrokenFile = "00000000-0000-0000-0000-000000000001.json";
      readonly CallToolResult _theReload;

      public because_its_load_failed_on_a_vocab_file_that_is_not_JSON()
      {
         var corpusCheckout = GetService<CorpusNoteFiles>().CorpusCheckout.FullName;
         GetService<ITaggregateFilesInMemory>().WriteAllText(Path.Combine(corpusCheckout, "vocab", "00", TheBrokenFile), "{ this is not JSON");
         _theReload = ReloadTheCollection();
      }

      [XF] public void reload_the_collection_runs_and_answers_with_the_failure_naming_the_file() =>
         new { _theReload.IsError, Text = TextOf(_theReload) }.Must().Satisfy(it => it.IsError == true && it.Text.Contains(TheBrokenFile));

      [XF] public void search_notes_answers_with_an_error_saying_the_load_failed_naming_the_file() =>
         new { SearchEveryNote().IsError, Text = TextOf(SearchEveryNote()) }
           .Must().Satisfy(it => it.IsError == true && it.Text.StartsWith("JAStudio's load of the collection failed", StringComparison.Ordinal) && it.Text.Contains(TheBrokenFile));

      [XF] public void describe_collection_says_the_load_failed_naming_the_file() =>
         DescribeTheCollection().WhyItHasNoNotesToUseYet.Must().Satisfy(it => it != null && it.Contains(TheBrokenFile));
   }

   public class once_a_load_has_succeeded : When_the_collection_has_no_notes_to_use_yet
   {
      public once_a_load_has_succeeded() => ReloadTheCollection();

      [XF] public void search_notes_answers_without_an_error() => SearchEveryNote().IsError.Must().Satisfy(it => it != true);

      [XF] public void describe_collection_gives_no_reason_there_are_no_notes_to_use() => DescribeTheCollection().WhyItHasNoNotesToUseYet.Must().BeNull();
   }
}
