using System;
using System.Collections.Generic;
using JAStudio.Core;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Controls.NoteFields.RelatedWords;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The "Related" tab of a vocab's <see cref="VocabEditorSectionViewModel"/> (<see cref="VocabRelatedEditor"/>): the vocab's relationships to other words,<br/>
/// such as its synonyms and its ergative twins, each edited with a picker of the corpus's vocab, and the word it's derived from. Every change is written<br/>
/// to the note at once.
/// </summary>
class VocabRelatedEditorViewModel
{
   /// <param name="services">The application's services.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   public VocabRelatedEditorViewModel(TemporaryServiceCollection services, Func<VocabNote> currentVocab)
   {
      var pickable = new Lazy<IReadOnlyList<PickableVocab>>(() => PickableVocab.AllIn(services.CoreApp.Collection.Vocab));
      Relationships =
      [
         new WordSetEditorViewModel(VocabEditableFields.Synonyms, currentVocab, pickable),
         new WordSetEditorViewModel(VocabEditableFields.PerfectSynonyms, currentVocab, pickable),
         new WordSetEditorViewModel(VocabEditableFields.Antonyms, currentVocab, pickable),
         new WordSetEditorViewModel(VocabEditableFields.ErgativeTwins, currentVocab, pickable),
         new WordSetEditorViewModel(VocabEditableFields.SeeAlso, currentVocab, pickable),
         new WordSetEditorViewModel(VocabEditableFields.ConfusedWith, currentVocab, pickable)
      ];

      var derivedFrom = VocabEditableFields.DerivedFrom;
      DerivedFrom = new TextFieldEditorViewModel(services.Resolve<IFatalErrorHandler>(),
                                                 derivedFrom.Label,
                                                 TextFieldFormat.Line,
                                                 () => derivedFrom.Read(currentVocab()),
                                                 name => derivedFrom.Write(currentVocab(), name));
   }

   /// <summary>The vocab's relationships to sets of words: synonyms, perfect synonyms, antonyms, ergative twins, see also and confused with.</summary>
   public IReadOnlyList<WordSetEditorViewModel> Relationships { get; }

   /// <summary>The word the vocab is derived from, typed by its name, such as <c>終わる</c> for <c>終わり</c>.</summary>
   public TextFieldEditorViewModel DerivedFrom { get; }

   /// <summary>Shows every relationship as the note has it now, after a change made elsewhere, such as the other side of a relationship that goes both ways.</summary>
   public void Refresh()
   {
      foreach(var relationship in Relationships) relationship.Refresh();
      DerivedFrom.Refresh();
   }
}
