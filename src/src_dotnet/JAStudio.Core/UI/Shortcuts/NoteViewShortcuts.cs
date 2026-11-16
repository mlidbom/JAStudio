using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.UI.Shortcuts;

/// <summary>
/// The keyboard shortcuts that work wherever a note is being viewed:<br/>
/// in Anki's reviewer and previewer, and in JAStudio's workspace.<br/>
/// Most toggle how sentence breakdowns are shown, and a few act on the note being viewed.
/// </summary>
/// <remarks>
/// This is the one definition of these shortcuts.<br/>
/// The Anki add-on binds Anki's keys from it, and each workspace tab (<c>NoteTabView</c>) runs the ones its page reports.<br/>
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
         new("7", EditingTheViewedSentence(it => it.SourceData.Comments.Empty())),
         new("8", EditingTheViewedSentence(it => it.Configuration.IncorrectMatches.Reset())),
         new("9", EditingTheViewedVocab(it => it.CompoundParts.AutoGenerate())),
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
            kanji.Edit(it => it.Mnemonic.Empty());
            break;
         case VocabNote vocab:
            vocab.Edit(it => it.SetMnemonic(string.Empty));
            break;
      }
   }

   /// <summary>Runs <paramref name="edit"/> on the viewed note, in an edit of it (<see cref="EditingANote"/>), when it is a sentence.</summary>
   static Action<JPNote?> EditingTheViewedSentence(Action<SentenceNote> edit) => viewedNote =>
   {
      if(viewedNote is SentenceNote sentence)
         sentence.Edit(edit);
   };

   /// <summary>Runs <paramref name="edit"/> on the viewed note, in an edit of it (<see cref="EditingANote"/>), when it is a vocab.</summary>
   static Action<JPNote?> EditingTheViewedVocab(Action<VocabNote> edit) => viewedNote =>
   {
      if(viewedNote is VocabNote vocab)
         vocab.Edit(edit);
   };
}
