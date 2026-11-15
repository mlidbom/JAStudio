using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Compze.DependencyInjection.Abstractions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki.ThePrimaryJAStudio;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.Studying;
using JAStudio.Core.UI.Shortcuts;
using JAStudio.Core.UI.Web;

// ReSharper disable InconsistentNaming

namespace JAStudio.Web.Specifications.FromAnki;

/// <summary>
/// The requests the Anki add-on makes of Anki's primary JAStudio, made through the add-on's own client (Python's <c>primary_jastudio</c>), of a<br/>
/// JAStudio's card server that became the primary JAStudio as JAStudio does (<see cref="PrimaryJAStudioAccessFile"/>), in a folder of the<br/>
/// specification's own. So the two sides of the contract are compared by using both: the add-on's requests, and what JAStudio answers to them.
/// </summary>
/// <remarks>The add-on's client runs in one Python process for every specification: <c>src/tests/jastudio_tests/primary_jastudio/asking_the_primary_jastudio.py</c>.</remarks>
public class When_the_Anki_add_on_asks_the_primary_JAStudio : IAsyncDisposable
{
   static readonly PythonScriptOfTheAddOnsTests TheAddOnsClient = new("jastudio_tests", "primary_jastudio", "asking_the_primary_jastudio.py");
   static readonly string ANoteIdJAStudioHasNot = Guid.NewGuid().ToString();

   readonly CollectionFactory.AppScope _appScope = CollectionFactory.InjectCollectionWithSelectData(DataNeeded.Kanji | DataNeeded.Vocabulary);
   readonly NoteLinkOpenerThatRecordsRequests _noteLinkOpener = new();
   readonly WorkspaceTogglesThatRecordRequests _workspaceToggles = new();
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-primary-jastudio-asked-by-the-add-on-");
   readonly PrimaryJAStudioAccessFile _accessFile;
   readonly CardServer _server;
   readonly BeingThePrimaryJAStudio _beingThePrimaryJAStudio;
   readonly VocabNote _vocab;
   readonly KanjiNote _kanji;

   public When_the_Anki_add_on_asks_the_primary_JAStudio()
   {
      _server = new CardServer(_appScope.CoreApp.Services.Resolve<IRootResolver>(), _noteLinkOpener, _workspaceToggles);
      _server.Start();
      _accessFile = new PrimaryJAStudioAccessFile(_folder);
      _beingThePrimaryJAStudio = _accessFile.TakeUnlessAnotherJAStudioIsIt(_server.Port) ?? throw new InvalidOperationException("No other JAStudio uses the specification's own folder.");
      _vocab = _appScope.CoreApp.Collection.Vocab.WithQuestion("入れる")[0];
      _kanji = _appScope.CoreApp.Collection.Kanji.WithKanji("病")!;
   }

   JapaneseConfig Config => _appScope.CoreApp.Config;
   T Resolve<T>() where T : class => _appScope.CoreApp.Services.Resolve<T>();

   /// <summary>
   /// What the add-on's client answered to <paramref name="request"/>, a method of its <c>PrimaryJAStudio</c>, given <paramref name="arguments"/> by<br/>
   /// their Python names: <c>{"answer": …}</c>, or <c>{"raised": the exception's class, "why": …}</c>.
   /// </summary>
   static JsonElement TheAddOnsAnswerTo(PrimaryJAStudioAccessFile accessFile, string request, object? arguments = null) =>
      TheAddOnsClient.AnswerTo(JsonSerializer.Serialize(new { accessFile = accessFile.File.FullName, request, arguments = arguments ?? new { } }));

   JsonElement TheAddOnsAnswerTo(string request, object? arguments = null) => TheAddOnsAnswerTo(_accessFile, request, arguments);

   static string WhatTheClientRaised(JsonElement answer) => answer.TryGetProperty("raised", out var raised) ? raised.GetString()! : "nothing";

   public async ValueTask DisposeAsync()
   {
      _beingThePrimaryJAStudio.Dispose();
      await _server.StopAsync();
      _folder.Delete(recursive: true);
      _appScope.Dispose();
   }

   public class for_its_version_of_the_contract : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      readonly JsonElement _answer;
      public for_its_version_of_the_contract() => _answer = TheAddOnsAnswerTo("contract_version");

