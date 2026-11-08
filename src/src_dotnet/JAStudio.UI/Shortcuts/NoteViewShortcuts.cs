using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Shortcuts;

/// <summary>
/// The keyboard shortcuts that work wherever a note is being viewed:<br/>
/// in Anki's reviewer and previewer, and in JAStudio's workspace.<br/>
/// Most toggle how sentence breakdowns are shown, and a few act on the note being viewed.
/// </summary>
/// <remarks>
/// This is the one definition of these shortcuts.<br/>
/// The Anki add-on binds Anki's keys from it, and each workspace tab (<see cref="NoteTabView"/>) runs the ones its page reports.<br/>
/// Neither refreshes anything here. The view toggles change the <see cref="JapaneseConfig"/>, the note actions
/// update the note, and each viewer refreshes on those changes in its own way.
/// </remarks>
public class NoteViewShortcuts
{
   public NoteViewShortcuts(JapaneseConfig config) =>
      All =
      [
         new("a", Toggle(config.ShowKanjiInSentenceBreakdown)),
         new("o", _ => ToggleShowCompoundPartsInSentenceBreakdown(config)),
         new("e", _ => config.ToggleAllSentenceDisplayAutoYieldFlags()),
         new("u", Toggle(config.HideAllCompounds)),
         new("i", Toggle(config.HideCompositionallyTransparentCompounds)),
         new("k", Toggle(config.ShowSentenceBreakdownInEditMode)),
         new("q", Toggle(config.ShowKanjiMnemonicsInSentenceBreakdown)),
         new("0", RemoveMnemonic),
         new("7", OnSentence(sentence => sentence.SourceData.Comments.Empty())),
         new("8", OnSentence(sentence => sentence.Configuration.IncorrectMatches.Reset())),
         new("9", OnVocab(vocab => vocab.CompoundParts.AutoGenerate())),
         new("Ctrl+Shift+Alt+s", Toggle(config.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound)),
         new("Ctrl+Shift+Alt+h", Toggle(config.AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound)),
         new("Ctrl+Shift+Alt+t", Toggle(config.AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound)),
         new("Ctrl+Shift+Alt+d", _ => config.ToggleAllSentenceDisplayAutoYieldFlags())
      ];

   public IReadOnlyList<NoteViewShortcut> All { get; }

   /// <summary>The shortcut bound to <paramref name="keyGesture"/>, or null when that key isn't a shortcut.</summary>
   public NoteViewShortcut? BoundTo(string keyGesture) => All.FirstOrDefault(it => it.KeyGesture == keyGesture);

   static Action<JPNote?> Toggle(ConfigurationValue<bool> flag) => _ => flag.Value = !flag.Value;

   /// <summary>Compound parts and edit mode don't go together, so showing compound parts leaves edit mode.</summary>
   static void ToggleShowCompoundPartsInSentenceBreakdown(JapaneseConfig config)
   {
      config.ShowCompoundPartsInSentenceBreakdown.Value = !config.ShowCompoundPartsInSentenceBreakdown.Value;
      if(config.ShowCompoundPartsInSentenceBreakdown.Value)
         config.ShowSentenceBreakdownInEditMode.Value = false;
   }

   static void RemoveMnemonic(JPNote? viewedNote)
   {
      switch(viewedNote)
      {
         case KanjiNote kanji:
            kanji.Mnemonic.Empty();
            break;
         case VocabNote vocab:
            vocab.Mnemonic.Empty();
            break;
      }
   }

   static Action<JPNote?> OnSentence(Action<SentenceNote> action) => viewedNote =>
   {
      if(viewedNote is SentenceNote sentence)
         action(sentence);
   };

   static Action<JPNote?> OnVocab(Action<VocabNote> action) => viewedNote =>
   {
      if(viewedNote is VocabNote vocab)
         action(vocab);
   };
}
