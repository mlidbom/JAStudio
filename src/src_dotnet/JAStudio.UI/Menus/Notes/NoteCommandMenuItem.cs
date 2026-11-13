using System;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Commands;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Commands;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;
using JAStudio.UI.Views.Workspace;

namespace JAStudio.UI.Menus.Notes;

/// <summary>
/// Menu items that run one of the commands of a note's menus (<see cref="INoteCommand{TNote}"/>) on a note, such as accepting a vocab's meaning or<br/>
/// creating its て-form: the note menus' items for the commands are these.
/// </summary>
/// <remarks>
/// Most are labeled with the command's <see cref="INoteCommand{TNote}.Label"/>, given a keyboard shortcut by a <c>shortcutFinger</c>, such as<br/>
/// <see cref="ShortcutFinger.Home1"/>. Those whose menu shows more, such as a preview of the vocab a command would create, take the label whole.
/// </remarks>
static class NoteCommandMenuItem
{
   /// <summary>A menu item that runs <paramref name="command"/> on <paramref name="note"/>, its label given a shortcut by <paramref name="shortcutFinger"/>.</summary>
   public static SpecMenuItem Running<TNote>(Func<string, string> shortcutFinger, NoteChangingCommand<TNote> command, TNote note, bool enabled = true) where TNote : JPNote =>
      SpecMenuItem.UICommand(shortcutFinger(command.Label), () => command.Run(note), enabled: enabled);

   /// <summary>A menu item that runs <paramref name="command"/> on <paramref name="note"/> with <paramref name="text"/>, its label given a shortcut by <paramref name="shortcutFinger"/>.</summary>
   public static SpecMenuItem Running<TNote>(Func<string, string> shortcutFinger, NoteChangingCommandTakingAText<TNote> command, TNote note, string text, bool enabled = true) where TNote : JPNote =>
      SpecMenuItem.UICommand(shortcutFinger(command.Label), () => command.Run(note, text), enabled: enabled);

   /// <summary>
   /// A menu item that creates a vocab from <paramref name="vocab"/> with <paramref name="command"/> and opens it in the workspace, its label given a<br/>
   /// shortcut by <paramref name="shortcutFinger"/>.
   /// </summary>
   public static SpecMenuItem Creating(Func<string, string> shortcutFinger, VocabCreatingCommand command, VocabNote vocab) =>
      Creating(shortcutFinger(command.Label), command, vocab);

   /// <summary>A menu item labeled <paramref name="label"/> that creates a vocab from <paramref name="vocab"/> with <paramref name="command"/> and opens it in the workspace.</summary>
   public static SpecMenuItem Creating(string label, VocabCreatingCommand command, VocabNote vocab) =>
      OpeningTheVocab(label, () => command.CreateFrom(vocab));

   /// <summary>
   /// A menu item labeled <paramref name="label"/> that creates a vocab from <paramref name="vocab"/> with <paramref name="command"/> and <paramref name="text"/>,<br/>
   /// and opens it in the workspace.
   /// </summary>
   public static SpecMenuItem Creating(string label, VocabCreatingCommandTakingAText command, VocabNote vocab, string text) =>
      OpeningTheVocab(label, () => command.CreateFrom(vocab, text));

   /// <summary>
   /// A menu item labeled <paramref name="label"/> that opens the vocab <paramref name="createVocab"/> gives in the workspace: the one it created, or the one<br/>
   /// that had the question already, as no two vocab may share one, which a dialog then says (<see cref="WorkspaceWindow.OpenTheVocab"/>).
   /// </summary>
   static SpecMenuItem OpeningTheVocab(string label, Func<CreatedOrExistingVocab> createVocab) =>
      SpecMenuItem.Command(label, () => WorkspaceWindow.OpenTheVocab(createVocab()));
}
