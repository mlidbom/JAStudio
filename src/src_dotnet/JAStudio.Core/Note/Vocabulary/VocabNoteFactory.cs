using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteFactory
{
   readonly DictLookup _dictLookup;
   readonly JPCollection _collection;
   readonly NoteServices _noteServices;

   internal VocabNoteFactory(DictLookup dictLookup, JPCollection collection, NoteServices noteServices)
   {
      _dictLookup = dictLookup;
      _collection = collection;
      _noteServices = noteServices;
   }

   public VocabNote CreateWithDictionary(string question)
   {
      var lookupResult = _dictLookup.LookupWord(question);
      if(!lookupResult.FoundWords())
      {
         return Create(question, "", []);
      }

      var created = Create(question, lookupResult.FormatAnswer(), lookupResult.Readings());
      created.Tags.Set(Tags.Source.Jamdict);
      created.UpdateGeneratedData();
      return created;
   }

   /// <summary>Creates a vocab whose answer, <paramref name="sourceAnswer"/>, is its source's, such as the dictionary's.</summary>
   public VocabNote Create(string question, string sourceAnswer, List<string> readings, Action<VocabNote>? initializer = null)
   {
      var note = new VocabNote(_noteServices);
      note.Question.SetText(question);
      note.SourceData.Answer.Set(sourceAnswer);
      note.SetReadings(readings);

      if(initializer != null)
      {
         initializer(note);
      }

      _collection.Vocab.Add(note);
      return note;
   }

   /// <summary>Creates a vocab whose answer, <paramref name="corpusAnswer"/>, is the corpus's own: a vocab that came from no source.</summary>
   public VocabNote CreateWithCorpusAnswer(string question, string corpusAnswer, List<string> readings, Action<VocabNote>? initializer = null)
   {
      var note = Create(question, "", readings, initializer);
      note.CorpusAnswer.Set(corpusAnswer);
      return note;
   }
}
