using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Anki;

/// <summary>
/// Builds Anki search query strings for finding notes and cards.
/// </summary>
public class QueryBuilder
{
   readonly VocabCollection _vocab;
   readonly KanjiCollection _kanji;
   readonly AnalysisServices _analysisServices;
   readonly ExternalNoteIdMap _externalNoteIdMap;

   public QueryBuilder(VocabCollection vocab, KanjiCollection kanji, AnalysisServices analysisServices, ExternalNoteIdMap externalNoteIdMap)
   {
      _vocab = vocab;
      _kanji = kanji;
      _analysisServices = analysisServices;
      _externalNoteIdMap = externalNoteIdMap;
   }

   const string ExcludedDeckSubstring = "*Excluded*";

   /// <summary>
   /// Combines clauses with OR, wrapping in parentheses if multiple clauses.
   /// </summary>
   static string OrClauses(IEnumerable<string> clauses)
   {
      var clauseList = clauses.ToList();
      return clauseList.Count == 1 ? clauseList[0] : $"({string.Join(" OR ", clauseList)})";
   }

   /// <summary>
   /// Creates a field search for whole word matches using regex word boundaries.
   /// </summary>
   static string FieldContainsWord(string field, params string[] words)
   {
      return OrClauses(words.Select(word => $"\"{field}:re:\\\\b{word}\\\\b\""));
   }

   /// <summary>
   /// Creates a field search for substring matches using wildcards.
   /// </summary>
   static string FieldContainsString(string field, params string[] words)
   {
      return OrClauses(words.Select(word => $"\"{field}:*{word}*\""));
   }

   /// <summary>
   /// Search for sentence notes containing the given word.
   /// If exact=false, searches for all forms of vocab notes matching the word.
   /// Ported from sentence_search()
   /// </summary>
   public string SentenceSearch(string word, bool exact = false)
   {
      var result = $"{Builtin.Note}:{NoteTypes.Sentence} ";

      string FormQuery(string form) =>
         $"(({AnkiFieldNames.Question}:*{form}* OR {FieldContainsWord(AnkiFieldNames.Sentence.ParsingResult, form)}))";

      if(!exact)
      {
         var vocabs = _vocab.WithForm(word).ToList();
         if(vocabs.Any())
         {
            var forms = vocabs.SelectMany(voc => voc.Forms.AllList()).Distinct().ToList();
            return result + "(" + string.Join("　OR ", forms.Select(FormQuery)) + ")";
         }
      }

      return result + $"({FormQuery(word)})";
   }

   /// <summary>
   /// Search for sentences that potentially match a vocab word.
   /// Ported from potentially_matching_sentences_for_vocab()
   /// </summary>
   public string PotentiallyMatchingSentencesForVocab(VocabNote vocab)
   {
      var searchStrings = vocab.MatchingConfiguration.RequiresForbids.Surface.IsRequired
                             ? vocab.Forms.AllList().ToList()
                             : vocab.Conjugator.GetStemsForAllForms().Concat(vocab.Forms.AllList()).ToList();

      return $"{Builtin.Note}:{NoteTypes.Sentence} {FieldContainsString(AnkiFieldNames.Question, searchStrings.ToArray())}";
   }

   /// <summary>
   /// Creates a query to find notes by their JPNote instances.
   /// Ported from notes_lookup()
   /// </summary>
   public string NotesLookup(IEnumerable<JPNote> notes)
   {
      return NotesByIds(notes.Select(n => n.GetId()));
   }

   /// <summary>
   /// Creates a query to find notes by their domain NoteIds.
   /// Converts to external long IDs for the query string.
   /// </summary>
   public string NotesByIds(IEnumerable<NoteId> noteIds)
   {
      var externalIds = noteIds
                       .Select(id => _externalNoteIdMap.ToExternalId(id))
                       .Where(id => id.HasValue)
                       .Select(id => id!.Value)
                       .ToList();
      return externalIds.Count > 0 ? $"{AnkiFieldNames.NoteId}:{string.Join(",", externalIds)}" : "";
   }

