using System.Collections.Generic;
using System.Text;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A short tag shown beside a vocab, in its page's header and in sentence breakdowns, that says something about it at a glance, such as its part of<br/>
/// speech, 名 for a noun, or how many sentences it's in.
/// </summary>
/// <param name="Name">What it says, as a name, such as <c>noun</c> or <c>in_no_sentences</c>, which the cards' styles colour it by.</param>
/// <param name="Display">What it shows, such as <c>名</c>.</param>
/// <param name="Tooltip">What it means, in words, shown when the pointer is over it.</param>
public sealed record VocabMetaTag(string Name, string Display, string Tooltip);

static class VocabNoteMetaTagFormatter
{
   public static string GetMetaTagsHtml(VocabNote vocab, bool displayExtendedSentenceStatistics = true, bool noSentenceStatistics = false)
   {
      var sb = new StringBuilder("<ol class=\"vocab_tag_list\">");
      foreach(var tag in GetMetaTags(vocab, displayExtendedSentenceStatistics, noSentenceStatistics))
      {
         sb.Append($"<li class=\"vocab_tag vocab_tag_{tag.Name}\" title=\"{tag.Tooltip}\">{tag.Display}</li>");
      }

      sb.Append("</ol>");

      return sb.ToString();
   }

   public static List<VocabMetaTag> GetMetaTags(VocabNote vocab, bool displayExtendedSentenceStatistics = true, bool noSentenceStatistics = false)
   {
      var tags = vocab.Tags;
      var meta = new List<VocabMetaTag>();
      var partsOfSpeech = vocab.PartsOfSpeech;

      if(!noSentenceStatistics)
      {
         string MaxNineNumber(int value) => value < 10 ? value.ToString() : "+";

         var highlightedIn = vocab.Sentences.Highlighted();
         meta.Add(new VocabMetaTag("highlighted_in_sentences", MaxNineNumber(highlightedIn.Count), $"highlighted in {highlightedIn.Count} sentences"));

         var counts = vocab.Sentences.Counts();
         if(counts.Total > 0)
         {
            var tooltipText = $"in {counts.Total} sentences. Studying-listening:{counts.StudyingListening}, Studying-reading:{counts.StudyingReading}";
            if(counts.StudyingListening > 0 || counts.StudyingReading > 0)
            {
               if(displayExtendedSentenceStatistics)
               {
                  meta.Add(new VocabMetaTag("in_studying_sentences", $"{counts.StudyingListening}:{counts.StudyingReading}/{counts.Total}", tooltipText));
               } else
               {
                  string CreateDisplayText()
                  {
                     if(counts is { StudyingListening: > 9, StudyingReading: > 9 })
                        return "+";
                     return $"{MaxNineNumber(counts.StudyingListening)}:{MaxNineNumber(counts.StudyingReading)}/{MaxNineNumber(counts.Total)}";
                  }

                  meta.Add(new VocabMetaTag("in_studying_sentences", CreateDisplayText(), tooltipText));
               }
            } else
            {
               meta.Add(new VocabMetaTag("in_sentences", counts.Total.ToString(), tooltipText));
            }
         } else
         {
            meta.Add(new VocabMetaTag("in_no_sentences", counts.Total.ToString(), $"in {counts.Total} sentences"));
         }
      }

      // Overarching info
      if(vocab.UsuallyWrittenInKana.IsYes) meta.Add(new VocabMetaTag("uk", "uk", "usually written using kana only"));
      if(partsOfSpeech.Contains(PartOfSpeech.Expression)) meta.Add(new VocabMetaTag("expression", "x", "expression"));
      if(partsOfSpeech.Contains(PartOfSpeech.Abbreviation)) meta.Add(new VocabMetaTag("abbreviation", "abbr", "abbreviation"));
      if(partsOfSpeech.Contains(PartOfSpeech.Auxiliary)) meta.Add(new VocabMetaTag("auxiliary", "aux", "auxiliary"));
      if(partsOfSpeech.Contains(PartOfSpeech.Prefix)) meta.Add(new VocabMetaTag("prefix", "頭", "prefix"));
      if(partsOfSpeech.Contains(PartOfSpeech.Suffix)) meta.Add(new VocabMetaTag("suffix", "尾", "suffix"));

      // Nouns
      if(partsOfSpeech.Contains(PartOfSpeech.ProperNoun)) meta.Add(new VocabMetaTag("proper-noun", "p-名", "proper noun"));
      if(partsOfSpeech.Contains(PartOfSpeech.Pronoun)) meta.Add(new VocabMetaTag("pronoun", "pr-名", "pronoun"));
      if(partsOfSpeech.Contains(PartOfSpeech.Noun)) meta.Add(new VocabMetaTag("noun", "名", "noun"));
      if(partsOfSpeech.Contains(PartOfSpeech.AdverbialNoun)) meta.Add(new VocabMetaTag("adverbial-noun", "副-名", "adverbial noun"));
      if(partsOfSpeech.Contains(PartOfSpeech.IndependentNoun)) meta.Add(new VocabMetaTag("independent-noun", "i-名", "independent noun"));

      // Verbs
      if(partsOfSpeech.Contains(PartOfSpeech.IchidanVerb)) meta.Add(CreateVerbMetaTag("ichidan", "1", "ichidan verb", partsOfSpeech));
      if(partsOfSpeech.Contains(PartOfSpeech.GodanVerb)) meta.Add(CreateVerbMetaTag("godan", "5", "godan verb", partsOfSpeech));
      if(partsOfSpeech.Contains(PartOfSpeech.SuruVerb)) meta.Add(CreateVerbMetaTag("suru-verb", "為", "suru verb", partsOfSpeech));
      if(partsOfSpeech.Contains(PartOfSpeech.KuruVerb)) meta.Add(CreateVerbMetaTag("kuru-verb", "k-v", "kuru verb", partsOfSpeech));

      // Adverbs
      if(partsOfSpeech.Contains(PartOfSpeech.ToAdverb)) meta.Add(new VocabMetaTag("to-adverb", "と", "adverbial noun taking the と particle to act as adverb"));
      else if(partsOfSpeech.Contains(PartOfSpeech.Adverb)) meta.Add(new VocabMetaTag("adverb", "副", "adverb"));
      else if(partsOfSpeech.Contains(PartOfSpeech.Adverbial)) meta.Add(new VocabMetaTag("adverbial", "副", "adverbial"));

      // Adjectives
      if(partsOfSpeech.Contains(PartOfSpeech.IAdjective)) meta.Add(new VocabMetaTag("i-adjective", "い", "true adjective ending on the い copula"));
      if(partsOfSpeech.Contains(PartOfSpeech.NaAdjective)) meta.Add(new VocabMetaTag("na-adjective", "な", "adjectival noun taking the な particle to act as adjective"));
      if(partsOfSpeech.Contains(PartOfSpeech.NoAdjective)) meta.Add(new VocabMetaTag("no-adjective", "の", "adjectival noun taking the の particle to act as adjective"));

      // Misc
      if(partsOfSpeech.Contains(PartOfSpeech.Counter)) meta.Add(new VocabMetaTag("counter", "ctr", "counter"));
      if(partsOfSpeech.Contains(PartOfSpeech.Numeral)) meta.Add(new VocabMetaTag("numeral", "num", "numeral"));
      if(partsOfSpeech.Contains(PartOfSpeech.Interjection)) meta.Add(new VocabMetaTag("interjection", "int", "interjection"));
      if(partsOfSpeech.Contains(PartOfSpeech.Conjunction)) meta.Add(new VocabMetaTag("conjunction", "conj", "conjunction"));
      if(partsOfSpeech.Contains(PartOfSpeech.Particle)) meta.Add(new VocabMetaTag("particle", "prt", "particle"));

      // My own inventions
      if(partsOfSpeech.Contains(PartOfSpeech.MasuSuffix)) meta.Add(new VocabMetaTag("masu-suffix", "連", "follows the 連用形/masu-stem form of a verb"));

      // Register
      var register = vocab.Register;
      if(register.Polite.IsSet()) meta.Add(new VocabMetaTag("register-polite", "P", "Polite"));
      if(register.Formal.IsSet()) meta.Add(new VocabMetaTag("register-formal", "F", "Formal"));
      if(register.Informal.IsSet()) meta.Add(new VocabMetaTag("register-informal", "I", "Informal"));
      if(register.Archaic.IsSet()) meta.Add(new VocabMetaTag("register-archaic", "A", "Archaic"));
      if(register.Sensitive.IsSet()) meta.Add(new VocabMetaTag("register-sensitive", "Se", "Sensitive"));
      if(register.Vulgar.IsSet()) meta.Add(new VocabMetaTag("register-vulgar", "V", "Vulgar, usually offensive"));
      if(register.Childish.IsSet()) meta.Add(new VocabMetaTag("register-childish", "C", "Childish, apt to make the speaker sound immature"));
      if(register.Slang.IsSet()) meta.Add(new VocabMetaTag("register-slang", "Sl", "Slang"));
      if(register.Humble.IsSet()) meta.Add(new VocabMetaTag("register-humble", "Hu", "Humble speech"));
      if(register.Honorific.IsSet()) meta.Add(new VocabMetaTag("register-honorific", "Ho", "Honorific form, used to elevate the listener"));
      if(register.RoughMasculine.IsSet()) meta.Add(new VocabMetaTag("register-rough-masculine", "R", "Rough speech, traditionally thought masculine"));
      if(register.SoftFeminine.IsSet()) meta.Add(new VocabMetaTag("register-soft-feminine", "S", "Soft speech, traditionally thought feminine"));
      if(register.Derogatory.IsSet()) meta.Add(new VocabMetaTag("register-derogatory", "D", "Derogatory form, usually offensive"));
      if(register.Literary.IsSet()) meta.Add(new VocabMetaTag("register-literary", "L", "literary, apt to stand out in speech"));

      // other
      if(!tags.Contains(Tags.Vocab.IsIchidanHidingGodanPotential)) return meta;

      var hiddenGodan = IchidanGodanPotentialOrImperativeHybridSplitter.TryGetGodanHiddenByIchidan(vocab);
      var word = hiddenGodan?.Word ?? "unknown";
      var answer = hiddenGodan?.Answer ?? "unknown";
      meta.Add(new VocabMetaTag("is-ichidan-hiding-godan-potential",
                                "HG",
                                $"Ichidan verb hiding godan potential form of the verb:\n{word}:\n{answer}\nMark the ichidan as an incorrect match to see the godan potential in the breakdown. The parser cannot tell which it is on its own."));
      return meta;
   }

   static VocabMetaTag CreateVerbMetaTag(string name, string display, string tooltip, VocabNotePartsOfSpeech partsOfSpeech)
   {
      var tag = new VocabMetaTag(name, display, tooltip);

      if(partsOfSpeech.IsIntransitive())
         tag = tag with { Display = tag.Display + "i", Tooltip = "intransitive " + tag.Tooltip };

      if(partsOfSpeech.IsTransitive())
         tag = tag with { Display = tag.Display + "t", Tooltip = "transitive " + tag.Tooltip };

      return tag;
   }
}
