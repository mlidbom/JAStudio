using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TestUtils;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Tagging;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Tagging;

/// <summary>The notes: a vocab tagged <c>source::wani</c>, a vocab with no tags, and a kanji with no tags.</summary>
public class When_adding_a_tag_to_the_notes_found : McpServerSpecification
{
   protected const string ToReview = "-::to-review";

   protected readonly VocabNote _vocabFromWaniKani;
   protected readonly VocabNote _vocabWithNoTags;
   protected readonly KanjiNote _kanjiWithNoTags;

   public When_adding_a_tag_to_the_notes_found()
   {
      _vocabFromWaniKani = CreateVocab("行く", "to go", "いく");
      _vocabFromWaniKani.Tags.Set(Tag.FromName("source::wani"));
      _vocabWithNoTags = CreateVocab("来る", "to come", "くる");
      _kanjiWithNoTags = CreateKanji("行", "go", "コウ", "い");
   }

   protected CallToolResult AddATagToTheNotesFound(JsonObject search, string tag) =>
      CallTool("add_a_tag_to_the_notes_found", new Dictionary<string, object?> { ["search"] = search, ["tag"] = tag });

   protected static bool HasTheTag(JPNote note, string tag) => note.Tags.Contains(Tag.FromName(tag));

   public class a_label_to_the_notes_without_a_source_tag : When_adding_a_tag_to_the_notes_found
   {
      readonly NotesTheTagChangedOn _result;

      public a_label_to_the_notes_without_a_source_tag() =>
         _result = ResultOf<NotesTheTagChangedOn>(AddATagToTheNotesFound(new JsonObject { ["not"] = new JsonObject { ["tagsInclude"] = "source" } }, ToReview));

      [XF] public void the_vocab_with_no_tags_has_it() => HasTheTag(_vocabWithNoTags, ToReview).Must().BeTrue();
      [XF] public void the_kanji_with_no_tags_has_it() => HasTheTag(_kanjiWithNoTags, ToReview).Must().BeTrue();
      [XF] public void the_vocab_from_WaniKani_the_search_did_not_find_does_not() => HasTheTag(_vocabFromWaniKani, ToReview).Must().BeFalse();
      [XF] public void it_reports_two_notes_changed() => _result.NotesChanged.Must().Be(2);
      [XF] public void it_lists_the_two_notes_it_changed() => _result.Notes.Select(it => it.Question).Order().Must().SequenceEqual(["来る", "行"]);
   }

   public class a_tag_one_of_the_notes_found_already_has : When_adding_a_tag_to_the_notes_found
   {
      readonly NotesTheTagChangedOn _result;

      public a_tag_one_of_the_notes_found_already_has() =>
         _result = ResultOf<NotesTheTagChangedOn>(AddATagToTheNotesFound(new JsonObject { ["noteTypeIs"] = "vocab" }, "source::wani"));

      [XF] public void it_reports_only_the_vocab_that_did_not_have_it_as_changed() => _result.Notes.Select(it => it.Question).Must().SequenceEqual(["来る"]);
      [XF] public void both_vocab_have_it() => (HasTheTag(_vocabFromWaniKani, "source::wani") && HasTheTag(_vocabWithNoTags, "source::wani")).Must().BeTrue();
   }

   /// <summary>A tag below <c>anki::</c>, one of the notes' tags in Anki, added to the vocab's study-notes in Anki, here the pretend Anki of the specifications.</summary>
   public class an_Anki_tag_to_the_vocab : When_adding_a_tag_to_the_notes_found
   {
      readonly NotesTheTagChangedOn _result;

      public an_Anki_tag_to_the_vocab() => _result = ResultOf<NotesTheTagChangedOn>(AddATagToTheNotesFound(new JsonObject { ["noteTypeIs"] = "vocab" }, "anki::flagged"));

      [XF] public void the_study_note_of_each_vocab_has_it_in_Anki() =>
         (GetService<PretendAnki>().TagsOf(_vocabFromWaniKani).SequenceEqual(["flagged"]) && GetService<PretendAnki>().TagsOf(_vocabWithNoTags).SequenceEqual(["flagged"])).Must().BeTrue();
      [XF] public void each_vocab_shows_it_among_its_Anki_tags() =>
         (_vocabFromWaniKani.AnkiTags.Contains(Tags.Anki.For("flagged")) && _vocabWithNoTags.AnkiTags.Contains(Tags.Anki.For("flagged"))).Must().BeTrue();
      [XF] public void it_is_not_one_of_their_own_tags() => (HasTheTag(_vocabFromWaniKani, "anki::flagged") || HasTheTag(_vocabWithNoTags, "anki::flagged")).Must().BeFalse();
      [XF] public void it_reports_both_vocab_changed() => _result.NotesChanged.Must().Be(2);
   }

   public class a_tag_JAStudio_defines : When_adding_a_tag_to_the_notes_found
   {
      readonly CallToolResult _result;

      public a_tag_JAStudio_defines() => _result = AddATagToTheNotesFound(new JsonObject { ["allOf"] = new JsonArray() }, Tags.Vocab.IsIchidanHidingGodanPotential.Name);

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_failure_names_the_tag() => TextOf(_result).Must().Contain(Tags.Vocab.IsIchidanHidingGodanPotential.Name);
      [XF] public void no_note_has_the_tag() => _vocabWithNoTags.Tags.Contains(Tags.Vocab.IsIchidanHidingGodanPotential).Must().BeFalse();
   }
}
