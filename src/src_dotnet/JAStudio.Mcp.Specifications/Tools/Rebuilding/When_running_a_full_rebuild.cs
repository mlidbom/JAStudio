using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.Mcp.Tools.Rebuilding;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Rebuilding;

/// <summary>
/// A collection of the kanji 行 and the vocab 行く, which has 行 in its main form. Tagging the kanji with what the corpus says about it is part of<br/>
/// the full rebuild, so before it, 行 isn't tagged as being in a vocab's main form.
/// </summary>
public class When_running_a_full_rebuild : CorpusRepositorySpecification
{
   readonly KanjiNote _kanji行;
   readonly bool _theKanjiWasTaggedAsInAVocabsMainFormBefore;
   readonly FullRebuildResult _result;

   public When_running_a_full_rebuild()
   {
      _kanji行 = CreateKanji("行", "go", "コウ", "い");
      CreateVocab("行く", "to go", "いく");
      _theKanjiWasTaggedAsInAVocabsMainFormBefore = _kanji行.Tags.Contains(Tags.Kanji.InVocabMainForm);

      _result = ResultOf<FullRebuildResult>(CallTool("full_rebuild", new Dictionary<string, object?>()));
   }

   [XF] public void the_kanji_was_not_tagged_as_in_a_vocabs_main_form_before() => _theKanjiWasTaggedAsInAVocabsMainFormBefore.Must().BeFalse();
   [XF] public void the_kanji_is_tagged_as_in_a_vocabs_main_form() => _kanji行.Tags.Contains(Tags.Kanji.InVocabMainForm).Must().BeTrue();
   [XF] public void it_reports_how_long_it_took() => _result.SecondsTaken.Must().BeGreaterThan(0);
}
