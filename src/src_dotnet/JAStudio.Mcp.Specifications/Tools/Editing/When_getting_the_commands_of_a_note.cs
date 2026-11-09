using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary.Commands;
using JAStudio.Mcp.Tools.Editing.Commands;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

public class When_getting_the_commands_of_a_note : McpServerSpecification
{
   protected CallToolResult GetTheCommandsOf(JPNote note) =>
      CallTool("get_the_commands_of_a_note", new Dictionary<string, object?> { ["noteId"] = note.GetId().ToString() });

   /// <summary>The vocab 行く.</summary>
   public class of_a_vocab : When_getting_the_commands_of_a_note
   {
      readonly NoteCommandsReported _commands;

      public of_a_vocab() => _commands = ResultOf<NoteCommandsReported>(GetTheCommandsOf(CreateVocab("行く", "to go", "いく")));

      NoteCommandReported Command(string name) => _commands.Commands.Single(it => it.Name == name);

      [XF] public void the_commands_are_reported_for_the_vocab() => _commands.Note.Must().Be("行く");
      [XF] public void every_command_of_the_vocabs_menus_is_listed_by_its_name_in_order() => _commands.Commands.Select(it => it.Name).Must().SequenceEqual(VocabCommands.All.Select(it => it.Name));
      [XF] public void cloning_to_a_form_takes_the_form() => (Command("cloneToForm").TextTaken ?? "").Must().Be("the form to clone the vocab to");
      [XF] public void cloning_to_a_form_creates_a_vocab() => Command("cloneToForm").CreatesAVocab.Must().BeTrue();
      [XF] public void accepting_the_meaning_takes_no_text() => Command("acceptMeaning").TextTaken.Must().BeNull();
      [XF] public void accepting_the_meaning_is_labeled_as_in_the_vocabs_menu() => Command("acceptMeaning").Label.Must().Be("Accept meaning");
   }

   public class of_a_kanji : When_getting_the_commands_of_a_note
   {
      [XF] public void every_command_of_the_kanjis_menus_is_listed_by_its_name_in_order() =>
         ResultOf<NoteCommandsReported>(GetTheCommandsOf(CreateKanji("行", "go", "コウ", "い"))).Commands.Select(it => it.Name).Must().SequenceEqual(KanjiCommands.All.Select(it => it.Name));
   }

   public class of_a_sentence : When_getting_the_commands_of_a_note
   {
      [XF] public void every_command_of_the_sentences_menus_is_listed_by_its_name_in_order() =>
         ResultOf<NoteCommandsReported>(GetTheCommandsOf(CreateSentence("行った"))).Commands.Select(it => it.Name).Must().SequenceEqual(SentenceCommands.All.Select(it => it.Name));
   }
}
