using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;
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

      UsuallyWrittenInKana = new UsuallyWrittenInKanaEditorViewModel(_fatalErrorHandler, () => _currentVocab().UsuallyWrittenInKana);
      MiscFlags = [Flag(VocabEditableFields.IsCompositionallyTransparentCompound)];
      RegisterFlags =
      [
         Flag(VocabEditableFields.Archaic),
         Flag(VocabEditableFields.Childish),
         Flag(VocabEditableFields.Derogatory),
         Flag(VocabEditableFields.Formal),
         Flag(VocabEditableFields.Honorific),
         Flag(VocabEditableFields.Humble),
         Flag(VocabEditableFields.Informal),
         Flag(VocabEditableFields.Literary),
         Flag(VocabEditableFields.Polite),
         Flag(VocabEditableFields.RoughMasculine),
         Flag(VocabEditableFields.Sensitive),
         Flag(VocabEditableFields.Slang),
         Flag(VocabEditableFields.SoftFeminine),
         Flag(VocabEditableFields.Vulgar)
      ];
   }

   public UsuallyWrittenInKanaEditorViewModel UsuallyWrittenInKana { get; }
   public IReadOnlyList<FlagEditorViewModel> MiscFlags { get; }
   public IReadOnlyList<FlagEditorViewModel> RegisterFlags { get; }

   public NoteTagsEditorViewModel Tags { get; }

   /// <summary>Shows whether the vocab is usually written in kana, every flag, and the tags, as the note has them now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      UsuallyWrittenInKana.Refresh();
      foreach(var flag in MiscFlags) flag.Refresh();
      foreach(var flag in RegisterFlags) flag.Refresh();
      Tags.Refresh();
   }

   FlagEditorViewModel Flag(EditableFlagField<VocabNote> field) => new(_fatalErrorHandler, field.Label, () => field.FlagOn(_currentVocab()), () => {});
}
