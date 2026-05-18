using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using JAStudio.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.SysUtils;

namespace JAStudio.UI.ViewModels;

partial class NoteSearchDialogViewModel : ObservableObject
{
   const int MaxResults = 100;
   readonly Core.TemporaryServiceCollection _services;

   [ObservableProperty] string _searchText = string.Empty;

   [ObservableProperty] string _statusText = "Hit enter to search";

   [ObservableProperty] bool _isSearching = false;

   [ObservableProperty] NoteSearchResultViewModel? _selectedResult;

   public ObservableCollection<NoteSearchResultViewModel> Results { get; } = [];

   public AsyncRelayCommand SearchCommand { get; }

   public NoteSearchDialogViewModel(Core.TemporaryServiceCollection services)
   {
      _services = services;
      SearchCommand = new AsyncRelayCommand(PerformSearchAsync);
   }

   async Task PerformSearchAsync()
   {
      var searchText = SearchText.Trim();
      if(string.IsNullOrWhiteSpace(searchText))
      {
         Results.Clear();
         StatusText = "Hit enter to search";
         return;
      }

      IsSearching = true;
      StatusText = "Searching...";

      try
      {
         // Run search in background to avoid blocking UI
         var results = await TaskCE.Run(() => SearchNotes(searchText));

         Results.Clear();
         foreach(var result in results)
         {
            Results.Add(result);
         }

         // Update status
         if(results.Count == 0)
         {
            StatusText = "No results found";
         } else if(results.Count >= MaxResults)
         {
            StatusText = $"{MaxResults}+ notes found. Showing first {MaxResults}";
         } else
         {
            StatusText = $"{results.Count} note{(results.Count != 1 ? "s" : "")} found";
         }
      }
      finally
      {
         IsSearching = false;
      }
   }

   List<NoteSearchResultViewModel> SearchNotes(string searchText)
   {
      var results = new List<NoteSearchResultViewModel>();

      var col = _services.CoreApp.Collection;

      // Search in kanji notes
      results.AddRange(SearchInNotes(
                          col.Kanji.All().ToList(),
                          searchText,
                          note => new Dictionary<string, Func<string>>
                                  {
                                     ["kanji_readings"] = () => string.Join(" ", note.ReadingsClean),
                                     ["kanji_romaji_readings"] = () => KanaUtils.Romanize(string.Join(" ", note.ReadingsClean)),
                                     ["question"] = () => StripHtml(note.GetQuestion()),
                                     ["answer"] = () => StripHtml(note.GetAnswer())
                                  }
                       ));

      if(results.Count >= MaxResults)
         return results.Take(MaxResults).ToList();

      // Search in vocab notes
      var vocabs = col.Vocab.All()
                      .OrderBy(v => v.GetQuestion().Length)
                      .ToList();

      results.AddRange(SearchInNotes(
                          vocabs,
                          searchText,
                          note => new Dictionary<string, Func<string>>
                                  {
                                     ["vocab_readings"] = () => string.Join(" ", note.GetReadings()),
                                     ["vocab_romaji_readings"] = () => KanaUtils.Romanize(string.Join(" ", note.GetReadings())),
                                     ["forms"] = () => string.Join(" ", note.Forms.WithoutNoiseCharacters()),
                                     ["question"] = () => StripHtml(note.GetQuestion()),
                                     ["answer"] = () => StripHtml(note.GetAnswer())
                                  }
                       ));

      if(results.Count >= MaxResults)
         return results.Take(MaxResults).ToList();

      // Search in sentence notes
      var sentences = col.Sentences.All()
                         .OrderBy(s => s.GetQuestion().Length)
                         .ToList();

      results.AddRange(SearchInNotes(
                          sentences,
                          searchText,
                          note => new Dictionary<string, Func<string>>
                                  {
                                     ["question"] = () => StripHtml(note.GetQuestion()),
                                     ["answer"] = () => StripHtml(note.GetAnswer())
                                  }
                       ));

      return results.Take(MaxResults).ToList();
   }

