using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Media;

/// <summary>
/// One list of a note's references to media files of one kind, such as the images a vocab's source gave it, or the screenshots added to a sentence<br/>
/// since, in the order they're preferred in. A change publishes the list's tevent with the whole new list, which the note applies (<see cref="Apply"/>).
/// </summary>
/// <remarks>
/// A note has a list for what its source gave it, in its source data, and one for what the corpus added since, such as TTS audio or an image from the<br/>
/// web: see <c>corpus-data-model.md</c>, Media, "The shape".
/// </remarks>
public class MediaReferenceList<TReference> where TReference : MediaReference
{
   readonly JPNote _note;
   readonly Func<IReadOnlyList<TReference>, NoteTevent> _teventOf;

   /// <param name="note">The note whose list it is, which publishes its tevent.</param>
   /// <param name="references">The references as the note stores them.</param>
   /// <param name="teventOf">The list's tevent: that its references are now those it's given.</param>
   public MediaReferenceList(JPNote note, IEnumerable<TReference> references, Func<IReadOnlyList<TReference>, NoteTevent> teventOf)
   {
      _note = note;
      _teventOf = teventOf;
      All = [.. references];
   }

   /// <summary>The references, in the order they're preferred in.</summary>
   public IReadOnlyList<TReference> All { get; private set; }

   /// <summary>Adds <paramref name="reference"/> last, to a file the note doesn't reference yet.</summary>
   public void Add(TReference reference) => SetTheReferences([.. All, reference]);

   /// <summary>Removes <paramref name="reference"/>, which the list has: a correction, as of a file that isn't the note's.</summary>
   public void Remove(TReference reference)
   {
      Contract.Argument.Assert(All.Contains(reference), () => $"The note has no reference to {reference.FileName} here to remove.");
      List<TReference> references = [.. All];
      references.Remove(reference);
      SetTheReferences(references);
   }

   /// <summary>Removes every reference that <paramref name="isRemoved"/> picks out.</summary>
   protected void RemoveThoseThat(Predicate<TReference> isRemoved) => SetTheReferences([.. All.Where(it => !isRemoved(it))]);

   /// <summary>Replaces every reference that <paramref name="isReplaced"/> picks out with <paramref name="replacement"/>, put last.</summary>
   protected void ReplaceThoseThat(Predicate<TReference> isReplaced, TReference replacement) => SetTheReferences([.. All.Where(it => !isReplaced(it)), replacement]);

   /// <summary>Makes <paramref name="references"/> the list's references: what the list's tevent's applier does.</summary>
   internal void Apply(IReadOnlyList<TReference> references) => All = references;

   void SetTheReferences(IReadOnlyList<TReference> references) => _note.PublishIfTheListChanged(All, references, _teventOf);
}
