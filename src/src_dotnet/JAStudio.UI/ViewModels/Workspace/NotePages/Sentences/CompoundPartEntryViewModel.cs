using System.Collections.Generic;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

/// <summary>
/// One part of a compound in a sentence's breakdown, under the compound's entry (<see cref="CompoundPartViewModel"/>): the part's vocab, with its audio,<br/>
/// readings, meta tags and answer, indented by how deep it is in the compound, since a part may be a compound with parts of its own. A right-click on<br/>
/// it gives the part's menu.
/// </summary>
sealed class CompoundPartEntryViewModel : INotePageElement
{
   /// <summary>How far the entry is indented for each level the part is below the compound, in pixels, as the cards indent it.</summary>
   const double IndentationPerLevel = 30;

   public CompoundPartEntryViewModel(CompoundPartViewModel part)
   {
      Audio = AudioButtonViewModel.For(part.VocabNote.PreferredAudio);
      Question = new NoteLinkViewModel(part.Question, part.VocabNote.GetId());
      Readings = part.DisplayReadings ? part.Readings : null;
      MetaTags = MetaTagViewModel.RowOf(part.MetaTags);
      Answer = part.Answer;
      IsHighlighted = part.IsHighlighted;
      Indentation = IndentationPerLevel * (part.Depth + 1);
   }

   public AudioButtonViewModel? Audio { get; }

   public NoteLinkViewModel Question { get; }

   /// <summary>The part's readings, unless they're what its question shows.</summary>
   public string? Readings { get; }

   public IReadOnlyList<MetaTagViewModel> MetaTags { get; }

   /// <summary>The part's answer, with its markup.</summary>
   public string Answer { get; }

   /// <summary>Whether the sentence highlights the part's word.</summary>
   public bool IsHighlighted { get; }

   public double Indentation { get; }
}
