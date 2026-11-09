using System;
using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Notes;

public class When_getting_a_note : McpServerSpecification
{
   protected CallToolResult GetNote(string noteId) => CallTool("get_note", new Dictionary<string, object?> { ["noteId"] = noteId });

   public class by_the_id_of_a_vocab : When_getting_a_note
   {
      readonly VocabNote _vocab;
      readonly CallToolResult _result;

      public by_the_id_of_a_vocab()
      {
         _vocab = CreateVocab("行く", "to go", "いく");
         _result = GetNote(_vocab.GetId().ToString());
      }

      [XF] public void the_answer_is_the_vocabs_json_as_the_corpus_stores_it() => TextOf(_result).Must().Be(GetService<NoteSerializer>().Serialize(_vocab));
   }

   public class by_the_id_of_a_kanji : When_getting_a_note
   {
      readonly KanjiNote _kanji;
      readonly CallToolResult _result;

      public by_the_id_of_a_kanji()
      {
         _kanji = CreateKanji("行", "go", "コウ", "い");
         _result = GetNote(_kanji.GetId().ToString());
      }

      [XF] public void the_answer_is_the_kanjis_json_as_the_corpus_stores_it() => TextOf(_result).Must().Be(GetService<NoteSerializer>().Serialize(_kanji));
   }

   public class by_an_id_no_note_has : When_getting_a_note
   {
      readonly string _idNoNoteHas = Guid.NewGuid().ToString();
      readonly CallToolResult _result;

      public by_an_id_no_note_has() => _result = GetNote(_idNoNoteHas);

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_says_no_note_has_the_id() => TextOf(_result).Must().Contain($"No note has the id {_idNoNoteHas}");
   }
}
