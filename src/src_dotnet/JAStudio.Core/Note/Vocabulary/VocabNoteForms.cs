using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Compze.Internals.SystemCE;
using JAStudio.Core.Note.CorpusData;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// The forms of a <see cref="VocabNote"/>: the texts that, when found in a sentence, are this vocab.<br/>
/// The vocab's question is always one of them. The others are its entered forms, exactly as the user typed them and as they are stored.<br/>
/// The matched forms are the question and the entered forms plus everything they expand to.
/// </summary>
/// <remarks>
/// The question is not stored among the entered forms. It's a form by definition, so a stored copy would only be a second place<br/>
/// that must be kept in step with it. Files written before this was so have the question among their forms; it's dropped when they're loaded.
/// </remarks>
/// <remarks>
/// The matched forms differ from the question and the entered forms only for a vocab that requires the dictionary form start,<br/>
/// where a form starting with the <see cref="DictionaryFormEndingPlaceholder"/> expands to one matched form per dictionary form ending.<br/>
/// Showing, editing, storing and copying forms uses the entered forms; looking forms up in text uses the matched forms.
/// </remarks>
public class VocabNoteForms
{
   readonly VocabNote _vocab;
   readonly NoteGuard _guard;
   List<string> _enteredFormsWithBrackets;
   LazyCE<List<string>> _enteredFormsList;
   LazyCE<HashSet<string>> _enteredFormsSet;
   LazyCE<List<string>> _questionAndEnteredFormsList;
   LazyCE<List<string>> _matchedFormsList;
   LazyCE<HashSet<string>> _matchedFormsSet;
   LazyCE<HashSet<string>> _ownedForms;
   LazyCE<HashSet<string>> _notOwnedByOtherVocab;

   public VocabNoteForms(VocabNote vocab, VocabData? data, NoteGuard guard)
   {
      _vocab = vocab;
      _guard = guard;
      _enteredFormsWithBrackets = WithoutTheQuestion(data?.Forms ?? []);
      InvalidateCaches();
   }

   /// <summary>Replaces every cached value derived from the forms with one computed afresh on first use.</summary>
   [MemberNotNull(nameof(_enteredFormsList), nameof(_enteredFormsSet), nameof(_questionAndEnteredFormsList), nameof(_matchedFormsList), nameof(_matchedFormsSet), nameof(_ownedForms), nameof(_notOwnedByOtherVocab))]
   void InvalidateCaches()
   {
      _enteredFormsList = new LazyCE<List<string>>(() => [.. _enteredFormsWithBrackets.Select(StripBrackets)]);
      _enteredFormsSet = new LazyCE<HashSet<string>>(() => [.. _enteredFormsList.Value]);
      _questionAndEnteredFormsList = new LazyCE<List<string>>(() => [_vocab.GetQuestion(), .. _enteredFormsList.Value]);
      _matchedFormsList = new LazyCE<List<string>>(ComputeMatchedForms);
      _matchedFormsSet = new LazyCE<HashSet<string>>(() => [.. _matchedFormsList.Value]);
      _ownedForms = new LazyCE<HashSet<string>>(ComputeOwnedForms);
      _notOwnedByOtherVocab = new LazyCE<HashSet<string>>(ComputeNotOwnedByOtherVocab);
   }

   List<string> ComputeMatchedForms()
   {
      if(!ExpandsDictionaryFormEndingPlaceholders) return _questionAndEnteredFormsList.Value;

      return
      [
         .. _questionAndEnteredFormsList.Value
                                        .SelectMany(DictionaryFormEndingPlaceholder.FormAndItsExpansions)
                                        .Distinct()
      ];
   }

   /// <summary><paramref name="formsWithBrackets"/> without the question, bracketed or not: the question is a form by definition, so it's never stored as one.</summary>
   List<string> WithoutTheQuestion(IEnumerable<string> formsWithBrackets) => [.. formsWithBrackets.Where(form => !IsTheQuestion(form))];

   bool IsTheQuestion(string formWithBrackets) => StripBrackets(formWithBrackets) == _vocab.GetQuestion();

   bool ExpandsDictionaryFormEndingPlaceholders => _vocab.MatchingConfiguration.RequiresForbids.DictionaryFormStart.IsRequired;

