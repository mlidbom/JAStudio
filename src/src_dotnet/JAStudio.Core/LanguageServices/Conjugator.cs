using System.Collections.Generic;
using System.Linq;
using Compze.Internals.Logging;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
public static class Conjugator
{
   static readonly ILogger Log = CompzeLogger.For(typeof(Conjugator));
   static readonly List<string> IchidanEndings = ["", "ろ", "な"];

   static readonly List<string> GodanRuEndings = ["り", "ら", "れ", "っ"];
   static readonly List<string> GodanRuOrIchidanEndings;

   public static readonly Dictionary<string, string> GodanPotentialVerbEndingToDictionaryFormEndings = new()
                                                                                                       {
                                                                                                          { "える", "う" },
                                                                                                          { "ける", "く" },
                                                                                                          { "げる", "ぐ" },
                                                                                                          { "せる", "す" },
                                                                                                          { "てる", "つ" },
                                                                                                          { "ねる", "ぬ" },
                                                                                                          { "べる", "ぶ" },
                                                                                                          { "める", "む" },
                                                                                                          { "れる", "る" }
                                                                                                       };

   public static readonly HashSet<string> GodanImperativeVerbEndings = ["え", "け", "げ", "せ", "て", "ね", "べ", "め", "れ"];

   const int IStemIndex = 0;
   const int AStemIndex = 1;
   const int EStemIndex = 2;
   const int TeStemIndex = 3;

   static readonly Dictionary<string, List<string>> OneCharacterMappings = new()
                                                                           {
                                                                              { "う", ["い", "わ", "え", "っ"] },
                                                                              { "く", ["き", "か", "け", "い"] },
                                                                              { "ぐ", ["ぎ", "が", "げ", "い"] },
                                                                              { "す", ["し", "さ", "せ", "し"] },
                                                                              { "つ", ["ち", "た", "て", "っ"] },
                                                                              { "ぬ", ["に", "な", "ね", "ん"] },
                                                                              { "ぶ", ["び", "ば", "べ", "ん"] },
                                                                              { "む", ["み", "ま", "め", "ん"] },
                                                                              { "る", GodanRuEndings },
                                                                              { "い", ["く", "け", "か"] } // I adjective stems
                                                                           };

   static readonly Dictionary<string, List<string>> TwoCharacterMappings = new()
                                                                           {
                                                                              { "する", ["し", "さ", "すれ", "し", "せ"] },
                                                                              { "くる", ["き", "こ", "くれ", "き"] },
                                                                              { "いく", ["いき", "いか", "いけ", "いっ", "いこ"] },
                                                                              { "行く", ["行き", "行か", "行け", "行っ", "行こ"] },
                                                                              { "ます", ["まし", "ませ"] },
                                                                              { "いい", ["よく", "よけ", "よか", "よかっ"] }
                                                                           };

   static readonly List<string> MasuFormsByIndex = ["まし", "ませ", "まし", "まし"];

   static readonly HashSet<string> AruVerbs = ["なさる", "くださる", "おっしゃる", "ござる", "らっしゃる", "下さる", "為さる"];

   static readonly Dictionary<string, List<string>> AruMappings = new()
                                                                  {
                                                                     { "さる", ["さい", "さら", "され", "さっ"] },
                                                                     { "ざる", ["ざい", "ざら", "ざれ", "ざっ"] },
                                                                     { "ゃる", ["ゃい", "ゃら", "れば", "ゃっ"] }
                                                                  };

   static Conjugator()
   {
      GodanRuOrIchidanEndings = new List<string>(GodanRuEndings);
      GodanRuOrIchidanEndings.AddRange(IchidanEndings);
   }

   public static string ConstructRootVerbForPossiblyPotentialGodanVerbDictionaryForm(string potentialVerbForm)
   {
      var ending = potentialVerbForm.Substring(potentialVerbForm.Length - 2);
      return potentialVerbForm.Substring(0, potentialVerbForm.Length - 2) +
             GodanPotentialVerbEndingToDictionaryFormEndings[ending];
   }

   static bool IsAruVerb(string word) => AruVerbs.Any(word.EndsWith);

