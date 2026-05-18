using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

public sealed class CandidateWordVariant
{
   readonly LazyCE<DictLookupResult> _dictLookup;
   bool _completedValidityAnalysis;
   bool _isValid;
   IEnumerable<Match> _validMatches;
   List<Match> _displayMatches;

   public CandidateWord Word { get; }
   public string Form { get; }
   public List<VocabMatch> VocabMatches { get; }
   public List<Match> Matches { get; private set; }

   public CandidateWordVariant(CandidateWord word, string form)
   {
      Word = word;
      Form = form;

      var services = word.Analysis.Services;
      _dictLookup = new LazyCE<DictLookupResult>(() => services.DictLookup.LookupWord(form));
      VocabMatches = services.Vocab.WithForm(form)
                             .Select(vocab => new VocabMatch(this, vocab))
                             .ToList();

      Matches = [];
      _validMatches = [];
      _displayMatches = [];
   }

   public bool IsSurface => Form == Word.SurfaceForm;

   public bool VocabsControlMatchStatus =>
      FormOwningVocabMatches.Any() ||
      (VocabMatches.Any() && !_dictLookup.Value.FoundWords() && Word.IsCompound);

   public void RunValidityAnalysis()
   {
      JAAssert.That(!_completedValidityAnalysis);

      if(VocabMatches.Any())
      {
         Matches = VocabMatches.Cast<Match>().ToList();
         ValidVocabMatches = VocabMatches.Where(vm => vm.IsValid).ToList();
      }

      if(ValidVocabMatches.Any() || VocabsControlMatchStatus)
      {
         _validMatches = ValidVocabMatches;
      } else
      {
         if(_dictLookup.Value.FoundWords())
         {
            Matches.Add(new DictionaryMatch(this, _dictLookup.Value.Entries[0]));
         } else if(!Word.IsCompound)
         {
            Matches.Add(new MissingMatch(this));
         }

         _validMatches = Matches.Where(match => match.IsValid);
      }

      _isValid = _validMatches.Any();
      _completedValidityAnalysis = true;
   }

   public void RunVisibilityAnalysis()
   {
      _displayMatches = OnceValidityAnalyzed.Matches.Where(match => match.IsDisplayed).ToList();
   }

   public int StartIndex => Word.StartLocation.CharacterStartIndex;
   public SentenceConfiguration Configuration => Word.Analysis.Configuration;
   public bool IsKnownWord => VocabMatches.Count > 0 || _dictLookup.Value.FoundWords();

   List<VocabMatch> FormOwningVocabMatches =>
      VocabMatches.Where(vm => vm.Vocab.Forms.IsOwnedForm(Form)).ToList();

   public bool HasValidMatch => OnceValidityAnalyzed._isValid;
   public IEnumerable<Match> ValidMatches => OnceValidityAnalyzed._validMatches;
   public List<Match> DisplayMatches => OnceVisibilityAnalyzed._displayMatches;

   List<VocabMatch> ValidVocabMatches { get; set; } = [];

   CandidateWordVariant OnceValidityAnalyzed
   {
      get
      {
         if(!_completedValidityAnalysis)
            throw new System.Exception("Analysis not completed yet");
         return this;
      }
   }

   CandidateWordVariant OnceVisibilityAnalyzed
   {
      get
      {
         if(!_completedValidityAnalysis)
            throw new System.Exception("Analysis not completed yet");
         return this;
      }
   }

   public override string ToString() => $"{Form}, is_valid_candidate:{HasValidMatch}";
}
