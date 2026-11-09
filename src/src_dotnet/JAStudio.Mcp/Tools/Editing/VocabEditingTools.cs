using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The tools that edit a vocab as its editors in the workspace do: through the vocab itself, so each edit is saved to the corpus checkout, and<br/>
/// passed on to the vocab's Anki note, like any edit.
/// </summary>
[McpServerToolType]
public class VocabEditingTools
{
   readonly JPCollection _collection;

   public VocabEditingTools(JPCollection collection) => _collection = collection;

   [McpServerTool(Name = "set_the_parts_of_speech_of_a_vocab", Destructive = true, Idempotent = true)]
   [Description("""
                Sets a vocab's parts of speech, replacing the ones it has, as adding and removing them in its editor in the workspace does. The
                vocab is saved to the corpus checkout, and its Anki note updated, as any edit is. Sentences aren't reparsed: a vocab's parts of
                speech decide how it's matched, such as whether it inflects as a godan or an ichidan verb, so run full_rebuild to see what the
                change does to the sentences. Every name must be one of the parts of speech JAStudio knows, such as "godan verb", "suru verb" or
                "transitive"; an unknown one changes nothing and fails with the names there are. Reports the parts of speech before and after.
                """)]
   public PartsOfSpeechChange SetThePartsOfSpeechOfAVocab(
      [Description("The id of the vocab, as search_notes lists it.")] string vocabNoteId,
      [Description("The names of all the parts of speech the vocab is to have, in any order.")] string[] partsOfSpeech)
   {
      var vocab = VocabWithTheId(vocabNoteId);
      var partsOfSpeechToSet = partsOfSpeech.Select(PartOfSpeechNamed).ToList();

      var before = vocab.PartsOfSpeech.All();
      vocab.PartsOfSpeech.Set(partsOfSpeechToSet);
      return new PartsOfSpeechChange(vocab.GetQuestion(), NamesOf(before), NamesOf(vocab.PartsOfSpeech.All()));
   }

   [McpServerTool(Name = "set_the_readings_of_a_vocab", Destructive = true, Idempotent = true)]
   [Description("""
                Sets a vocab's readings, in kana, replacing the ones it has, as editing its readings in its editor in the workspace does. The vocab
                is saved to the corpus checkout, and its Anki note updated, as any edit is. Sentences aren't reparsed: run full_rebuild to see
                whether the change changes how sentences parse. Reports the readings before and after.
                """)]
   public ReadingsChange SetTheReadingsOfAVocab(
      [Description("The id of the vocab, as search_notes lists it.")] string vocabNoteId,
      [Description("All the readings the vocab is to have, in kana, first the primary one.")] string[] readings)
   {
      var vocab = VocabWithTheId(vocabNoteId);

      var before = vocab.GetReadings();
      vocab.SetReadings([.. readings]);
      return new ReadingsChange(vocab.GetQuestion(), before, vocab.GetReadings());
   }

   [McpServerTool(Name = "set_a_matching_flag_of_a_vocab", Destructive = true, Idempotent = true)]
   [Description("""
                Sets or clears one of a vocab's matching flags, the yes-or-no settings of its matching configuration, as ticking or unticking it
                in its editor in the workspace does. A flag is named as the corpus stores it, such as "isPoisonWord" or
                "isCompositionallyTransparentCompound"; an unknown name changes nothing and fails with the names there are. The vocab is saved
                to the corpus checkout, and its Anki note updated, as any edit is. Sentences aren't reparsed: run full_rebuild to see what the
                change does to them. Reports the vocab's set flags before and after.
                """)]
   public MatchingFlagsChange SetAMatchingFlagOfAVocab(
      [Description("The id of the vocab, as search_notes lists it.")] string vocabNoteId,
      [Description("The flag's name, as the corpus stores it, such as \"isCompositionallyTransparentCompound\".")] string flag,
      [Description("true to set the flag, false to clear it.")] bool set)
   {
      var vocab = VocabWithTheId(vocabNoteId);
      var flags = vocab.MatchingConfiguration.Flags;

      var before = NamesOfTheSetFlags(flags);
      FlagNamed(flags, flag).SetTo(set);
      return new MatchingFlagsChange(vocab.GetQuestion(), before, NamesOfTheSetFlags(flags));
   }

   static FlagField FlagNamed(VocabMatchingFlags flags, string name) =>
      flags.All.SingleOrDefault(it => it.Name == name)
      ?? throw new ArgumentException($"A vocab has no matching flag named '{name}'. Its matching flags are: {string.Join(", ", flags.All.Select(it => $"\"{it.Name}\""))}.");

   static string[] NamesOfTheSetFlags(VocabMatchingFlags flags) => [.. flags.All.Where(it => it.IsSet()).Select(it => it.Name)];

   static PartOfSpeech PartOfSpeechNamed(string name) =>
      PartOfSpeech.Exists(name)
         ? PartOfSpeech.Named(name)
         : throw new ArgumentException($"There is no part of speech named '{name}'. The parts of speech JAStudio knows are: {string.Join(", ", NamesOf(PartOfSpeech.All).Select(it => $"\"{it}\""))}.");

   static string[] NamesOf(IEnumerable<PartOfSpeech> partsOfSpeech) => [.. partsOfSpeech.Select(it => it.Name)];

   VocabNote VocabWithTheId(string noteId) =>
      _collection.Vocab.WithIdOrNone(NoteId.Parse(noteId)) ?? throw new ArgumentException($"No vocab has the id {noteId}.", nameof(noteId));
}
