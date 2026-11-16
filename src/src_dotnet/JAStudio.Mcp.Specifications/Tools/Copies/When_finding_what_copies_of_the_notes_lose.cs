using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Tools.Copies;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Copies;

/// <summary>A collection with the vocab 行く, the kanji 行 and a sentence, every note then copied through its snapshot and compared with its copy.</summary>
public class When_finding_what_copies_of_the_notes_lose : McpServerSpecification
{
   readonly WhatCopiesLose _result;

   public When_finding_what_copies_of_the_notes_lose()
   {
      CreateVocab("行く", "to go", "いく");
      CreateKanji("行", "go", "コウ", "い");
      CreateTestSentence("学校に行く", "go to school");

      _result = ResultOf<WhatCopiesLose>(CallTool("find_what_copies_of_the_notes_lose", new Dictionary<string, object?>()));
   }

   [XF] public void it_checks_every_note() => _result.NotesChecked.Must().Be(3);
   [XF] public void it_reports_how_long_it_took() => _result.SecondsTaken.Must().BeGreaterThanOrEqualTo(0);
}
