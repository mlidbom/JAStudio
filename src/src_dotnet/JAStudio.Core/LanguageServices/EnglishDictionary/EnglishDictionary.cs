using Compze.Internals.SystemCE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JAStudio.Core.LanguageServices.EnglishDictionary;

public class WordSense
{
   public string Definition { get; set; }
   public string Pos { get; set; }

   public WordSense(string definition, string pos)
   {
      Definition = definition;
      Pos = pos;
   }
}

public class EnglishWord
{
   public string Word { get; set; }
   public string LowerCaseWord { get; set; }
   public List<WordSense> Senses { get; set; }

   public EnglishWord(string word, string definition = "", string pos = "")
   {
      Word = word;
      LowerCaseWord = word.ToLowerInvariant();
      Senses = [];

      // Add the initial sense if provided
      if(!string.IsNullOrEmpty(definition) || !string.IsNullOrEmpty(pos))
      {
         AddSense(definition, pos);
      }
   }

   public void AddSense(string definition, string pos)
   {
      Senses.Add(new WordSense(definition, pos));
   }
}

public class EnglishDictionary
{
   static readonly Lazy<EnglishDictionary> _instance = new(() => new EnglishDictionary());
   public static EnglishDictionary Instance => _instance.Value;

   public List<EnglishWord> Words { get; }
   public Dictionary<string, EnglishWord> WordMap { get; }

   EnglishDictionary()
   {
      Words = [];
      WordMap = new Dictionary<string, EnglishWord>();

      // Get the path to the data files
      var currentDir = Path.GetDirectoryName(typeof(EnglishDictionary).Assembly.Location)!;
      var dataDir = Path.Combine(currentDir, "LanguageServices", "EnglishDictionary", "data");
      var dataFilePath = Path.Combine(dataDir, "english_words.csv");
      var rawWordListPath = Path.Combine(dataDir, "raw_word_list.txt");

      // Load english_words.csv
      if(File.Exists(dataFilePath))
      {
         var lines = File.ReadAllLines(dataFilePath);
         // Skip header
         foreach(var line in lines.Skip(1))
         {
            if(string.IsNullOrWhiteSpace(line))
               continue;

            var parts = line.Split(',', 3);

            if(parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
            {
               var word = parts[0];
               var pos = parts.Length > 1 ? parts[1] : "";
               var definition = parts.Length > 2 ? parts[2] : "";

               // Check if the word already exists in our dictionary
               var lowerWord = word.ToLowerInvariant();
               if(WordMap.TryGetValue(lowerWord, out var existingWord))
               {
                  // Add a new sense to the existing word
                  existingWord.AddSense(definition, pos);
               } else
               {
                  // Create a new EnglishWord
                  var englishWord = new EnglishWord(word, definition, pos);
                  Words.Add(englishWord);
                  WordMap[lowerWord] = englishWord;
               }
            }
         }
      }

      // Load raw_word_list.txt
      if(File.Exists(rawWordListPath))
      {
         var lines = File.ReadAllLines(rawWordListPath);
         foreach(var line in lines)
         {
            var word = line.Trim();
            if(!string.IsNullOrWhiteSpace(word))
            {
               var lowerWord = word.ToLowerInvariant();
               if(!WordMap.ContainsKey(lowerWord))
               {
                  var englishWord = new EnglishWord(word, "no-definition");
                  Words.Add(englishWord);
                  WordMap[lowerWord] = englishWord;
               }
            }
         }
      }
   }

   public List<EnglishWord> WordsContainingStartingWithFirstThenByShortestFirst(string searchString)
   {
      searchString = searchString.ToLowerInvariant();

      var hits = Words.Where(word => word.LowerCaseWord.Contains(searchString)).ToList();

      // Sort by: 1) starts with search (false first = starts with), 2) length, 3) alphabetically
      return hits.OrderBy(word => !word.LowerCaseWord.StartsWithOrdinal(searchString))
                 .ThenBy(word => word.LowerCaseWord.Length)
                 .ThenBy(word => word.LowerCaseWord)
                 .ToList();
   }
}
