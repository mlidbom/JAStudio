using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.Controls.NoteFields.RelatedWords;

/// <summary>
/// A vocab a <see cref="WordSetEditor"/> offers for adding, as its box lists it: by its name and its answer. Typing any part of its name, of one of its<br/>
/// forms or of one of its readings finds it, so 入る is found by typing はいる too.
/// </summary>
sealed class PickableVocab
{
   readonly string[] _textsItIsFoundBy;

   PickableVocab(VocabNote vocab)
   {
      Name = vocab.Question.DisambiguationName;
      Answer = vocab.GetAnswer();
      _textsItIsFoundBy = [Name, .. vocab.Forms.MatchedFormsList(), .. vocab.GetReadings()];
   }

   /// <summary>The vocab's name, its <see cref="VocabNoteQuestion.DisambiguationName"/>, as <c>入る:masu</c>: what's added when it's picked.</summary>
   public string Name { get; }

   /// <summary>The vocab's answer, shown beside its name so vocab with the same text can be told apart.</summary>
   public string Answer { get; }

   /// <summary>Whether <paramref name="typed"/> is part of the vocab's name, of one of its forms, or of one of its readings.</summary>
   public bool IsFoundBy(string typed) => _textsItIsFoundBy.Any(it => it.Contains(typed, StringComparison.Ordinal));

   /// <summary>Every vocab of <paramref name="vocab"/>, in the order of their names.</summary>
   public static IReadOnlyList<PickableVocab> AllIn(VocabCollection vocab) => [.. vocab.All().Select(it => new PickableVocab(it)).OrderBy(it => it.Name, StringComparer.Ordinal)];
}
