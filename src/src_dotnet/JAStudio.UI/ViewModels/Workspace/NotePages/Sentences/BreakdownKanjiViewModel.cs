using JAStudio.Core.ViewModels.KanjiList;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

/// <summary>
/// A kanji of a word in a sentence's breakdown, under the word's entry, when the breakdown shows the kanji (<see cref="KanjiViewModel"/>): the kanji,<br/>
/// linked to its note, with its answer and readings, and its mnemonic when the breakdown shows those too. A right-click on it gives the kanji's menu.
/// </summary>
sealed class BreakdownKanjiViewModel : INotePageElement
{
   public BreakdownKanjiViewModel(KanjiViewModel kanji, bool showsItsMnemonic)
   {
      Kanji = new NoteLinkViewModel(kanji.Question(), kanji.Kanji.GetId());
      Answer = kanji.Answer();
      Readings = kanji.Readings();
      Mnemonic = showsItsMnemonic && kanji.Mnemonic() is { Length: > 0 } mnemonic ? mnemonic : null;
      IsBeingStudied = kanji.Kanji.GetMetaTags().Contains("is_studying");
   }

   public NoteLinkViewModel Kanji { get; }

   /// <summary>The kanji's answer, with its markup.</summary>
   public string Answer { get; }

   /// <summary>The kanji's readings, with their markup: the on readings, a bar, and the kun readings, the primary ones underlined.</summary>
   public string Readings { get; }

   /// <summary>The kanji's mnemonic, with its markup, when the breakdown shows the kanji's mnemonics and it has one.</summary>
   public string? Mnemonic { get; }

   /// <summary>Whether the kanji is being studied, which its border shows.</summary>
   public bool IsBeingStudied { get; }
}
