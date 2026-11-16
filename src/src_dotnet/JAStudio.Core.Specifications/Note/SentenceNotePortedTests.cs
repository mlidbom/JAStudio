using JAStudio.Core.Note;
using Xunit;

namespace JAStudio.Core.Specifications.Note;

/// <summary>
/// Tests ported from test_sentencenote.py
/// </summary>
public class SentenceNotePortedTests : SpecificationStartingWithAnEmptyCollection
{
   [Fact]
   public void SplitToken()
   {
      var sentence = "だったら普通に金貸せって言えよ";

      var sentenceNoteReference = CreateSentence(sentence).Reference();
      sentenceNoteReference.Current.Edit(it => it.Question.SplitTokenWithWordBreakTag("金貸"));

      Assert.Equal($"だったら普通に金{StringExtensions.InvisibleSpace}貸せって言えよ", sentenceNoteReference.Current.Question.WithInvisibleSpace());
   }
}
