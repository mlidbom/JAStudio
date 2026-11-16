using System;
using System.Collections.Generic;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences.ParsingCases;

/// <summary>
/// One row of a sentence's breakdown shown beside the one its parsing case expects (<see cref="BreakdownBesideTheExpected.Row"/>): the word the<br/>
/// sentence displays, as the breakdown lists it, the word expected, or both, and how what is displayed differs from what is expected.
/// </summary>
/// <param name="Displayed">The word the sentence displays, without its compound parts and kanji. Null in the row of a word that is expected and not displayed.</param>
/// <param name="Expected">The word the parsing case expects. Null in the row of a word that is displayed and not expected.</param>
/// <param name="Differences">How what is displayed differs from what is expected, a line per difference (<see cref="BreakdownBesideTheExpected.Row.Differences"/>).</param>
sealed record BreakdownRowBesideTheExpectedViewModel(BreakdownEntryViewModel? Displayed, ExpectedBreakdownEntryViewModel? Expected, IReadOnlyList<string> Differences)
{
   /// <summary>Whether what is displayed differs from what is expected, which the page marks on the side of what is displayed.</summary>
   public bool Differs => Differences.Count > 0;

   /// <summary>The <see cref="Differences"/>, a line each, which the side of what is displayed shows on hover. Null when the row doesn't differ.</summary>
   public string? WhatDiffers => Differs ? string.Join(Environment.NewLine, Differences) : null;
}