   /// <summary>
   /// Search for vocab notes using wildcard matching on forms, reading, or answer.
   /// Ported from single_vocab_wildcard()
   /// </summary>
   public string SingleVocabWildcard(string form) => $"{Builtin.Note}:{NoteTypes.Vocab} ({AnkiFieldNames.Vocab.Forms}:*{form}* OR {AnkiFieldNames.Vocab.Reading}:*{form}* OR {AnkiFieldNames.Answer}:*{form}*)";

   /// <summary>
   /// Search for vocab notes by exact word match in forms, reading, or answer.
   /// Ported from single_vocab_by_question_reading_or_answer_exact()
   /// </summary>
   public string SingleVocabByQuestionReadingOrAnswerExact(string search)
   {
      var hiraganaSearch = KanaUtils.KatakanaToHiragana(search);
      return $"{Builtin.Note}:{NoteTypes.Vocab} ({FieldContainsWord(AnkiFieldNames.Vocab.Forms, search)} OR {FieldContainsWord(AnkiFieldNames.Vocab.Reading, hiraganaSearch)} OR {FieldContainsWord(AnkiFieldNames.Answer, search)})";
   }

   /// <summary>
   /// Search for vocab notes by exact form match.
   /// Ported from single_vocab_by_form_exact()
   /// </summary>
   public string SingleVocabByFormExact(string form) => $"{Builtin.Note}:{NoteTypes.Vocab} {FieldContainsWord(AnkiFieldNames.Vocab.Forms, form)}";

   /// <summary>
   /// Search for vocab reading cards by exact form match.
   /// Ported from single_vocab_by_form_exact_read_card_only()
   /// </summary>
   public string SingleVocabByFormExactReadCardOnly(string form) => $"({SingleVocabByFormExact(form)}) {Builtin.Card}:{AnkiFieldNames.VocabCard.Reading}";

   /// <summary>
   /// Search for kanji notes for all kanji characters in the string.
   /// Ported from kanji_in_string()
   /// </summary>
   public string KanjiInString(string text)
   {
      var kanjiClauses = text.Select(c => $"{AnkiFieldNames.Question}:{c}");
      return $"{Builtin.Note}:{NoteTypes.Kanji} ( {string.Join(" OR ", kanjiClauses)} )";
   }

   /// <summary>
   /// Search for vocab notes containing a specific kanji.
   /// Ported from vocab_with_kanji()
   /// </summary>
   public string VocabWithKanji(KanjiNote kanji) => $"{Builtin.Note}:{NoteTypes.Vocab} {AnkiFieldNames.Vocab.Forms}:*{kanji.GetQuestion()}*";

   /// <summary>
   /// Search for vocab notes by dictionary forms extracted from text.
   /// Ported from text_vocab_lookup()
   /// </summary>
   public string TextVocabLookup(string text)
   {
      var dictionaryForms = TextAnalysis.FromText(_analysisServices, text).AllWordsStrings();
      return VocabsLookup(dictionaryForms);
   }

   /// <summary>
   /// Creates a vocab clause for a single form.
   /// Ported from vocab_clause()
   /// </summary>
   static string VocabClause(string form) => FieldContainsWord(AnkiFieldNames.Vocab.Forms, form);

   /// <summary>
   /// Search for vocab notes matching any of the given dictionary forms.
   /// Ported from vocabs_lookup()
   /// </summary>
   static string VocabsLookup(IEnumerable<string> dictionaryForms)
   {
      var forms = dictionaryForms.ToList();
      var clauses = forms.Select(VocabClause);
      return $"{Builtin.Note}:{NoteTypes.Vocab} ({string.Join(" OR ", clauses)})";
   }

   /// <summary>
   /// Search for vocab notes matching any of the given words (exact word match).
   /// Ported from vocabs_lookup_strings()
   /// </summary>
   public string VocabsLookupStrings(IEnumerable<string> words)
   {
      var wordList = words.ToList();
      var clauses = wordList.Select(word => FieldContainsWord(AnkiFieldNames.Vocab.Forms, word));
      return $"{Builtin.Note}:{NoteTypes.Vocab} ({string.Join(" OR ", clauses)})";
   }

   /// <summary>
   /// Search for vocab reading cards matching any of the given words.
   /// Ported from vocabs_lookup_strings_read_card()
   /// </summary>
   public string VocabsLookupStringsReadCard(IEnumerable<string> words) => $"{VocabsLookupStrings(words)} {Builtin.Card}:{AnkiFieldNames.VocabCard.Reading}";