   HashSet<string> ComputeOwnedForms()
   {
      var questionAndBracketedForms = _enteredFormsWithBrackets
                                     .Where(IsBracketed)
                                     .Select(StripBrackets)
                                     .Prepend(_vocab.GetQuestion());

      return ExpandsDictionaryFormEndingPlaceholders
                ? [..questionAndBracketedForms.SelectMany(DictionaryFormEndingPlaceholder.FormAndItsExpansions)]
                : [..questionAndBracketedForms];

      static bool IsBracketed(string form) => form.StartsWith('[');
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

   /// <summary>Whether <paramref name="form"/> is one of the <see cref="OwnedForms"/>.</summary>
   public bool IsOwnedForm(string form) => _ownedForms.Value.Contains(form);

   /// <summary>
   /// The forms this vocab claims as its own: its question and its bracketed entered forms, plus, for a vocab that requires<br/>
   /// the dictionary form start, what those of them starting with the <see cref="DictionaryFormEndingPlaceholder"/> expand to.
   /// </summary>
   /// <remarks>
   /// Owning a form makes this vocab win over the vocab that match the same text without owning it.<br/>
   /// And when an owning vocab is among the matches for a text, only the vocab matches decide whether the text is a word there:<br/>
   /// the dictionary is not used as a fallback even if none of them is valid.
   /// </remarks>
   public HashSet<string> OwnedForms() => _ownedForms.Value;

   /// <summary>The forms besides the question, exactly as the user entered them, with the ownership brackets of owned forms removed.</summary>
   public List<string> EnteredFormsList() => _enteredFormsList.Value;

   /// <inheritdoc cref="EnteredFormsList"/>
   public HashSet<string> EnteredFormsSet() => _enteredFormsSet.Value;

   /// <summary>The forms besides the question, exactly as the user entered them, ownership brackets included, joined with commas for editing.</summary>
   public string EnteredFormsWithBracketsString() => string.Join(", ", _enteredFormsWithBrackets);

   /// <summary>The forms besides the question, exactly as the user entered them, ownership brackets included: <c>[form]</c> stays as it is.</summary>
   public List<string> EnteredFormsWithBrackets() => [.. _enteredFormsWithBrackets];

   /// <summary>All the vocab's forms as entered: its question first, then the <see cref="EnteredFormsList"/>.</summary>
   public List<string> QuestionAndEnteredFormsList() => _questionAndEnteredFormsList.Value;

   /// <summary>
   /// The forms this vocab matches in text: the <see cref="QuestionAndEnteredFormsList"/>, plus, for a vocab that requires the dictionary form start,<br/>
   /// what each of them starting with the <see cref="DictionaryFormEndingPlaceholder"/> expands to.
   /// </summary>
   public List<string> MatchedFormsList() => _matchedFormsList.Value;

   /// <inheritdoc cref="MatchedFormsList"/>
   public HashSet<string> MatchedFormsSet() => _matchedFormsSet.Value;

   /// <summary>The <see cref="MatchedFormsList"/> with the vocab prefix/suffix marker removed.</summary>
   public List<string> MatchedFormsWithoutNoiseCharacters() => [.. MatchedFormsList().Select(StripNoiseCharacters)];

   /// <summary>The vocab notes whose question is one of the <see cref="QuestionAndEnteredFormsList"/>, so this vocab among them.</summary>
   public List<VocabNote> VocabForQuestionAndEnteredForms()
   {
      return
      [
         .. _questionAndEnteredFormsList.Value
                                        .SelectMany(form => _vocab.Services.Collection.Vocab.Cache.WithQuestion(form))
      ];
   }

   /// <summary>The <see cref="VocabForQuestionAndEnteredForms"/>, the ones found in the most sentences first.</summary>
   public List<VocabNote> VocabForQuestionAndEnteredFormsBySentenceCount()
   {
      return
      [
         .. VocabForQuestionAndEnteredForms()
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

   /// <summary>Replaces the entered forms with <paramref name="forms"/>, dropping duplicates and the question, which is a form anyway.</summary>
   public void SetEnteredForms(IEnumerable<string> forms) => _guard.Update(() =>
   {
      _enteredFormsWithBrackets = WithoutTheQuestion(forms.Distinct());
      InvalidateCaches();
   });

   /// <summary>
   /// Called by <see cref="VocabNoteQuestion.Set"/>: drops the new question from the entered forms, where it would now be a copy,<br/>
   /// and discards everything derived from the old question.
   /// </summary>
   internal void OnQuestionChanged()
   {
      _enteredFormsWithBrackets = WithoutTheQuestion(_enteredFormsWithBrackets);
      InvalidateCaches();
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
   /// of every vocab whose question is <paramref name="add"/>. Adding the question, which is always a form, changes nothing.
   /// </summary>
   public void Add(string add)
   {
      if(IsTheQuestion(add)) return;

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
   /// The <see cref="MatchedFormsList"/> and the <see cref="OwnedForms"/> depend on whether the vocab requires the dictionary form start.
   /// </summary>
   internal void OnTagsUpdated() => InvalidateCaches();

   public override string ToString() => string.Join(", ", _enteredFormsWithBrackets);
}
