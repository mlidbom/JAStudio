using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE;
using JAStudio.Core.Note.CorpusData;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteForms
{
   readonly VocabNote _vocab;
   readonly NoteGuard _guard;
   List<string> _rawParts;
   readonly LazyCE<HashSet<string>> _allRawSet;
   readonly LazyCE<List<string>> _allList;
   readonly LazyCE<HashSet<string>> _allSet;
   readonly LazyCE<HashSet<string>> _ownedForms;
   readonly LazyCE<HashSet<string>> _notOwnedByOtherVocab;

   public VocabNoteForms(VocabNote vocab, VocabData? data, NoteGuard guard)
   {
      _vocab = vocab;
      _guard = guard;
      _rawParts = data?.Forms != null ? [..data.Forms] : [];
      _allRawSet = new LazyCE<HashSet<string>>(() => _rawParts.ToHashSet());
      _allList = new LazyCE<List<string>>(() => _rawParts.Select(StripBrackets).ToList());
      _allSet = new LazyCE<HashSet<string>>(() => _allList.Value.ToHashSet());
      _ownedForms = new LazyCE<HashSet<string>>(ComputeOwnedForms);
      _notOwnedByOtherVocab = new LazyCE<HashSet<string>>(ComputeNotOwnedByOtherVocab);
   }

   void InvalidateCaches()
   {
      _allRawSet.Reset();
      _allList.Reset();
      _allSet.Reset();
      _ownedForms.Reset();
      _notOwnedByOtherVocab.Reset();
   }

   HashSet<string> ComputeOwnedForms()
   {
      var owned = new HashSet<string> { _vocab.GetQuestion() };
      foreach(var form in _allRawSet.Value)
      {
         if(form.StartsWith("["))
         {
            owned.Add(StripBrackets(form));
         }
      }

      return owned;
   }

   HashSet<string> ComputeNotOwnedByOtherVocab()
   {
      bool IsNotOwnedByOtherFormNote(string form)
      {
         return !_vocab.Services.Collection.Vocab.Cache.WithQuestion(form)
                       .Any(formOwningVocab =>
                               !Equals(formOwningVocab, _vocab) &&
                               formOwningVocab.Forms.AllSet().Contains(_vocab.GetQuestion()));
      }

      return _vocab.Forms.AllSet()
                   .Where(IsNotOwnedByOtherFormNote)
                   .ToHashSet();
   }

   public bool IsOwnedForm(string form) => _ownedForms.Value.Contains(form);

   public HashSet<string> OwnedForms() => _ownedForms.Value;

   public List<string> AllList() => _allList.Value;
   public HashSet<string> AllSet() => _allSet.Value;
   public string AllRawString() => string.Join(", ", _rawParts);

   /// Returns all forms with brackets preserved (e.g. "[form]" stays as-is).
   public List<string> AllRawList() => _rawParts.ToList();

   public List<VocabNote> AllListNotes()
   {
      return _allList.Value
                     .SelectMany(form => _vocab.Services.Collection.Vocab.Cache.WithQuestion(form))
                     .ToList();
   }

   public List<VocabNote> AllListNotesBySentenceCount()
   {
      return AllListNotes()
            .OrderByDescending(vocab => vocab.Sentences.Counts().Total)
            .ToList();
   }

   public HashSet<string> NotOwnedByOtherVocab() => _notOwnedByOtherVocab.Value;

   public List<string> WithoutNoiseCharacters() => AllList().Select(StripNoiseCharacters).ToList();

   static string StripNoiseCharacters(string input) => input.Replace(Mine.VocabPrefixSuffixMarker, "");

   static string StripBrackets(string input) => input.Replace("[", "").Replace("]", "");

   public void SetSet(HashSet<string> forms)
   {
      SetList(forms.ToList());
   }

   public void SetList(List<string> forms) => _guard.Update(() =>
   {
      _rawParts = forms.Distinct().ToList();
      InvalidateCaches();
   });

   public void Remove(string remove)
   {
      _guard.Update(() =>
      {
         _rawParts = _rawParts.Where(item => item != remove).ToList();
         InvalidateCaches();
      });

      // Also remove from notes that have this vocab's question as a form
      var removeNotes = _vocab.Services.Collection.Vocab.Cache.WithQuestion(remove)
                              .Where(voc => voc.Forms.AllSet().Contains(_vocab.GetQuestion()))
                              .ToList();

      foreach(var removeNote in removeNotes)
      {
         removeNote.Forms.Remove(_vocab.GetQuestion());
      }
   }

   public void Add(string add)
   {
      _guard.Update(() =>
      {
         if(!_rawParts.Contains(add))
         {
            _rawParts.Add(add);
         }

         InvalidateCaches();
      });

      // Also add to notes that reference this form
      var addNotes = _vocab.Services.Collection.Vocab.Cache.WithQuestion(add)
                           .Where(voc => !voc.Forms.AllSet().Contains(_vocab.GetQuestion()))
                           .ToList();

      foreach(var addNote in addNotes)
      {
         addNote.Forms.Add(_vocab.GetQuestion());
      }
   }

   public override string ToString() => string.Join(", ", _rawParts);
}
