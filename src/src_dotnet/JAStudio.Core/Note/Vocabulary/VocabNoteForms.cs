using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE;
using JAStudio.Core.Note.CorpusData;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// The forms of a <see cref="VocabNote"/>: the texts that, when found in a sentence, are this vocab.<br/>
/// They come as two sets: the entered forms, exactly as the user typed them and as they are stored,<br/>
/// and the matched forms, which are the entered forms plus everything they expand to.
/// </summary>
/// <remarks>
/// The two sets differ only for a vocab that requires the dictionary form start, where an entered form starting with the<br/>
/// <see cref="DictionaryFormEndingPlaceholder"/> expands to one matched form per dictionary form ending.<br/>
/// Showing, editing, storing and copying forms uses the entered forms; looking forms up in text uses the matched forms.
/// </remarks>
public class VocabNoteForms
{
   readonly VocabNote _vocab;
   readonly NoteGuard _guard;
   List<string> _enteredFormsWithBrackets;
   readonly LazyCE<List<string>> _enteredFormsList;
   readonly LazyCE<HashSet<string>> _enteredFormsSet;
   readonly LazyCE<List<string>> _matchedFormsList;
   readonly LazyCE<HashSet<string>> _matchedFormsSet;
   readonly LazyCE<HashSet<string>> _ownedForms;
   readonly LazyCE<HashSet<string>> _notOwnedByOtherVocab;

   public VocabNoteForms(VocabNote vocab, VocabData? data, NoteGuard guard)
   {
      _vocab = vocab;
      _guard = guard;
      _enteredFormsWithBrackets = data?.Forms != null ? [..data.Forms] : [];
      _enteredFormsList = new LazyCE<List<string>>(() => [.. _enteredFormsWithBrackets.Select(StripBrackets)]);
      _enteredFormsSet = new LazyCE<HashSet<string>>(() => [.. _enteredFormsList.Value]);
      _matchedFormsList = new LazyCE<List<string>>(ComputeMatchedForms);
      _matchedFormsSet = new LazyCE<HashSet<string>>(() => [.. _matchedFormsList.Value]);
      _ownedForms = new LazyCE<HashSet<string>>(ComputeOwnedForms);
      _notOwnedByOtherVocab = new LazyCE<HashSet<string>>(ComputeNotOwnedByOtherVocab);
   }

   void InvalidateCaches()
   {
      _enteredFormsList.Reset();
      _enteredFormsSet.Reset();
      _matchedFormsList.Reset();
      _matchedFormsSet.Reset();
      _ownedForms.Reset();
      _notOwnedByOtherVocab.Reset();
   }

   List<string> ComputeMatchedForms()
   {
      if(!ExpandsDictionaryFormEndingPlaceholders) return _enteredFormsList.Value;

      return
      [
         .. _enteredFormsList.Value
                             .SelectMany(DictionaryFormEndingPlaceholder.FormAndItsExpansions)
                             .Distinct()
      ];
   }

   bool ExpandsDictionaryFormEndingPlaceholders => _vocab.MatchingConfiguration.RequiresForbids.DictionaryFormStart.IsRequired;

   HashSet<string> ComputeOwnedForms()
   {
      var owned = new HashSet<string> { _vocab.GetQuestion() };
      foreach(var form in _enteredFormsWithBrackets)
      {
         if(form.StartsWith('['))
         {
            owned.Add(StripBrackets(form));
         }
      }

      return owned;
   }

   HashSet<string> ComputeNotOwnedByOtherVocab()
   {
      return
      [
         .. MatchedFormsSet()
           .Where(IsNotOwnedByOtherFormNote)
      ];

      bool IsNotOwnedByOtherFormNote(string form)
      {
         return !_vocab.Services.Collection.Vocab.Cache.WithQuestion(form)
                       .Any(formOwningVocab =>
                               !Equals(formOwningVocab, _vocab) &&
                               formOwningVocab.Forms.MatchedFormsSet().Contains(_vocab.GetQuestion()));
      }
   }

   public bool IsOwnedForm(string form) => _ownedForms.Value.Contains(form);

   public HashSet<string> OwnedForms() => _ownedForms.Value;

   /// <summary>The forms exactly as the user entered them, with the ownership brackets of owned forms removed.</summary>
   public List<string> EnteredFormsList() => _enteredFormsList.Value;

   /// <inheritdoc cref="EnteredFormsList"/>
   public HashSet<string> EnteredFormsSet() => _enteredFormsSet.Value;

   /// <summary>The forms exactly as the user entered them, ownership brackets included, joined with commas for editing.</summary>
   public string EnteredFormsWithBracketsString() => string.Join(", ", _enteredFormsWithBrackets);

   /// <summary>The forms exactly as the user entered them, ownership brackets included: <c>[form]</c> stays as it is.</summary>
   public List<string> EnteredFormsWithBrackets() => [.. _enteredFormsWithBrackets];

