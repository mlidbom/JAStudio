using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.UI.Web;

namespace JAStudio.Core.Specifications.UI.Web;

public class When_reading_which_note_a_card_page_shows : SpecificationStartingWithAnEmptyCollection
{
   static readonly Uri CardServerBaseUrl = new("http://localhost:5000");

   public class from_the_url_of_a_note_page : When_reading_which_note_a_card_page_shows
   {
      readonly VocabNote _vocab;
      readonly NoteId? _noteIdOfThePage;

      public from_the_url_of_a_note_page()
      {
         _vocab = CreateVocab("入れる", "to put in", "いれる");
         _noteIdOfThePage = CardServerUrl.NoteIdOfPage(new Uri(CardServerBaseUrl, CardServerUrl.NotePagePath(_vocab)));
      }

      [XF] public void it_is_the_note_the_page_was_made_for() => _noteIdOfThePage.Must().Be(_vocab.GetId());
   }

   public class from_the_url_of_a_page_that_shows_no_note : When_reading_which_note_a_card_page_shows
   {
      [XF] public void there_is_none() => CardServerUrl.NoteIdOfPage(CardServerBaseUrl).Must().BeNull();
   }
}