   /// <summary>
   /// Search for kanji notes whose readings contain the given reading part.
   /// Ported from kanji_with_reading_part()
   /// </summary>
   public string KanjiWithReadingPart(string readingPart)
   {
      var hiraganaReading = KanaUtils.AnythingToHiragana(readingPart);
      return $"{Builtin.Note}:{NoteTypes.Kanji} ({AnkiFieldNames.Kanji.ReadingOn}:*{hiraganaReading}* OR {AnkiFieldNames.Kanji.ReadingKun}:*{hiraganaReading}*)";
   }

   /// <summary>
   /// Search for notes with exact question match or form match.
   /// Ported from exact_matches()
   /// </summary>
   public string ExactMatches(string question) => $"{AnkiFieldNames.Question}:\"{question}\" OR {FieldContainsWord(AnkiFieldNames.Vocab.Forms, question)}";

   /// <summary>
   /// Search for exact matches excluding sentence notes.
   /// Ported from exact_matches_no_sentences()
   /// </summary>
   string ExactMatchesNoSentences(string question) => $"({ExactMatches(question)}) -{Builtin.Note}:{NoteTypes.Sentence}";

   /// <summary>
   /// Search for exact matches on reading cards, excluding sentences and excluded decks.
   /// Ported from exact_matches_no_sentences_reading_cards()
   /// </summary>
   public string ExactMatchesNoSentencesReadingCards(string question) => $"({ExactMatchesNoSentences(question)}) {Builtin.Card}:{AnkiFieldNames.VocabCard.Reading} -{Builtin.Deck}:{ExcludedDeckSubstring}";

   /// <summary>
   /// Search for Immersion Kit sentence notes.
   /// Ported from immersion_kit_sentences()
   /// </summary>
   public string ImmersionKitSentences() => $"\"{Builtin.Note}:{NoteTypes.ImmersionKit}\"";

   /// <summary>
   /// Search for kanji notes containing all specified radicals.
   /// Ported from kanji_with_radicals_in_string()
   /// </summary>
   public string KanjiWithRadicalsInString(string search)
   {
      var radicals = search.Trim().Replace(",", "").Replace(" ", "").ToCharArray().ToHashSet();

      bool KanjiContainsAllRadicals(KanjiNote kanji)
      {
         var kanjiRadicals = kanji.Radicals.ToHashSet();
         return radicals.All(r => kanjiRadicals.Contains(r.ToString()));
      }

      var matchingKanji = radicals
                         .SelectMany(radical => _kanji.WithRadical(radical.ToString()))
                         .Distinct()
                         .Where(KanjiContainsAllRadicals)
                         .ToList();

      return NotesLookup(matchingKanji);
   }

   /// <summary>
   /// Search for kanji notes whose meaning contains the search string.
   /// Ported from kanji_with_meaning()
   /// </summary>
   public string KanjiWithMeaning(string search) => $"{Builtin.Note}:{NoteTypes.Kanji} ({AnkiFieldNames.Answer}:*{search}*)";

   /// <summary>
   /// Search for vocab dependencies (vocab and kanji that a word depends on).
   /// Ported from vocab_dependencies_lookup_query()
   /// </summary>
   public string VocabDependenciesLookupQuery(VocabNote vocab)
   {
      string CreateVocabClause(string text)
      {
         var dictionaryForms = TextAnalysis.FromText(_analysisServices, text).AllWordsStrings();
         if(!dictionaryForms.Any())
            return "";

         var clauses = dictionaryForms.Select(VocabClause);
         return $"({Builtin.Note}:{NoteTypes.Vocab} ({string.Join(" OR ", clauses)})) OR ";
      }

      string CreateKanjiClause()
      {
         var kanjiClauses = vocab.GetQuestion().Select(c => $"{AnkiFieldNames.Question}:{c}");
         return $"{Builtin.Note}:{NoteTypes.Kanji} ( {string.Join(" OR ", kanjiClauses)} )";
      }

      var vocabVocabClause = CreateVocabClause(vocab.GetQuestion());
      var kanjiClause = CreateKanjiClause();

      return $"{vocabVocabClause}({kanjiClause})";
   }
}
