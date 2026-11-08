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

   public VocabNote Create(string question, string answer, List<string> readings, Action<VocabNote>? initializer = null)
   {
      var note = new VocabNote(_noteServices);
      note.Question.SetText(question);
      note.SourceAnswer.Set(answer);
      note.SetReadings(readings);

      if(initializer != null)
      {
         initializer(note);
      }

      _collection.Vocab.Add(note);
      return note;
   }

   public VocabNote CreateFromUserData(string question, string answer, List<string> readings, Action<VocabNote>? initializer = null)
   {
      var note = Create(question, answer, readings, initializer);
      note.User.Answer.Set(note.SourceAnswer.Value);
      note.SourceAnswer.Empty();
      return note;
   }
}
