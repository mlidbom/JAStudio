using System;
using System.Collections.Generic;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Controls;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Metadata" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabMetadataEditor"/>):<br/>
/// what kind of word the vocab is, such as its register, which doesn't affect parsing. Every change is written to the note at once.
/// </summary>
class VocabMetadataEditorViewModel
{
   readonly Func<VocabNote> _currentVocab;

   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   public VocabMetadataEditorViewModel(Func<VocabNote> currentVocab)
   {
      _currentVocab = currentVocab;

      MiscFlags = [Flag("Compositionally transparent compound", it => it.MatchingConfiguration.BoolFlags.IsCompositionallyTransparentCompound)];
      RegisterFlags =
      [
         Flag("Archaic", it => it.Register.Archaic),
         Flag("Childish", it => it.Register.Childish),
         Flag("Derogatory", it => it.Register.Derogatory),
         Flag("Formal", it => it.Register.Formal),
         Flag("Honorific", it => it.Register.Honorific),
         Flag("Humble", it => it.Register.Humble),
         Flag("Informal", it => it.Register.Informal),
         Flag("Literary", it => it.Register.Literary),
         Flag("Polite", it => it.Register.Polite),
         Flag("Rough, traditionally considered male", it => it.Register.RoughMasculine),
         Flag("Sensitive", it => it.Register.Sensitive),
         Flag("Slang", it => it.Register.Slang),
         Flag("Soft, traditionally considered feminine", it => it.Register.SoftFeminine),
         Flag("Vulgar, usually offensive", it => it.Register.Vulgar)
      ];
   }

   public IReadOnlyList<TagFlagCheckBoxViewModel> MiscFlags { get; }
   public IReadOnlyList<TagFlagCheckBoxViewModel> RegisterFlags { get; }

   /// <summary>Shows every flag as the note has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      foreach(var flag in MiscFlags) flag.Refresh();
      foreach(var flag in RegisterFlags) flag.Refresh();
   }

   TagFlagCheckBoxViewModel Flag(string label, Func<VocabNote, TagFlagField> flag) => new(label, () => flag(_currentVocab()), () => {});
}
