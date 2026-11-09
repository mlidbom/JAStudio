using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications;

public class When_listing_the_tools : McpServerSpecification
{
   readonly IReadOnlyList<Tool> _tools;
   public When_listing_the_tools() => _tools = ListTools();

   string DescriptionOf(string toolName) =>
      _tools.Single(it => it.Name == toolName).Description ?? throw new InvalidOperationException($"The tool {toolName} has no description.");

   [XF] public void the_server_offers_search_notes_get_note_and_describe_collection() =>
      _tools.Select(it => it.Name).Order().Must().SequenceEqual(["describe_collection", "get_note", "search_notes"]);

   [XF] public void the_description_of_search_notes_names_every_text_property_a_search_can_have_a_condition_on() =>
      PropertiesTheDescriptionOfSearchNotesDoesNotName(NoteProperties.EveryTextProperty).Must().BeEmpty();

   [XF] public void the_description_of_search_notes_names_every_true_or_false_property_a_search_can_have_a_condition_on() =>
      PropertiesTheDescriptionOfSearchNotesDoesNotName(NoteProperties.EveryTrueOrFalseProperty).Must().BeEmpty();

   List<string> PropertiesTheDescriptionOfSearchNotesDoesNotName(IEnumerable<NoteProperty> properties) =>
      [.. properties.Select(it => it.Name).Where(name => !DescriptionOf("search_notes").Contains($"\"{name}\""))];
}
