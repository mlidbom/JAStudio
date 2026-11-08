using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.Tagging;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Metadata" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabMetadataEditor"/>):<br/>
/// what kind of word the vocab is, such as its register, which doesn't affect parsing, and its tags. Every change is written to the note at once.
/// </summary>
class VocabMetadataEditorViewModel
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly Func<VocabNote> _currentVocab;

   /// <param name="services">The application's services.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   public VocabMetadataEditorViewModel(TemporaryServiceCollection services, Func<VocabNote> currentVocab)
   {
      _fatalErrorHandler = services.Resolve<IFatalErrorHandler>();
      _currentVocab = currentVocab;
      Tags = new NoteTagsEditorViewModel(services, currentVocab);

      MiscFlags = [Flag("Compositionally transparent compound", it => it.MatchingConfiguration.Flags.IsCompositionallyTransparentCompound)];
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

   public IReadOnlyList<FlagEditorViewModel> MiscFlags { get; }
   public IReadOnlyList<FlagEditorViewModel> RegisterFlags { get; }

   public NoteTagsEditorViewModel Tags { get; }

   /// <summary>Shows every flag, and the tags, as the note has them now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      foreach(var flag in MiscFlags) flag.Refresh();
      foreach(var flag in RegisterFlags) flag.Refresh();
      Tags.Refresh();
   }

   FlagEditorViewModel Flag(string label, Func<VocabNote, FlagField> flag) => new(_fatalErrorHandler, label, () => flag(_currentVocab()), () => {});
}
