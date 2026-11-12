using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// The editor for the texts of a condition in a vocab's match-condition, such as the endings of <c>EndsWithOneOf</c>, shown in a<br/>
/// <see cref="ConditionTextsDialog"/>: for a new condition, or one being changed. Its <see cref="ConditionTestWithTexts"/> makes the condition from<br/>
/// what's typed.
/// </summary>
partial class ConditionTextsEditorViewModel : ObservableObject
{
   readonly ConditionTestWithTexts _test;
   readonly Func<Condition, Condition> _placed;

   /// <param name="title">The <see cref="Title"/>.</param>
   /// <param name="test">Makes the condition from what's typed.</param>
   /// <param name="placed">The condition as it goes into the match-condition, such as on the subject a new condition was picked for.</param>
   /// <param name="typedText">What's typed to start with: nothing for a new condition, or what makes the condition being changed.</param>
   ConditionTextsEditorViewModel(string title, ConditionTestWithTexts test, Func<Condition, Condition> placed, string typedText)
   {
      Title = title;
      _test = test;
      _placed = placed;
      PickableValues = [.. test.PickableValues.Select(value => new PickableValueViewModel(value, ConditionTestWithTexts.TypedTextPicks(typedText, value), PickedOrUnpicked))];
      Text = typedText;
   }

   /// <summary>
   /// An editor for a new condition of <paramref name="test"/>, picked in the add menu, with nothing typed yet. <paramref name="placed"/> puts the<br/>
   /// condition on the subjects picked on the way to <paramref name="test"/>, such as the preceding token's surface.
   /// </summary>
   public static ConditionTextsEditorViewModel ForANewCondition(string title, ConditionTestWithTexts test, Func<Condition, Condition> placed) =>
      new(title, test, placed, typedText: "");

   /// <summary>An editor filled in with the texts of <paramref name="condition"/>, a condition of <paramref name="test"/>, to change it in its place.</summary>
   public static ConditionTextsEditorViewModel ForChanging(Condition condition, ConditionTestWithTexts test, string title) =>
      new(title, test, placed: it => it, test.TypedTextOf(condition));

   /// <summary>The dialog's title, the code of the condition being made, such as <c>PrecedingToken.Surface.EndsWithOneOf</c>.</summary>
   public string Title { get; }

   /// <summary>What to type, such as "The texts, separated by spaces".</summary>
   public string HowToType => _test.HowToTypeTheTexts;

   /// <summary>
   /// Whether the condition's texts are picked from the tokenizer's values, such as parts of speech, found by their Japanese or English, rather than<br/>
   /// typed: picking them types them.
   /// </summary>
   public bool PicksValues => PickableValues.Count > 0;

   /// <summary>The tokenizer's values to pick from, each picked or not, in order: none when the texts are typed.</summary>
   public IReadOnlyList<PickableValueViewModel> PickableValues { get; }

   /// <summary>What finds the values shown: text in their Japanese or their English, such as 接尾 or suffix.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(ShownValues))]
   public partial string Filter { get; set; } = "";

   /// <summary>The values the <see cref="Filter"/> finds, in order: every one while it's empty.</summary>
   public IReadOnlyList<PickableValueViewModel> ShownValues => [.. PickableValues.Where(it => it.IsFoundBy(Filter))];

   /// <summary>The texts as typed, or as the values picked type them: separated by spaces.</summary>
   [ObservableProperty] public partial string Text { get; set; } = "";

   /// <summary>Why what's typed makes no condition, such as a part of speech the tokenizer never gives. Null when there's no problem to show.</summary>
   [ObservableProperty] public partial string? Problem { get; set; }

   /// <summary>The condition what's typed makes, placed as it goes into the match-condition, or null while it makes none.</summary>
   public Condition? Condition { get; private set; }

   /// <summary>Whether the dialog can be accepted: what's typed makes a <see cref="Condition"/>.</summary>
   public bool CanAccept => Condition is not null;

   /// <summary>Accepts the editor's <see cref="Condition"/>. Set by the <see cref="ConditionTextsDialog"/>, which closes with it.</summary>
   public IRelayCommand? AcceptCommand { get; set; }

   /// <summary>Gives up on the condition. Set by the <see cref="ConditionTextsDialog"/>, which closes without one.</summary>
   public IRelayCommand? CancelCommand { get; set; }

   /// <summary>
   /// Types the values picked, after <paramref name="value"/> was picked or unpicked, as <paramref name="isPicked"/> says: for a test that picks only one,<br/>
   /// such as a part of speech, picking one unpicks the one picked before.
   /// </summary>
   void PickedOrUnpicked(PickableValueViewModel value, bool isPicked)
   {
      if(_test.PicksOne && isPicked)
      {
         UnpickAllBut(value);
      }

      Text = ConditionTestWithTexts.TypedTextPicking(PickableValues.Where(it => it.IsPicked).Select(it => it.TokenizerValue));
      return;

      void UnpickAllBut(PickableValueViewModel picked)
      {
         foreach(var other in PickableValues.Where(it => it != picked && it.IsPicked))
         {
            other.IsPicked = false;
         }
      }
   }

   partial void OnTextChanged(string value)
   {
      (Condition, Problem) = TheConditionTypedOrWhyThereIsNone();
      OnPropertyChanged(nameof(Condition));
      OnPropertyChanged(nameof(CanAccept));
      return;

      (Condition? Condition, string? Problem) TheConditionTypedOrWhyThereIsNone() =>
         string.IsNullOrWhiteSpace(value)                    ? (null, null)
         : _test.WhyItMakesNoConditionFrom(value) is {} why ? (null, why)
                                                             : (_placed(_test.MadeFrom(value)), null);
   }
}
