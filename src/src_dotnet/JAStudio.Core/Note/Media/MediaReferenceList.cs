using System;
using System.Collections.Generic;
using Compze.Contracts;

namespace JAStudio.Core.Note.Media;

/// <summary>
/// One list of a note's references to media files of one kind, such as the images a vocab's source gave it, or the screenshots added to a sentence<br/>
/// since, in the order they're preferred in. Each change saves the note.
/// </summary>
/// <remarks>
/// A note has a list for what its source gave it, in its source data, and one for what the corpus added since, such as TTS audio or an image from the<br/>
/// web: see <c>corpus-data-model.md</c>, Media, "The shape".
/// </remarks>
public class MediaReferenceList<TReference> where TReference : MediaReference
{
   readonly List<TReference> _references;
   readonly NoteGuard _guard;

   public MediaReferenceList(IEnumerable<TReference> references, NoteGuard guard)
   {
      _references = [..references];
      _guard = guard;
   }

   /// <summary>The references, in the order they're preferred in.</summary>
   public IReadOnlyList<TReference> All => _references;

   /// <summary>Adds <paramref name="reference"/> last, to a file the note doesn't reference yet.</summary>
   public void Add(TReference reference) => _guard.Update(() => _references.Add(reference));

   /// <summary>Removes <paramref name="reference"/>, which the list has: a correction, as of a file that isn't the note's.</summary>
   public void Remove(TReference reference)
   {
      Contract.Argument.Assert(_references.Contains(reference), () => $"The note has no reference to {reference.FileName} here to remove.");
      _guard.Update(() => _references.Remove(reference));
   }

   /// <summary>Replaces every reference that <paramref name="isReplaced"/> picks out with <paramref name="replacement"/>, put last.</summary>
   protected void ReplaceThoseThat(Predicate<TReference> isReplaced, TReference replacement) => _guard.Update(() =>
   {
      _references.RemoveAll(isReplaced);
      _references.Add(replacement);
   });
}