   public static List<string> GetWordStems(string word, bool isIchidanVerb = false, bool isGodan = false)
   {
      try
      {
         if(IsAruVerb(word))
         {
            var ending = word.Substring(word.Length - 2);
            return AruMappings[ending].Select(end => word.Substring(0, word.Length - 2) + end).ToList();
         }

         if(isIchidanVerb)
         {
            return IchidanEndings.Select(end => word.Substring(0, word.Length - 1) + end).ToList();
         }

         if(isGodan)
         {
            var lastChar = word.Substring(word.Length - 1);
            return OneCharacterMappings[lastChar].Select(end => word.Substring(0, word.Length - 1) + end).ToList();
         }

         var lastTwoChars = word.Length >= 2 ? word.Substring(word.Length - 2) : "";
         if(TwoCharacterMappings.ContainsKey(lastTwoChars))
         {
            return TwoCharacterMappings[lastTwoChars].Select(end => word.Substring(0, word.Length - 2) + end).ToList();
         }

         var lastOneChar = word.Substring(word.Length - 1);
         if(OneCharacterMappings.ContainsKey(lastOneChar))
         {
            if(lastOneChar == "る")
            {
               return GodanRuOrIchidanEndings.Select(end => word.Substring(0, word.Length - 1) + end).ToList();
            }

            return OneCharacterMappings[lastOneChar].Select(end => word.Substring(0, word.Length - 1) + end).ToList();
         }
      }
      catch(KeyNotFoundException)
      {
         Log.Warning($"GetWordStems failed to handle {word}, returning empty list");
      }
      catch(System.ArgumentOutOfRangeException)
      {
         Log.Warning($"GetWordStems failed to handle {word}, returning empty list");
      }

      return [word];
   }

   static string GetStem(string word, int stemIndex, bool isIchidanVerb = false, bool isGodan = false)
   {
      try
      {
         if(IsAruVerb(word))
         {
            var ending = word.Substring(word.Length - 2);
            return word.Substring(0, word.Length - 2) + AruMappings[ending][stemIndex];
         }

         if(isIchidanVerb)
         {
            return word.Substring(0, word.Length - 1);
         }

         if(isGodan)
         {
            var lastChar = word.Substring(word.Length - 1);
            return word.Substring(0, word.Length - 1) + OneCharacterMappings[lastChar][stemIndex];
         }

         var lastTwoChars = word.Length >= 2 ? word.Substring(word.Length - 2) : "";
         if(lastTwoChars == "ます")
         {
            return word.Substring(0, word.Length - 2) + MasuFormsByIndex[stemIndex];
         }

         if(TwoCharacterMappings.TryGetValue(lastTwoChars, out var mapping))
         {
            return word.Substring(0, word.Length - 2) + mapping[stemIndex];
         }

         var lastOneChar = word.Substring(word.Length - 1);
         if(OneCharacterMappings.TryGetValue(lastOneChar, out var characterMapping))
         {
            return word.Substring(0, word.Length - 1) + characterMapping[stemIndex];
         }
      }
      catch(KeyNotFoundException)
      {
         Log.Warning($"GetStem failed to handle {word}, returning word as-is");
      }
      catch(System.ArgumentOutOfRangeException)
      {
         Log.Warning($"GetStem failed to handle {word}, returning word as-is");
      }

      return word;
   }

   public static List<string> GetVocabStems(VocabNote vocab) => GetWordStems(vocab.GetQuestion(), vocab.PartsOfSpeech.IsIchidan(), vocab.PartsOfSpeech.IsGodan());

   public static string GetIStem(string word, bool isIchidan = false, bool isGodan = false) => GetStem(word, IStemIndex, isIchidan, isGodan);

   public static string GetAStem(string word, bool isIchidan = false, bool isGodan = false) => GetStem(word, AStemIndex, isIchidan, isGodan);

   public static string GetEStem(string word, bool isIchidan = false, bool isGodan = false)
   {
      if(isIchidan)
      {
         return word.Substring(0, word.Length - 1) + "れ";
      }

      return GetStem(word, EStemIndex, isIchidan, isGodan);
   }

   public static string GetTeStem(string word, bool isIchidan = false, bool isGodan = false) => GetStem(word, TeStemIndex, isIchidan, isGodan);

   public static string GetIStemVocab(VocabNote vocab, string form = "")
   {
      var word = !string.IsNullOrEmpty(form) ? form : vocab.GetQuestion();
      return GetIStem(word, vocab.PartsOfSpeech.IsIchidan(), vocab.PartsOfSpeech.IsGodan());
   }

   public static string GetEStemVocab(VocabNote vocab, string form = "")
   {
      var word = !string.IsNullOrEmpty(form) ? form : vocab.GetQuestion();
      return GetEStem(word, vocab.PartsOfSpeech.IsIchidan(), vocab.PartsOfSpeech.IsGodan());
   }

   public static string GetAStemVocab(VocabNote vocab, string form = "")
   {
      var word = !string.IsNullOrEmpty(form) ? form : vocab.GetQuestion();
      return GetAStem(word, vocab.PartsOfSpeech.IsIchidan(), vocab.PartsOfSpeech.IsGodan());
   }

   public static string GetTeStemVocab(VocabNote vocab, string form = "")
   {
      var word = !string.IsNullOrEmpty(form) ? form : vocab.GetQuestion();
      return GetTeStem(word, vocab.PartsOfSpeech.IsIchidan(), vocab.PartsOfSpeech.IsGodan());
   }

   public static string GetImperative(string word, bool isIchidan = false, bool isGodan = false)
   {
      if(isGodan || (!isIchidan && !word.EndsWith("る")))
      {
         return GetEStem(word, isIchidan, isGodan);
      }

      return word.Substring(0, word.Length - 1) + "ろ";
   }
}
