using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Vocabulary;

class VocabMetaTag
{
   public string Name { get; set; }
   public string Display { get; set; }
   public string Tooltip { get; set; }

   public VocabMetaTag(string name, string display, string tooltip)
   {
      Name = name;
      Display = display;
      Tooltip = tooltip;
   }
}

static class VocabNoteMetaTagFormatter
{
   public static string GetMetaTagsHtml(VocabNote vocab, bool displayExtendedSentenceStatistics = true, bool noSentenceStatistics = false)
   {
      var tags = vocab.Tags;
      var tagNames = tags.Select(t => t.Name).ToHashSet();
      var meta = new List<VocabMetaTag>();
      var tos = vocab.PartsOfSpeech.RawStringValue()
                     .Split(',')
                     .Select(t => t.ToLower().Trim())
                     .ToHashSet();

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
                     if(counts.StudyingListening > 9 && counts.StudyingReading > 9)
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
      if(tagNames.Contains("_uk")) meta.Add(new VocabMetaTag("uk", "uk", "usually written using kana only"));
      if(tos.Contains(POS.Expression)) meta.Add(new VocabMetaTag(POS.Expression, "x", "expression"));
      if(tos.Contains(POS.Abbreviation)) meta.Add(new VocabMetaTag("abbreviation", "abbr", "abbreviation"));
      if(tos.Contains(POS.Auxiliary)) meta.Add(new VocabMetaTag("auxiliary", "aux", "auxiliary"));
      if(tos.Contains(POS.Prefix)) meta.Add(new VocabMetaTag("prefix", "頭", "prefix"));
      if(tos.Contains(POS.Suffix)) meta.Add(new VocabMetaTag("suffix", "尾", "suffix"));

      // Nouns
      if(tos.Contains(POS.ProperNoun)) meta.Add(new VocabMetaTag("proper-noun", "p-名", "proper noun"));
      if(tos.Contains(POS.Pronoun)) meta.Add(new VocabMetaTag("pronoun", "pr-名", "pronoun"));
      if(tos.Contains(POS.Noun)) meta.Add(new VocabMetaTag(POS.Noun, "名", "noun"));
      if(tos.Contains(POS.AdverbialNoun)) meta.Add(new VocabMetaTag("adverbial-noun", "副-名", "adverbial noun"));
      if(tos.Contains(POS.IndependentNoun)) meta.Add(new VocabMetaTag("independent-noun", "i-名", "independent noun"));

      // Verbs
      if(tos.Contains(POS.IchidanVerb)) meta.Add(CreateVerbMetaTag("ichidan", "1", POS.IchidanVerb, tos));
      if(tos.Contains(POS.GodanVerb)) meta.Add(CreateVerbMetaTag("godan", "5", POS.GodanVerb, tos));
      if(tos.Contains(POS.SuruVerb) || tos.Contains("verbal noun") || tos.Contains("する verb")) meta.Add(CreateVerbMetaTag("suru-verb", "為", POS.SuruVerb, tos));
      if(tos.Contains(POS.KuruVerb)) meta.Add(CreateVerbMetaTag("kuru-verb", "k-v", "kuru verb", tos));
      if(tos.Contains("auxiliary verb")) meta.Add(CreateVerbMetaTag("auxiliary-verb", "aux-v", "auxiliary verb", tos));

      // Adverbs
      if(tos.Contains(POS.ToAdverb)) meta.Add(new VocabMetaTag("to-adverb", "と", "adverbial noun taking the と particle to act as adverb"));
      else if(tos.Contains(POS.Adverb)) meta.Add(new VocabMetaTag("adverb", "副", "adverb"));
      else if(tos.Contains(POS.Adverbial)) meta.Add(new VocabMetaTag("adverbial", "副", "adverbial"));

      // Adjectives
      if(tos.Contains(POS.IAdjective)) meta.Add(new VocabMetaTag("i-adjective", "い", "true adjective ending on the い copula"));
      if(tos.Contains(POS.NaAdjective)) meta.Add(new VocabMetaTag("na-adjective", "な", "adjectival noun taking the な particle to act as adjective"));
      if(tos.Contains(POS.NoAdjective)) meta.Add(new VocabMetaTag("no-adjective", "の", "adjectival noun taking the の particle to act as adjective"));
      if(tos.Contains("auxiliary adjective")) meta.Add(new VocabMetaTag("auxiliary-adjective", "a-い", "auxiliary adjective"));

      // Misc
      if(tos.Contains(POS.Counter)) meta.Add(new VocabMetaTag("counter", "ctr", "counter"));
      if(tos.Contains(POS.Numeral)) meta.Add(new VocabMetaTag("numeral", "num", "numeral"));
      if(tos.Contains(POS.Interjection)) meta.Add(new VocabMetaTag("interjection", "int", "interjection"));
      if(tos.Contains(POS.Conjunction)) meta.Add(new VocabMetaTag("conjunction", "conj", "conjunction"));
      if(tos.Contains(POS.Particle)) meta.Add(new VocabMetaTag("particle", "prt", "particle"));

      // My own inventions
      if(tos.Contains(POS.MasuSuffix)) meta.Add(new VocabMetaTag("masu-suffix", "連", "follows the 連用形/masu-stem form of a verb"));

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
      if(tags.Contains(Tags.Vocab.IsIchidanHidingGodanPotential))
      {
         var hiddenGodan = IchidanGodanPotentialOrImperativeHybridSplitter.TryGetGodanHiddenByIchidan(vocab);
         var word = hiddenGodan?.Word ?? "unknown";
         var answer = hiddenGodan?.Answer ?? "unknown";
         meta.Add(new VocabMetaTag("is-ichidan-hiding-godan-potential",
                                   "HG",
                                   $"Ichidan verb hiding godan potential form of the verb:\n{word}:\n{answer}\nMark the ichidan as an incorrect match to see the godan potential in the breakdown. The parser cannot tell which it is on its own."));
      }

      var sb = new StringBuilder("<ol class=\"vocab_tag_list\">");
      foreach(var tag in meta)
      {
         sb.Append($"<li class=\"vocab_tag vocab_tag_{tag.Name}\" title=\"{tag.Tooltip}\">{tag.Display}</li>");
      }

      sb.Append("</ol>");

      return sb.ToString();
   }

   static VocabMetaTag CreateVerbMetaTag(string name, string display, string tooltip, HashSet<string> tos)
   {
      var tag = new VocabMetaTag(name, display, tooltip);

      if(tos.Contains(POS.Intransitive))
      {
         tag.Display += "i";
         tag.Tooltip = "intransitive " + tag.Tooltip;
      }

      if(tos.Contains(POS.Transitive))
      {
         tag.Display += "t";
         tag.Tooltip = "transitive " + tag.Tooltip;
      }

      return tag;
   }
}
