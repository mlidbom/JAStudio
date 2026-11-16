using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Compze.DependencyInjection.Abstractions;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.Studying;
using JAStudio.Core.UI.Shortcuts;
using JAStudio.Core.UI.Web;

// ReSharper disable InconsistentNaming

namespace JAStudio.Web.Specifications.FromAnki;

/// <summary>The requests the Anki add-on makes of JAStudio on its card server (<see cref="EndpointsForTheAnkiAddOn"/>), made as the add-on makes them.</summary>
public class When_the_Anki_add_on_asks_JAStudio : IAsyncDisposable
{
   readonly CollectionFactory.AppScope _appScope = CollectionFactory.InjectCollectionWithSelectData(DataNeeded.Kanji | DataNeeded.Vocabulary);
   readonly NoteLinkOpenerThatRecordsRequests _noteLinkOpener = new();
   readonly WorkspaceTogglesThatRecordRequests _workspaceToggles = new();
   readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false });
   readonly CardServer _server;
   readonly ITaggregateReference<VocabNote> _vocabReference;
   VocabNote _vocab => _vocabReference.Current;
   readonly ITaggregateReference<KanjiNote> _kanjiReference;
   KanjiNote _kanji => _kanjiReference.Current;
   static readonly string ANoteIdJAStudioHasNot = Guid.NewGuid().ToString();

   public When_the_Anki_add_on_asks_JAStudio()
   {
      _server = new CardServer(_appScope.CoreApp.Services.Resolve<IRootResolver>(), _noteLinkOpener, _workspaceToggles);
      _server.Start();
      _vocabReference = _appScope.CoreApp.Collection.Vocab.WithQuestion("入れる")[0].Reference();
      _kanjiReference = (_appScope.CoreApp.Collection.Kanji.WithKanji("病")!).Reference();
   }

   JapaneseConfig Config => _appScope.CoreApp.Config;
   T Resolve<T>() where T : class => _appScope.CoreApp.Services.Resolve<T>();

   HttpResponseMessage Get(string path) => _http.Send(new HttpRequestMessage(HttpMethod.Get, $"{_server.BaseUrl}{path}"));
   HttpResponseMessage Post(string path) => _http.Send(new HttpRequestMessage(HttpMethod.Post, $"{_server.BaseUrl}{path}"));
   static JsonElement TheJsonOf(HttpResponseMessage response) => JsonDocument.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult()).RootElement;

   public async ValueTask DisposeAsync()
   {
      _http.Dispose();
      await _server.StopAsync();
      _appScope.Dispose();
   }

   public class for_the_contract : When_the_Anki_add_on_asks_JAStudio
   {
      readonly HttpResponseMessage _answer;
      public for_the_contract() => _answer = Get("/anki/contract");

      [XF] public void it_answers_with_the_version_this_JAStudio_speaks() => TheJsonOf(_answer).GetProperty("version").GetInt32().Must().Be(4);
   }

   public class for_a_cards_page : When_the_Anki_add_on_asks_JAStudio
   {
      public class of_a_note_JAStudio_has : for_a_cards_page
      {
         readonly HttpResponseMessage _answer;
         public of_a_note_JAStudio_has() => _answer = Get($"/anki/card/front?noteId={_vocab.GetId()}&cardType={CardTypes.Listening}&displayContext=review");

         [XF] public void it_sends_the_iframe_on_to_the_cards_page() => _answer.StatusCode.Must().Be(HttpStatusCode.Redirect);
         [XF] public void which_is_the_page_of_that_side_of_that_card_as_Anki_shows_it() =>
            _answer.Headers.Location!.OriginalString.Must().Be(CardServerUrl.CardPagePath(_vocab, CardTypes.Listening, "front", "review"));
      }

      public class of_a_note_JAStudio_has_not : for_a_cards_page
      {
         readonly HttpResponseMessage _answer;
         public of_a_note_JAStudio_has_not() => _answer = Get($"/anki/card/back?noteId={ANoteIdJAStudioHasNot}&cardType={CardTypes.Reading}&displayContext=preview");

         [XF] public void it_answers_that_there_is_no_such_note() => _answer.StatusCode.Must().Be(HttpStatusCode.NotFound);
         [XF] public void saying_so_in_words_that_the_iframe_shows() => _answer.Content.ReadAsStringAsync().GetAwaiter().GetResult().Must().Contain(ANoteIdJAStudioHasNot);
      }
   }

   public class to_toggle_the_workspace_window : When_the_Anki_add_on_asks_JAStudio
   {
      readonly HttpResponseMessage _answer;
      public to_toggle_the_workspace_window() => _answer = Post("/anki/workspace/toggle");

      [XF] public void it_answers_that_it_did() => _answer.StatusCode.Must().Be(HttpStatusCode.NoContent);
      [XF] public void the_workspace_window_is_toggled_once() => _workspaceToggles.TimesTheWorkspaceWindowWasToggled.Must().Be(1);
      [XF] public void the_note_search_is_left_alone() => _workspaceToggles.TimesTheNoteSearchWasToggled.Must().Be(0);
   }

   public class to_toggle_the_note_search : When_the_Anki_add_on_asks_JAStudio
   {
      public to_toggle_the_note_search() => Post("/anki/note-search/toggle").EnsureSuccessStatusCode();

      [XF] public void the_note_search_is_toggled_once() => _workspaceToggles.TimesTheNoteSearchWasToggled.Must().Be(1);
      [XF] public void the_workspace_window_is_left_alone() => _workspaceToggles.TimesTheWorkspaceWindowWasToggled.Must().Be(0);
   }

   public class to_show_a_note_in_the_workspace : When_the_Anki_add_on_asks_JAStudio
   {
      public class which_JAStudio_has : to_show_a_note_in_the_workspace
      {
         public which_JAStudio_has() => Post($"/anki/workspace/show-note?noteId={_vocab.GetId()}").EnsureSuccessStatusCode();

         [XF] public void that_note_is_opened_in_the_workspace() => _noteLinkOpener.NotesOpenedInWorkspace.Must().SequenceEqual([_vocab.GetId()]);
      }

      public class which_JAStudio_has_not : to_show_a_note_in_the_workspace
      {
         readonly HttpResponseMessage _answer;
         public which_JAStudio_has_not() => _answer = Post($"/anki/workspace/show-note?noteId={ANoteIdJAStudioHasNot}");

         [XF] public void it_answers_that_there_is_no_such_note() => _answer.StatusCode.Must().Be(HttpStatusCode.NotFound);
         [XF] public void saying_why() => TheJsonOf(_answer).GetProperty("error").GetString().Must().Contain(ANoteIdJAStudioHasNot);
         [XF] public void nothing_is_opened() => _noteLinkOpener.NotesOpenedInWorkspace.Must().BeEmpty();
      }
   }

   public class for_the_keys_of_the_note_view_shortcuts : When_the_Anki_add_on_asks_JAStudio
   {
      readonly string[] _keys;
      public for_the_keys_of_the_note_view_shortcuts() => _keys = TheJsonOf(Get("/anki/note-view-shortcuts")).GetProperty("keys").EnumerateArray().Select(it => it.GetString()!).ToArray();

      [XF] public void it_answers_with_the_key_of_each_shortcut() => _keys.Must().SequenceEqual(Resolve<NoteViewShortcuts>().All.Select(it => it.KeyGesture));
   }

   public class to_run_a_note_view_shortcut : When_the_Anki_add_on_asks_JAStudio
   {
      public class that_acts_on_the_note_Anki_shows : to_run_a_note_view_shortcut
      {
         public that_acts_on_the_note_Anki_shows()
         {
            _vocab.Edit(it => it.Mnemonic.Set("a mnemonic to remove"));
            Post($"/anki/note-view-shortcuts/run?key=0&noteId={_vocab.GetId()}").EnsureSuccessStatusCode();
         }

         [XF] public void it_acts_on_that_note() => _vocab.Mnemonic.Value.Must().Be("");
      }

      public class that_toggles_a_setting_while_Anki_shows_no_note : to_run_a_note_view_shortcut
      {
         readonly bool _settingBefore;

         public that_toggles_a_setting_while_Anki_shows_no_note()
         {
            _settingBefore = Config.ShowKanjiInSentenceBreakdown.Value;
            Post("/anki/note-view-shortcuts/run?key=a").EnsureSuccessStatusCode();
         }

         [XF] public void the_setting_is_toggled() => Config.ShowKanjiInSentenceBreakdown.Value.Must().Be(!_settingBefore);
      }

      public class bound_to_no_key : to_run_a_note_view_shortcut
      {
         readonly HttpResponseMessage _answer;
         public bound_to_no_key() => _answer = Post("/anki/note-view-shortcuts/run?key=Ctrl%2Bz");

         [XF] public void it_answers_that_there_is_no_such_shortcut() => _answer.StatusCode.Must().Be(HttpStatusCode.NotFound);
         [XF] public void saying_why() => TheJsonOf(_answer).GetProperty("error").GetString().Must().Contain("Ctrl+z");
      }
   }

   public class for_how_long_to_show_the_question : When_the_Anki_add_on_asks_JAStudio
   {
      double SecondsAnswered(JPNote note, double secondsTheDeckGives, int againAnswersInARowToday) =>
         TheJsonOf(Get($"/anki/seconds-to-show-the-question?noteId={note.GetId()}&secondsTheDeckGives={secondsTheDeckGives.ToString(CultureInfo.InvariantCulture)}&againAnswersInARowToday={againAnswersInARowToday}"))
           .GetProperty("seconds").GetDouble();

      [XF] public void of_a_vocab_it_answers_with_the_time_JAStudio_gives_its_question() =>
         SecondsAnswered(_vocab, secondsTheDeckGives: 100, againAnswersInARowToday: 0).Must().Be(Resolve<HowLongToShowTheQuestion>().Seconds(_vocab, 100, 0));

      [XF] public void of_a_kanji_it_answers_with_the_time_the_deck_gives() => SecondsAnswered(_kanji, secondsTheDeckGives: 5, againAnswersInARowToday: 0).Must().Be(5.0);

      [XF] public void of_a_card_failed_twice_in_a_row_today_it_answers_with_more_time_for_each_failure() =>
         SecondsAnswered(_kanji, secondsTheDeckGives: 4, againAnswersInARowToday: 2).Must().Be(4 * Math.Pow(Config.BoostFailedCardAllowedTimeByFactor.Value, 2));

      [XF] public void of_a_note_JAStudio_has_not_it_answers_that_there_is_no_such_note() =>
         Get($"/anki/seconds-to-show-the-question?noteId={ANoteIdJAStudioHasNot}&secondsTheDeckGives=5&againAnswersInARowToday=0").StatusCode.Must().Be(HttpStatusCode.NotFound);
   }

   public class for_the_length_of_a_timebox : When_the_Anki_add_on_asks_JAStudio
   {
      JsonElement MinutesAnswered(string noteType, string cardType) =>
         TheJsonOf(Get($"/anki/timebox-minutes?noteType={Uri.EscapeDataString(noteType)}&cardType={cardType}")).GetProperty("minutes");

      [XF] public void of_vocab_reading_cards_it_answers_with_the_setting_for_them() =>
         MinutesAnswered(NoteTypes.Vocab, CardTypes.Reading).GetInt64().Must().Be(Config.TimeboxVocabRead.Value);

      [XF] public void of_cards_JAStudio_has_no_timebox_for_it_answers_with_none() =>
         MinutesAnswered("Basic", CardTypes.Reading).ValueKind.Must().Be(JsonValueKind.Null);
   }
}
