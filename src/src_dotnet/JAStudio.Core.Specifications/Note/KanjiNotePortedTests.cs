using JAStudio.Core.Note.Collection;
using JAStudio.Core.Specifications.Fixtures;
using Xunit;

namespace JAStudio.Core.Specifications.Note;

/// <summary>
/// Tests ported from test_kanjinote.py
/// </summary>
public class KanjiNotePortedTests : SpecificationUsingACollection
{
   public KanjiNotePortedTests() : base(DataNeeded.Kanji) {}

   [Fact]
   public void InsideRadicalPopulation()
   {
      var inside = GetService<KanjiCollection>().WithKanji("内")!;
      inside.Mnemonic.Set("<rad>head</rad> <rad>person</rad>");

      inside.PopulateRadicalsFromMnemonicTags();

      Assert.Equal(["冂", "人"], inside.Radicals);
   }

   [Theory]
   [InlineData("内", "冂, 人", "<rad>head</rad> <rad>person</rad> <kan>inside</kan> <compound-reading><read>U</read>ber-<read>chi</read>mp</compound-reading> ...")]
   [InlineData("病", "疒,丙", "<rad>sick</rad> <rad>dynamite</rad> <kan>illness</kan> <read>yu</read>ck <compound-reading><read>yu</read>ck-<read>mi</read>ce</compound-reading> <read>BO</read> ...")]
   [InlineData("品", "", "<kan>goods</kan> <read>hin</read>t <compound-reading><read>shi</read>t-<read>nu</read>t</compound-reading> ...")]
   [InlineData("塚", "", "<kan>a-mound</kan> <read>Tsuka</read> ...")]
   public void BootstrapMnemonic(string kanji, string radicals, string expectedMnemonic)
   {
      var kanjiNote = GetService<KanjiCollection>().WithKanji(kanji)!;
      kanjiNote.RadicalsRaw = radicals;

      kanjiNote.BootstrapMnemonicFromRadicals();

      Assert.Equal(expectedMnemonic, kanjiNote.Mnemonic.Value);
   }

   [Fact]
   public void GetPrimaryMeaning()
   {
      var one = GetService<KanjiCollection>().WithKanji("一")!;
      Assert.Equal("ground", one.PrimaryRadicalMeaning);

      var hon = GetService<KanjiCollection>().WithKanji("本")!;
      Assert.Equal("true", hon.PrimaryRadicalMeaning);
   }
}
