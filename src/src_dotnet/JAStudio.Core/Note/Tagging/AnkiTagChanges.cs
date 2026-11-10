using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// The changes a person makes in JAStudio to the notes' Anki tags (<see cref="Tags.Anki"/>), the tags of their study-notes in Anki: adding one to<br/>
/// notes, removing one from them, and renaming or deleting a branch of them. The tags are the student's, stored only in Anki, so each change is made<br/>
/// in Anki, as one Anki operation, which Anki's Edit > Undo undoes (<see cref="IAnkiTagOperations"/>). Then JAStudio reads the study-notes from Anki<br/>
/// again (<see cref="JPCollection.ReadTheStudyNotesFromAnkiAgain"/>), so the notes show the change.
/// </summary>
/// <remarks>
/// Renaming and deleting work as in Anki, on every Anki note with a tag in the branch, of whatever note type, not only on the study-notes<br/>
/// (Magnus, 2026-11-10): "I don't end up with strange in-between results and having to do the work twice, once in jastudio and once in anki".<br/>
/// Each blocks until Anki has made the change and JAStudio has read it, so it's for the background.
/// </remarks>
public sealed class AnkiTagChanges
{
   readonly JPCollection _collection;
   readonly IAnkiTagOperations _anki;

   /// <param name="collection">The collection whose notes' Anki tags change, and which reads them again.</param>
   /// <param name="anki">Makes each change in Anki.</param>
   public AnkiTagChanges(JPCollection collection, IAnkiTagOperations anki)
   {
      _collection = collection;
      _anki = anki;
   }

   /// <summary>Adds the Anki tag <paramref name="tag"/> to the study-note of each of <paramref name="notes"/> that has one and lacks the tag: a note with no study-note can't have an Anki tag.</summary>
   public void AddTo(IReadOnlyCollection<JPNote> notes, Tag tag)
   {
      AssertItIsAnAnkiTag(tag);
      _anki.AddTheTagToTheStudyNotesOf([..notes.Where(it => it.HasAnAnkiNote() && !it.AnkiTags.Contains(tag)).Select(it => it.GetId())], Tags.Anki.NameInAnkiOf(tag.Name));
      _collection.ReadTheStudyNotesFromAnkiAgain();
   }

   /// <summary>Removes the Anki tag <paramref name="tag"/> from the study-note of each of <paramref name="notes"/> that has it.</summary>
   public void RemoveFrom(IReadOnlyCollection<JPNote> notes, Tag tag)
   {
      AssertItIsAnAnkiTag(tag);
      _anki.RemoveTheTagFromTheStudyNotesOf([..notes.Where(it => it.AnkiTags.Contains(tag)).Select(it => it.GetId())], Tags.Anki.NameInAnkiOf(tag.Name));
      _collection.ReadTheStudyNotesFromAnkiAgain();
   }

   /// <summary>Makes <paramref name="rename"/>, of a branch below <c>anki::</c> and one that can be done, on every Anki note with a tag in the branch.</summary>
   public void Rename(TagBranchRename rename)
   {
      Contract.Argument.Assert(rename.Branch.IsInTheAnkiBranch && rename.WhyItCannotBeDone is null, () => $"The rename of {rename.Branch.Path} to {rename.Renamed.Path} isn't one of Anki tags that can be done.");
      _anki.RenameTheBranchOnEveryAnkiNote(Tags.Anki.NameInAnkiOf(rename.Branch.Path), Tags.Anki.NameInAnkiOf(rename.Renamed.Path));
      _collection.ReadTheStudyNotesFromAnkiAgain();
   }

   /// <summary>Deletes <paramref name="branch"/>, a branch below <c>anki::</c>, from every Anki note with a tag in it.</summary>
   public void Delete(TagBranch branch)
   {
      AssertItIsBelowAnki(branch);
      _anki.DeleteTheBranchFromEveryAnkiNote(Tags.Anki.NameInAnkiOf(branch.Path));
      _collection.ReadTheStudyNotesFromAnkiAgain();
   }

   /// <summary>How many Anki notes, of any note type, have a tag in <paramref name="branch"/>, a branch below <c>anki::</c>: those renaming or deleting it changes.</summary>
   public int NumberOfAnkiNotesWithATagIn(TagBranch branch)
   {
      AssertItIsBelowAnki(branch);
      return _anki.NumberOfAnkiNotesWithATagIn(Tags.Anki.NameInAnkiOf(branch.Path));
   }

   static void AssertItIsAnAnkiTag(Tag tag) => AssertItIsBelowAnki(new TagBranch(tag.Name));

   static void AssertItIsBelowAnki(TagBranch branch) =>
      Contract.Argument.Assert(branch is { IsInTheAnkiBranch: true, IsTheAnkiBranch: false }, () => $"{branch.Path} isn't below {Tags.Anki.Folder}.");
}
