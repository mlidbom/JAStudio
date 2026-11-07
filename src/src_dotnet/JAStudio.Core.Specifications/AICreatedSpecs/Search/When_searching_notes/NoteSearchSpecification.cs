using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

/// <summary>
/// A collection of four notes, one or more of every type, with tags and studying states, for specifying which notes a search finds:
/// <list type="bullet">
/// <item>the sentence 明日学校に行く, "I'll go to school tomorrow", tagged <c>source::anime::natsume::s3::01</c>, whose reading is studied;</item>
/// <item>the sentence 家に帰る, "Go home", tagged <c>source::anime::natsume::s3::02</c> and <c>-::priority::low</c>;</item>
/// <item>the vocab 行く, "to go", tagged <c>source::wani</c>;</item>
/// <item>the kanji 行, "go", with no tags.</item>
/// </list>
/// </summary>
public abstract class NoteSearchSpecification : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected const string SentenceAboutSchool = "明日学校に行く";
   protected const string SentenceAboutHome = "家に帰る";
   protected const string VocabToGo = "行く";
   protected const string KanjiGo = "行";

   protected NoteSearchSpecification()
   {
      var sentenceAboutSchool = CreateTestSentence(SentenceAboutSchool, "I'll go to school tomorrow");
      sentenceAboutSchool.Tags.Set(Tag.FromName("source::anime::natsume::s3::01"));
      sentenceAboutSchool.SetStudyingStatus(new CardStudyingStatus(0, CardTypes.Reading, isSuspended: false, NoteTypes.Sentence));

      var sentenceAboutHome = CreateTestSentence(SentenceAboutHome, "Go home");
      sentenceAboutHome.Tags.Set(Tag.FromName("source::anime::natsume::s3::02"));
      sentenceAboutHome.Tags.Set(Tag.FromName("-::priority::low"));

      CreateVocab(VocabToGo, "to go", "いく").Tags.Set(Tag.FromName("source::wani"));

      CreateKanji(KanjiGo, "go", "コウ", "い");
   }

   /// <summary>The questions of the notes <paramref name="search"/> finds, in ordinal order, so specs can compare them with a list.</summary>
   protected List<string> QuestionsOfTheNotesFoundBy(NoteCondition search) =>
      [..GetService<JPCollection>().NotesMatching(search).Select(it => it.GetQuestion()).Order(StringComparer.Ordinal)];

   /// <summary><paramref name="questions"/> in ordinal order, the order <see cref="QuestionsOfTheNotesFoundBy"/> returns.</summary>
   protected static List<string> InOrdinalOrder(params string[] questions) => [..questions.Order(StringComparer.Ordinal)];
}
