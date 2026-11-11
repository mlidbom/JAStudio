using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A condition that a subject either meets or doesn't. The subject is a word in its sentence (<see cref="CandidateWordVariant"/>), a token<br/>
/// (<see cref="IAnalysisToken"/>), or a text. A vocab's match-condition is a condition on a word: a word can match the vocab only when it meets it.
/// </summary>
/// <remarks>
/// Conditions combine into a tree: <see cref="ConditionGroup"/> makes "all of", "any of" and "not", and a <see cref="SubjectCondition{TFrom,TSubject}"/><br/>
/// holds a tree of conditions on another subject, such as the token preceding the word.<br/>
/// Conditions are values: two built from the same parts, in the same order, are equal. See <c>dev_docs/matching-configuration.md</c>.
/// </remarks>
public abstract record Condition<TSubject> where TSubject : class
{
   const string Indentation = "   ";

   /// <summary>Whether <paramref name="subject"/> meets this condition. An empty condition (<see cref="IsEmpty"/>) is met by every subject.</summary>
   public abstract bool IsMetBy(TSubject subject);

   /// <summary>
   /// Whether this condition adds nothing: a group with no conditions in it, or only empty ones, or a condition on an empty one.<br/>
   /// The group it's in ignores it, as the note search ignores an empty group (<see cref="JAStudio.Core.Note.Search.NoteCondition.IsEmpty"/>), so that a<br/>
   /// group just added changes nothing until it has conditions.
   /// </summary>
   public virtual bool IsEmpty => false;

   /// <summary>The condition as the editor would show it: one line per condition, with each group's conditions indented under it.</summary>
   public abstract IEnumerable<string> DescriptionLines();

   /// <summary>
   /// The <see cref="DescriptionLines"/> with <paramref name="prefix"/> in front of the first, as a condition holding this one shows it on the same line<br/>
   /// as itself, such as "Not: " or "The preceding token: ".
   /// </summary>
   internal IEnumerable<string> DescriptionLinesPrefixedWith(string prefix)
   {
      var lines = DescriptionLines().ToList();
      return [prefix + lines[0], ..lines.Skip(1)];
   }

   /// <summary>The condition as the corpus stores it: an object whose key says what kind of condition it is. <see cref="ConditionLevel{TSubject}"/> reads it back.</summary>
   internal abstract JsonObject ToJson();

   /// <summary>
   /// The texts that the <see cref="TextComparison"/>s in this condition compare with, one <see cref="TextSet"/> for each: its own when it is one,<br/>
   /// else those of the conditions it holds. None for a condition that compares no text.
   /// </summary>
   internal virtual IEnumerable<TextSet> ComparedTextSets() => [];

   public sealed override string ToString() => string.Join(Environment.NewLine, DescriptionLines());

   /// <summary><paramref name="line"/> indented one level, for a condition shown under the group it's in.</summary>
   protected static string Indented(string line) => Indentation + line;
}
