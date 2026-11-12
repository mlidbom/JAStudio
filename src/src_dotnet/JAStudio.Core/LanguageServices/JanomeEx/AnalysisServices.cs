using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.LanguageServices.JanomeEx;

/// <summary>
/// Bundles the services needed by the text analysis pipeline (tokenizing, word extraction, match evaluation).
/// Threaded through TextAnalysis → TextLocation → CandidateWord → CandidateWordVariant → Match state tests.
/// </summary>
public class AnalysisServices
{
   public VocabCollection Vocab { get; }
   public DictLookup DictLookup { get; }
   public Settings Settings { get; }

   public AnalysisServices(VocabCollection vocab, DictLookup dictLookup, Settings settings)
   {
      Vocab = vocab;
      DictLookup = dictLookup;
      Settings = settings;
   }

   /// <summary>
   /// Whether a vocab form or a word form of the dictionary starts with <paramref name="text"/>, or is it: those are the forms a candidate word<br/>
   /// can match, so a text no such form starts with can't grow into a word that matches anything.
   /// </summary>
   public bool AVocabOrDictionaryWordFormStartsWith(string text) => Vocab.HasAFormStartingWith(text) || DictLookup.HasAWordFormStartingWith(text);
}
