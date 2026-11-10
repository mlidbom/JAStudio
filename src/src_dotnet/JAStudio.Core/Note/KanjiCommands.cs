using System.Collections.Generic;
using JAStudio.Core.Note.Commands;

namespace JAStudio.Core.Note;

/// <summary>
/// Every command of a kanji's menus (<see cref="INoteCommand{TNote}"/>): accepting its meaning, populating its radicals from its mnemonic, bootstrapping<br/>
/// its mnemonic from its radicals, and adding a kanji with a similar meaning.
/// </summary>
/// <remarks>
/// The kanji's menus are built from these commands, and JAStudio's MCP server runs <see cref="All"/> of them, so the two do the same things.<br/>
/// Each menu item a person runs on a kanji is one of these, other than those that open notes and those that edit a field (<see cref="KanjiEditableFields"/>),<br/>
/// such as its primary vocab or which of its readings are primary.
/// </remarks>
public static class KanjiCommands
{
   public static readonly NoteChangingCommand<KanjiNote> AcceptMeaning = new("acceptMeaning", "Accept meaning", it => it.AcceptMeaning());

   /// <summary>Makes the kanji's radicals those whose answer has a word its mnemonic marks as a radical, with a <c>&lt;rad&gt;</c> tag.</summary>
   public static readonly NoteChangingCommand<KanjiNote> PopulateRadicalsFromMnemonicTags = new("populateRadicalsFromMnemonicTags", "Populate radicals from mnemonic tags", it => it.PopulateRadicalsFromMnemonicTags());

   /// <summary>Makes the kanji's mnemonic the default one, made from its radicals.</summary>
   public static readonly NoteChangingCommand<KanjiNote> BootstrapMnemonicFromRadicals = new("bootstrapMnemonicFromRadicals", "Bootstrap mnemonic from radicals", it => it.BootstrapMnemonicFromRadicals());

   /// <summary>
   /// Adds a kanji to the kanji's kanji with a similar meaning, and the kanji to that kanji's, as adding it in the editor does<br/>
   /// (<see cref="KanjiEditableFields.SimilarMeaning"/>): the string menus' quick way to add the text selected.
   /// </summary>
   public static readonly NoteChangingCommandTakingAText<KanjiNote> AddSimilarMeaning = new("addSimilarMeaning", "Similar meaning", "the kanji with a similar meaning", (it, kanji) => it.SimilarMeaning.Add(kanji));

   /// <summary>Every command above, in the same order.</summary>
   public static IReadOnlyList<INoteCommand<KanjiNote>> All { get; } = [AcceptMeaning, PopulateRadicalsFromMnemonicTags, BootstrapMnemonicFromRadicals, AddSimilarMeaning];
}
