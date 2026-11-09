using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Dictionary;

namespace JAStudio.Core.LanguageServices.JamdictEx;

public sealed class DictEntry
{
   public IReadOnlyList<WordReading> KanaForms { get; }
   public IReadOnlyList<WordKanji> KanjiForms { get; }
   public IReadOnlyList<WordSense> Senses { get; }
   readonly IReadOnlyList<FrozenSet<PartOfSpeech>> _partsOfSpeechBySense;

   DictEntry(IReadOnlyList<WordReading> kanaForms, IReadOnlyList<WordKanji> kanjiForms, IReadOnlyList<WordSense> senses)
   {
      KanaForms = kanaForms;
      KanjiForms = kanjiForms;
      Senses = senses;
      _partsOfSpeechBySense = [.. senses.Select(sense => DictionaryPartsOfSpeech.Of(sense.PartsOfSpeech))];
   }

   internal static DictEntry FromWord(WordEntry source) =>
      new(source.Readings, source.Kanjis, source.Senses);

   internal static DictEntry FromName(NameEntry source) =>
      new(source.Readings.Select(r => new WordReading(r.Text, r.Priorities)).ToList(),
          source.Kanjis.Select(k => new WordKanji(k.Text, k.Priorities)).ToList(),
          source.Translations.Select(t => new WordSense(t.Transcriptions, [], [])).ToList());

   public List<string> KanaFormsText() => KanaForms.Select(it => it.Text).ToList();
   public List<string> KanjiFormsText() => KanjiForms.Select(it => it.Text).ToList();

   public bool IsKanaOnly() =>
      KanjiForms.Count == 0 || Senses.Any(sense => sense.Miscellanea.Contains("word usually written using kana alone"));

   public bool HasMatchingKanaForm(string search)
   {
      search = KanaUtils.KatakanaToHiragana(search);
      return KanaForms.Any(form => search == KanaUtils.KatakanaToHiragana(form.Text));
   }

   public bool HasMatchingKanjiForm(string search)
   {
      search = KanaUtils.KatakanaToHiragana(search);
      return KanjiForms.Any(form => search == KanaUtils.KatakanaToHiragana(form.Text));
   }

   public HashSet<string> ValidForms(bool forceAllowKanaOnly = false)
   {
      var kanaSet = KanaFormsText().ToHashSet();
      var kanjiSet = KanjiFormsText().ToHashSet();

      if(IsKanaOnly() || forceAllowKanaOnly)
      {
         kanaSet.UnionWith(kanjiSet);
         return kanaSet;
      }

      return kanjiSet;
   }

   public bool IsTransitiveVerb() => _partsOfSpeechBySense.All(it => it.Contains(PartOfSpeech.Transitive));
   public bool IsIntransitiveVerb() => _partsOfSpeechBySense.All(it => it.Contains(PartOfSpeech.Intransitive));
   public bool IsToBeVerb() => Senses.All(s => s.Glosses.All(g => g.StartsWith("to be ")));

   /// <summary>The <see cref="PartOfSpeech"/>s of all the entry's senses.</summary>
   public HashSet<PartOfSpeech> PartsOfSpeech() => [.. _partsOfSpeechBySense.SelectMany(it => it)];

   public string FormatAnswer()
   {
      string FormatSenseGlosses(int senseIndex)
      {
         var sense = Senses[senseIndex];
         var partsOfSpeech = _partsOfSpeechBySense[senseIndex];
         var glosses = sense.Glosses.Select(g => g.Replace(" ", "-")).ToList();

         if(partsOfSpeech.Any(it => it.IsAKindOfVerb))
         {
            var typeMarker = "{?}";
            if(partsOfSpeech.Contains(PartOfSpeech.Transitive)) typeMarker = "{}";
            if(partsOfSpeech.Contains(PartOfSpeech.Intransitive)) typeMarker = ":";

            var startGroup = "{";
            var endGroup = "}";
            if(glosses.Count == 1)
            {
               startGroup = "";
               endGroup = "";
            }

            bool AllStart(string prefix) => glosses.All(it => it.StartsWith(prefix));

            if(AllStart("to-be-"))
               return $"to-be:{startGroup}{string.Join("/", glosses.Select(it => RemovePrefix(it, "to-be-")))}{endGroup}";

            if(AllStart("to-"))
               return $"to{typeMarker}{startGroup}{string.Join("/", glosses.Select(it => RemovePrefix(it, "to-")))}{endGroup}";
         }

         return string.Join("/", glosses);
      }

      var defaultFormat = string.Join(" | ", Enumerable.Range(0, Senses.Count).Select(FormatSenseGlosses));

      if(IsToBeVerb())
         return $"to-be: {defaultFormat.Replace("to-be:", "").Replace("{", "").Replace("}", "")}";

      if(IsTransitiveVerb())
         return $"to{{}} {defaultFormat.Replace("to{}", "").Replace("{", "").Replace("}", "")}";

      if(IsIntransitiveVerb())
      {
         if(defaultFormat.Contains("to-be:"))
            return $"to: {defaultFormat.Replace("to-be:", "be-").Replace("to:", "")}";
         return $"to: {defaultFormat.Replace("to:", "").Replace("{", "").Replace("}", "")}";
      }

      return defaultFormat;
   }

   static string RemovePrefix(string text, string prefix) => text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;
}
