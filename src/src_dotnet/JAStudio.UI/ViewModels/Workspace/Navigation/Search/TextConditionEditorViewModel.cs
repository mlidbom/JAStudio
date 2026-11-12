using System.Collections.Generic;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.TextProperties;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>The editor for a condition on a <see cref="TextProperty"/>: how to compare (<see cref="TextMatchChoice"/>), and the text to compare with.</summary>
partial class TextConditionEditorViewModel : ConditionEditorViewModel
{
   readonly TextProperty _property;
   NoteCondition? _condition;

   public TextConditionEditorViewModel(TextProperty property, TextMatchChoice matchChoice, string text) : base(property)
   {
      _property = property;
      MatchChoice = matchChoice;
      Text = text;
   }

   /// <summary>Every way the text can be compared, for the editor to offer.</summary>
   public IReadOnlyList<TextMatchChoice> MatchChoices { get; } = TextMatchChoice.All;

   /// <summary>How the text is compared.</summary>
   [ObservableProperty] public partial TextMatchChoice MatchChoice { get; set; } = TextMatchChoice.Contains;

   /// <summary>The text, or pattern, to compare with.</summary>
   [ObservableProperty] public partial string Text { get; set; } = "";

   /// <summary>Why the inputs don't describe a condition, such as a regular expression that isn't valid. Null when there's no problem to show.</summary>
   [ObservableProperty] public partial string? Problem { get; set; }

   public override NoteCondition? Condition => _condition;

   partial void OnMatchChoiceChanged(TextMatchChoice value) => UpdateTheCondition(value, Text);

   partial void OnTextChanged(string value) => UpdateTheCondition(MatchChoice, value);

   void UpdateTheCondition(TextMatchChoice matchChoice, string text)
   {
      Problem = null;
      _condition = text.Length == 0 ? null : TheConditionTheInputsDescribe(matchChoice, text);
      OnConditionChanged();
   }

   /// <remarks>An invalid regular expression is something one types on the way to a valid one, so it's shown as a problem, not thrown.</remarks>
   TextCondition? TheConditionTheInputsDescribe(TextMatchChoice matchChoice, string text)
   {
      try
      {
         return matchChoice.MakeCondition(_property, text);
      }
      catch(RegexParseException exception)
      {
         Problem = exception.Message;
         return null;
      }
   }
}