   /// <summary>
   /// The forms this vocab matches in text: the <see cref="EnteredFormsList"/>, plus, for a vocab that requires the dictionary form start,<br/>
   /// what each entered form starting with the <see cref="DictionaryFormEndingPlaceholder"/> expands to.
   /// </summary>
   public List<string> MatchedFormsList() => _matchedFormsList.Value;

   /// <inheritdoc cref="MatchedFormsList"/>
   public HashSet<string> MatchedFormsSet() => _matchedFormsSet.Value;

   /// <summary>The <see cref="MatchedFormsList"/> with the vocab prefix/suffix marker removed.</summary>
   public List<string> MatchedFormsWithoutNoiseCharacters() => [.. MatchedFormsList().Select(StripNoiseCharacters)];

   /// <summary>The vocab notes whose question is one of the <see cref="EnteredFormsList"/>.</summary>
   public List<VocabNote> VocabForEnteredForms()
   {
      return
      [
         .. _enteredFormsList.Value
                             .SelectMany(form => _vocab.Services.Collection.Vocab.Cache.WithQuestion(form))
      ];
   }

   /// <summary>The <see cref="VocabForEnteredForms"/>, the ones found in the most sentences first.</summary>
   public List<VocabNote> VocabForEnteredFormsBySentenceCount()
   {
      return
      [
         .. VocabForEnteredForms()
           .OrderByDescending(vocab => vocab.Sentences.Counts().Total)
      ];
   }

   /// <summary>
   /// The <see cref="MatchedFormsSet"/> except those that are the question of another vocab<br/>
   /// which in turn has this vocab's question among its own matched forms.
   /// </summary>
   public HashSet<string> NotOwnedByOtherVocab() => _notOwnedByOtherVocab.Value;

   static string StripNoiseCharacters(string input) => input.Replace(Mine.VocabPrefixSuffixMarker, "");

   static string StripBrackets(string input) => input.Replace("[", "").Replace("]", "");

   /// <summary>Replaces the entered forms with <paramref name="forms"/>, dropping duplicates.</summary>
   public void SetEnteredForms(IEnumerable<string> forms) => _guard.Update(() =>
   {
      _enteredFormsWithBrackets = [.. forms.Distinct()];
      InvalidateCaches();
   });

   /// <summary>
   /// Adds <paramref name="form"/> to the entered forms unless this vocab already has it, bracketed (owned) or not.<br/>
   /// All existing forms are kept exactly as stored, so their ownership brackets survive.
   /// </summary>
   /// <remarks>
   /// Unlike <see cref="Add"/>, this does not touch other vocab notes' forms.
   /// </remarks>
   public void EnsureContains(string form)
   {
      if(EnteredFormsSet().Contains(form)) return;

      _guard.Update(() =>
      {
         _enteredFormsWithBrackets.Add(form);
         InvalidateCaches();
      });
   }

   /// <summary>
   /// Removes <paramref name="remove"/> from the entered forms, and removes this vocab's question from the entered forms<br/>
   /// of every vocab whose question is <paramref name="remove"/>.
   /// </summary>
   public void Remove(string remove)
   {
      _guard.Update(() =>
      {
         _enteredFormsWithBrackets = [.. _enteredFormsWithBrackets.Where(item => item != remove)];
         InvalidateCaches();
      });

      var removeNotes = _vocab.Services.Collection.Vocab.Cache.WithQuestion(remove)
                              .Where(voc => voc.Forms.EnteredFormsSet().Contains(_vocab.GetQuestion()))
                              .ToList();

      foreach(var removeNote in removeNotes)
      {
         removeNote.Forms.Remove(_vocab.GetQuestion());
      }
   }

   /// <summary>
   /// Adds <paramref name="add"/> to the entered forms, and adds this vocab's question to the entered forms<br/>
   /// of every vocab whose question is <paramref name="add"/>.
   /// </summary>
   public void Add(string add)
   {
      _guard.Update(() =>
      {
         if(!_enteredFormsWithBrackets.Contains(add))
         {
            _enteredFormsWithBrackets.Add(add);
         }

         InvalidateCaches();
      });

      var addNotes = _vocab.Services.Collection.Vocab.Cache.WithQuestion(add)
                           .Where(voc => !voc.Forms.EnteredFormsSet().Contains(_vocab.GetQuestion()))
                           .ToList();

      foreach(var addNote in addNotes)
      {
         addNote.Forms.Add(_vocab.GetQuestion());
      }
   }

   /// <summary>
   /// Discards everything derived from the vocab's tags.<br/>
   /// The <see cref="MatchedFormsList"/> depend on whether the vocab requires the dictionary form start.
   /// </summary>
   internal void OnTagsUpdated() => InvalidateCaches();

   public override string ToString() => string.Join(", ", _enteredFormsWithBrackets);
}