      [XF] public void the_add_on_finds_that_it_speaks_the_add_ons_version() => _answer.GetProperty("answer").GetInt32().Must().Be(4);
   }

   public class for_the_address_of_a_cards_page : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      readonly HttpResponseMessage _theAddressAnswered;

      public for_the_address_of_a_cards_page()
      {
         var address = TheAddOnsAnswerTo("the_address_of_a_cards_page", new { note_id = _vocab.GetId().ToString(), card_type = CardTypes.Reading, side = "back", display_context = "preview" })
                      .GetProperty("answer").GetString()!;
         using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
         _theAddressAnswered = http.Send(new HttpRequestMessage(HttpMethod.Get, address));
      }

      [XF] public void JAStudio_sends_the_iframe_on_to_the_page_of_that_side_of_that_card_as_Anki_shows_it() =>
         _theAddressAnswered.Headers.Location!.OriginalString.Must().Be(CardServerUrl.CardPagePath(_vocab, CardTypes.Reading, "back", "preview"));
   }

   public class for_the_address_of_the_stylesheet_for_Ankis_page : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      readonly HttpResponseMessage _theAddressAnswered;

      public for_the_address_of_the_stylesheet_for_Ankis_page()
      {
         var address = TheAddOnsAnswerTo("the_address_of_the_stylesheet_for_ankis_page").GetProperty("answer").GetString()!;
         using var http = new HttpClient();
         _theAddressAnswered = http.Send(new HttpRequestMessage(HttpMethod.Get, address));
      }

      [XF] public void the_card_server_serves_a_stylesheet_there() => _theAddressAnswered.Content.Headers.ContentType!.MediaType.Must().Be("text/css");
   }

   public class to_toggle_the_workspace_window : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      public to_toggle_the_workspace_window() => TheAddOnsAnswerTo("toggle_the_workspace_window");

      [XF] public void the_workspace_window_is_toggled_once() => _workspaceToggles.TimesTheWorkspaceWindowWasToggled.Must().Be(1);
   }

   public class to_toggle_the_note_search : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      public to_toggle_the_note_search() => TheAddOnsAnswerTo("toggle_the_note_search");

      [XF] public void the_note_search_is_toggled_once() => _workspaceToggles.TimesTheNoteSearchWasToggled.Must().Be(1);
   }

   public class to_show_a_note_in_the_workspace : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      public class which_JAStudio_has : to_show_a_note_in_the_workspace
      {
         public which_JAStudio_has() => TheAddOnsAnswerTo("show_the_note_in_the_workspace", new { note_id = _vocab.GetId().ToString() });

         [XF] public void that_note_is_opened_in_the_workspace() => _noteLinkOpener.NotesOpenedInWorkspace.Must().SequenceEqual([_vocab.GetId()]);
      }

      public class which_JAStudio_has_not : to_show_a_note_in_the_workspace
      {
         readonly JsonElement _answer;
         public which_JAStudio_has_not() => _answer = TheAddOnsAnswerTo("show_the_note_in_the_workspace", new { note_id = ANoteIdJAStudioHasNot });

         [XF] public void the_client_raises_that_JAStudio_refused() => WhatTheClientRaised(_answer).Must().Be("JAStudioRefused");
         [XF] public void saying_why() => _answer.GetProperty("why").GetString().Must().Contain(ANoteIdJAStudioHasNot);
      }
   }

   public class for_the_keys_of_the_note_view_shortcuts : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      readonly string[] _keys;
      public for_the_keys_of_the_note_view_shortcuts() => _keys = TheAddOnsAnswerTo("the_keys_of_the_note_view_shortcuts").GetProperty("answer").EnumerateArray().Select(it => it.GetString()!).ToArray();

      [XF] public void it_answers_with_the_key_of_each_shortcut() => _keys.Must().SequenceEqual(Resolve<NoteViewShortcuts>().All.Select(it => it.KeyGesture));
   }

   public class to_run_a_note_view_shortcut : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      public class that_acts_on_the_note_Anki_shows : to_run_a_note_view_shortcut
      {
         public that_acts_on_the_note_Anki_shows()
         {
            _vocab.Mnemonic.Set("a mnemonic to remove");
            TheAddOnsAnswerTo("run_the_note_view_shortcut", new { key = "0", note_id = _vocab.GetId().ToString() });
         }

         [XF] public void it_acts_on_that_note() => _vocab.Mnemonic.Value.Must().Be("");
      }

      public class whose_key_has_modifiers_while_Anki_shows_no_note : to_run_a_note_view_shortcut
      {
         readonly bool _settingBefore;

         public whose_key_has_modifiers_while_Anki_shows_no_note()
         {
            _settingBefore = Config.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound.Value;
            TheAddOnsAnswerTo("run_the_note_view_shortcut", new { key = "Ctrl+Shift+Alt+s", note_id = (string?)null });
         }

         [XF] public void the_setting_it_toggles_is_toggled() => Config.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound.Value.Must().Be(!_settingBefore);
      }
   }

   public class for_how_long_to_show_the_question : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      double SecondsAnswered(JPNote note, double secondsTheDeckGives, int againAnswersInARowToday) =>
         TheAddOnsAnswerTo("seconds_to_show_the_question", new { note_id = note.GetId().ToString(), seconds_the_deck_gives = secondsTheDeckGives, again_answers_in_a_row_today = againAnswersInARowToday })
           .GetProperty("answer").GetDouble();

      [XF] public void of_a_vocab_it_answers_with_the_time_JAStudio_gives_its_question() =>
         SecondsAnswered(_vocab, secondsTheDeckGives: 5.5, againAnswersInARowToday: 1).Must().Be(Resolve<HowLongToShowTheQuestion>().Seconds(_vocab, 5.5, 1));

      [XF] public void of_a_kanji_it_answers_with_the_time_the_deck_gives_boosted_for_its_failure() =>
         SecondsAnswered(_kanji, secondsTheDeckGives: 5.5, againAnswersInARowToday: 1).Must().Be(5.5 * Config.BoostFailedCardAllowedTimeByFactor.Value);
   }

   public class for_the_length_of_a_timebox : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      JsonElement MinutesAnswered(string noteType, string cardType) => TheAddOnsAnswerTo("timebox_minutes", new { note_type = noteType, card_type = cardType }).GetProperty("answer");

      [XF] public void of_vocab_listening_cards_it_answers_with_the_setting_for_them() =>
         MinutesAnswered(NoteTypes.Vocab, CardTypes.Listening).GetInt64().Must().Be(Config.TimeboxVocabListen.Value);

      [XF] public void of_cards_JAStudio_has_no_timebox_for_it_answers_with_none() => MinutesAnswered("Basic", CardTypes.Reading).ValueKind.Must().Be(JsonValueKind.Null);
   }

   public class once_the_JAStudio_has_stopped_being_the_primary_one : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      readonly JsonElement _answer;

      public once_the_JAStudio_has_stopped_being_the_primary_one()
      {
         _beingThePrimaryJAStudio.Dispose();
         _answer = TheAddOnsAnswerTo("toggle_the_workspace_window");
      }

      [XF] public void the_add_on_finds_no_primary_JAStudio() => _answer.GetProperty("noPrimaryJAStudio").GetBoolean().Must().BeTrue();
      [XF] public void and_asks_nothing() => _workspaceToggles.TimesTheWorkspaceWindowWasToggled.Must().Be(0);
   }

   public class once_the_primary_JAStudio_has_ended_without_stopping : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      readonly JsonElement _answer;

      public once_the_primary_JAStudio_has_ended_without_stopping()
      {
         File.WriteAllText(_accessFile.File.FullName, JsonSerializer.Serialize(new { port = APortNothingListensOn(), processId = 1 }));
         _answer = TheAddOnsAnswerTo("toggle_the_workspace_window");
      }

      [XF] public void the_client_raises_that_JAStudio_does_not_answer() => WhatTheClientRaised(_answer).Must().Be("JAStudioDoesNotAnswer");

      static int APortNothingListensOn()
      {
         var listener = new TcpListener(IPAddress.Loopback, 0);
         listener.Start();
         var port = ((IPEndPoint)listener.LocalEndpoint).Port;
         listener.Stop();
         return port;
      }
   }

   public class for_the_access_file_of_this_user : When_the_Anki_add_on_asks_the_primary_JAStudio
   {
      [XF] public void the_add_on_names_the_file_JAStudio_writes() =>
         TheAddOnsClient.AnswerTo("""{"request": "access-file-of-this-user"}""").GetProperty("accessFile").GetString().Must().Be(PrimaryJAStudioAccessFile.OfThisUser.File.FullName);
   }
}
