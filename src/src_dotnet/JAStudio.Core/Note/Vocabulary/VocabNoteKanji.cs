using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteKanji
{
   public VocabNoteKanji(VocabNote vocab) => Vocab = vocab;

   VocabNote Vocab { get; }

   public List<string> ExtractMainFormKanji()
   {
      var clean = StripHtmlAndBracketMarkup(Vocab.GetQuestion());
      return clean.Where(c => IsKanji(c)).Select(c => c.ToString()).ToList();
   }

   public HashSet<string> ExtractAllKanji()
   {
      var combined = Vocab.GetQuestion() + string.Concat(Vocab.Forms.EnteredFormsList());
      var clean = StripHtmlAndBracketMarkup(combined);
      return clean.Where(c => IsKanji(c)).Select(c => c.ToString()).ToHashSet();
   }

   static string StripHtmlAndBracketMarkup(string text)
   {
      // Remove HTML tags
      var noHtml = Regex.Replace(text, "<[^>]*>", string.Empty);

      // Remove bracket markup like [kanji] or {brackets}
      var noBrackets = Regex.Replace(noHtml, @"\[[^\]]*\]", string.Empty);
      noBrackets = Regex.Replace(noBrackets, @"\{[^\}]*\}", string.Empty);

      return noBrackets;
   }

   static bool IsKanji(char c)
   {
      // Kanji Unicode ranges:
      // CJK Unified Ideographs: U+4E00 to U+9FFF
      // CJK Unified Ideographs Extension A: U+3400 to U+4DBF
      // CJK Compatibility Ideographs: U+F900 to U+FAFF
      var code = (int)c;
      return (code >= 0x4E00 && code <= 0x9FFF) ||
             (code >= 0x3400 && code <= 0x4DBF) ||
             (code >= 0xF900 && code <= 0xFAFF);
   }

   public override string ToString() => $"main: [{string.Join(", ", ExtractMainFormKanji())}], all: [{string.Join(", ", ExtractAllKanji())}]";
}