   static List<NoteSearchResultViewModel> SearchInNotes<TNote>(
      List<TNote> notes,
      string searchText,
      Func<TNote, Dictionary<string, Func<string>>> extractorsFactory)
      where TNote : JPNote
   {
      var results = new List<NoteSearchResultViewModel>();

      // Split search text by " && " to get multiple conditions
      var searchConditions = searchText
                            .Split([" && "], StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .ToList();

      foreach(var note in notes)
      {
         if(results.Count >= MaxResults)
            break;

         var extractors = extractorsFactory(note);
         var allConditionsMatch = true;

         foreach(var condition in searchConditions)
         {
            var conditionMatches = false;
            var conditionLower = condition.ToLowerInvariant();

            // Check for prefixed search
            if(condition.StartsWith("r:", StringComparison.OrdinalIgnoreCase))
            {
               // Only search in reading fields
               var readingValue = condition.Substring(2).Trim().ToLowerInvariant();
               var readingFields = extractors
                                  .Where(kvp => kvp.Key.Contains("reading", StringComparison.OrdinalIgnoreCase))
                                  .ToList();

               if(!readingFields.Any())
               {
                  allConditionsMatch = false;
                  break;
               }

               foreach(var extractor in readingFields)
               {
                  var fieldText = extractor.Value().ToLowerInvariant();
                  if(fieldText.Contains(readingValue))
                  {
                     conditionMatches = true;
                     break;
                  }
               }
            } else if(condition.StartsWith("a:", StringComparison.OrdinalIgnoreCase))
            {
               // Only search in answer field
               var answerValue = condition.Substring(2).Trim().ToLowerInvariant();
               if(extractors.TryGetValue("answer", out var answerExtractor))
               {
                  var fieldText = answerExtractor().ToLowerInvariant();
                  if(fieldText.Contains(answerValue))
                  {
                     conditionMatches = true;
                  }
               }
            } else if(condition.StartsWith("q:", StringComparison.OrdinalIgnoreCase))
            {
               // Only search in question field
               var questionValue = condition.Substring(2).Trim().ToLowerInvariant();
               if(extractors.TryGetValue("question", out var questionExtractor))
               {
                  var fieldText = questionExtractor().ToLowerInvariant();
                  if(fieldText.Contains(questionValue))
                  {
                     conditionMatches = true;
                  }
               }
            } else
            {
               // Standard search in all fields
               foreach(var extractor in extractors.Values)
               {
                  var fieldText = extractor().ToLowerInvariant();
                  if(fieldText.Contains(conditionLower))
                  {
                     conditionMatches = true;
                     break;
                  }
               }
            }

            if(!conditionMatches)
            {
               allConditionsMatch = false;
               break;
            }
         }

         if(allConditionsMatch)
         {
            results.Add(new NoteSearchResultViewModel(note));
         }
      }

      return results;
   }

   static string StripHtml(string html) =>
      // Strip HTML tags and bracket markup
      html.StripHtmlAndBracketMarkupAndNoiseCharacters();

   public void OpenSelectedNote()
   {
      if(SelectedResult == null)
         return;

      var noteId = SelectedResult.NoteId;
      var query = _services.QueryBuilder().NotesByIds([noteId]);
      AnkiFacade.Browser.ExecuteLookup(query);
   }
}

class NoteSearchResultViewModel
{
   public NoteId NoteId { get; }
   public string NoteType { get; }
   public string Question { get; }
   public string Answer { get; }

   public NoteSearchResultViewModel(JPNote note)
   {
      NoteId = note.GetId();
      NoteType = GetNoteTypeDisplay(note);
      Question = note.GetQuestion().StripHtmlAndBracketMarkupAndNoiseCharacters();
      Answer = note.GetAnswer().StripHtmlAndBracketMarkupAndNoiseCharacters();
   }

   static string GetNoteTypeDisplay(JPNote note)
   {
      return note switch
      {
         VocabNote    => "Vocab",
         KanjiNote    => "Kanji",
         SentenceNote => "Sentence",
         _            => "Note"
      };
   }
}
