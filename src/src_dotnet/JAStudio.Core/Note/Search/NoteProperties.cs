using System.Linq;
using JAStudio.Core.Note.Search.NoteTypeProperties;
using JAStudio.Core.Note.Search.TagProperties;
using JAStudio.Core.Note.Search.TextProperties;
using JAStudio.Core.Note.Search.TrueOrFalseProperties;

namespace JAStudio.Core.Note.Search;

/// <summary>Every <see cref="NoteProperty"/> a search can have conditions on.</summary>
public static class NoteProperties
{
   /// <summary>
   /// The texts a quick lookup looks in, with their HTML stripped: the question and answer of every note,<br/>
   /// and a vocab's or kanji's readings, in kana and in romaji. See <see cref="JPNote.SearchableTexts"/>.
   /// </summary>
   public static readonly TextProperty SearchableText = new("Searchable text", it => it.SearchableTexts().Select(StringExtensions.StripHtmlMarkup));

   /// <summary>The note's question: a vocab's word, a kanji's character, a sentence's text.</summary>
   public static readonly TextProperty Question = new("Question", it => [it.GetQuestion()]);

   /// <summary>The note's answer, as its card shows it.</summary>
   public static readonly TextProperty Answer = new("Answer", it => [it.GetAnswer()]);

   /// <summary>The note's tags. A condition on a tag includes the tag's whole branch.</summary>
   public static readonly TagsProperty Tags = new();

   /// <summary>Whether the note is a vocab, a kanji or a sentence.</summary>
   public static readonly NoteTypeProperty NoteType = new();

   /// <summary>Whether the note's reading is being studied: it has a reading card that isn't suspended.</summary>
   public static readonly TrueOrFalseProperty StudyingReading = new("Studying reading", it => it.IsStudying(CardTypes.Reading));

   /// <summary>Whether the note's listening is being studied: it has a listening card that isn't suspended.</summary>
   public static readonly TrueOrFalseProperty StudyingListening = new("Studying listening", it => it.IsStudying(CardTypes.Listening));
}
