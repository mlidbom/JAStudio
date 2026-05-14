using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Sentences;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.Utils;

namespace JAStudio.UI.Menus.Notes.Sentence;

/// <summary>
/// Sentence string menu builder - builds context menus for selected text within sentence notes.
/// Corresponds to notes/sentence/string_menu.py in Python.
/// </summary>
static class SentenceStringMenus
{
   public static SpecMenuItem BuildStringMenuSpec(SentenceNote sentence, string menuString) =>
      SpecMenuItem.Submenu(
         $"Selection: \"{menuString}\"",
         new List<SpecMenuItem>
         {
            BuildAddMenuSpec(sentence, menuString),
            BuildRemoveMenuSpec(sentence, menuString),
            BuildSplitWithWordBreakTagSpec(sentence, menuString)
         }
      );

   static SpecMenuItem BuildAddMenuSpec(SentenceNote sentence, string menuString)
   {
      void AddAddWordExclusionAction(List<SpecMenuItem> items, string exclusionTypeTitle, WordExclusionSet exclusionSet)
      {
         var menuStringAsWordExclusion = WordExclusion.Global(menuString);
         var analysis = sentence.CreateAnalysis();
         var displayMatches = analysis.DisplayMatches;
         var matchesExcludedByMenuString = displayMatches
                                          .Where(match => menuStringAsWordExclusion.ExcludesFormAtIndex(match.ParsedForm, match.StartIndex))
                                          .ToList();

         if(matchesExcludedByMenuString.Any())
         {
            if(matchesExcludedByMenuString.Count == 1)
            {
               var match = matchesExcludedByMenuString[0];
               items.Add(SpecMenuItem.Command(
                            exclusionTypeTitle,
                            () => exclusionSet.Add(match.ToExclusion())
                         ));
            } else
            {
               var subItems = new List<SpecMenuItem>();
               for(var i = 0; i < matchesExcludedByMenuString.Count; i++)
               {
                  var match = matchesExcludedByMenuString[i];
                  var index = i; // Capture for lambda
                  subItems.Add(SpecMenuItem.Command(
                                  ShortcutFinger.FingerByPriorityOrder(index, $"{match.StartIndex}: {match.ParsedForm}"),
                                  () => exclusionSet.Add(match.ToExclusion())
                               ));
               }

               items.Add(SpecMenuItem.Submenu(exclusionTypeTitle, subItems));
            }
         } else
         {
            items.Add(SpecMenuItem.Command(exclusionTypeTitle, () => {}, enabled: false));
         }
      }

      var items = new List<SpecMenuItem>();

      var isAlreadyHighlighted = sentence.Configuration.HighlightedWords.Contains(menuString);
      items.Add(SpecMenuItem.Command(ShortcutFinger.Home1("Highlighted Vocab"), () => sentence.Configuration.AddHighlightedWord(menuString), enabled: !isAlreadyHighlighted));

      AddAddWordExclusionAction(items, ShortcutFinger.Home2("Hidden matches"), sentence.Configuration.HiddenMatches);
      AddAddWordExclusionAction(items, ShortcutFinger.Home3("Incorrect matches"), sentence.Configuration.IncorrectMatches);

      return SpecMenuItem.Submenu(ShortcutFinger.Home1("Add"), items);
   }

   static SpecMenuItem BuildRemoveMenuSpec(SentenceNote sentence, string menuString)
   {
      void AddRemoveWordExclusionAction(List<SpecMenuItem> items, string exclusionTypeTitle, WordExclusionSet exclusionSet)
      {
         var menuStringAsWordExclusion = WordExclusion.Global(menuString);
         var currentExclusions = exclusionSet.Get().ToList();
         var coveredExistingExclusions = currentExclusions
                                        .Where(excl => menuStringAsWordExclusion.ExcludesAllWordsExcludedBy(excl))
                                        .ToList();

         if(coveredExistingExclusions.Any())
         {
            if(coveredExistingExclusions.Count == 1)
            {
               items.Add(SpecMenuItem.Command(
                            exclusionTypeTitle,
                            () => exclusionSet.RemoveString(menuString)
                         ));
            } else
            {
               var subItems = new List<SpecMenuItem>();
               for(var i = 0; i < coveredExistingExclusions.Count; i++)
               {
                  var exclusion = coveredExistingExclusions[i];
                  var index = i; // Capture for lambda
                  subItems.Add(SpecMenuItem.Command(
                                  ShortcutFinger.FingerByPriorityOrder(index, $"{exclusion.Index}:{exclusion.Word}"),
                                  () => exclusionSet.Remove(exclusion)
                               ));
               }

               items.Add(SpecMenuItem.Submenu(exclusionTypeTitle, subItems));
            }
         } else
         {
            items.Add(SpecMenuItem.Command(exclusionTypeTitle, () => {}, enabled: false));
         }
      }

      var items = new List<SpecMenuItem>();

      var isHighlighted = sentence.Configuration.HighlightedWords.Contains(menuString);
      items.Add(SpecMenuItem.Command(ShortcutFinger.Home1("Highlighted vocab"), () => sentence.Configuration.RemoveHighlightedWord(menuString), enabled: isHighlighted));

      AddRemoveWordExclusionAction(items, ShortcutFinger.Home2("Hidden matches"), sentence.Configuration.HiddenMatches);
      AddRemoveWordExclusionAction(items, ShortcutFinger.Home3("Incorrect matches"), sentence.Configuration.IncorrectMatches);

      return SpecMenuItem.Submenu(ShortcutFinger.Home2("Remove"), items);
   }

   static SpecMenuItem BuildSplitWithWordBreakTagSpec(SentenceNote sentence, string menuString)
   {
      var questionText = sentence.Question.WithInvisibleSpace();
      var canSplit = questionText.Contains(menuString);

      return SpecMenuItem.Command(ShortcutFinger.Home3("Split with word-break tag in question"), () => sentence.Question.SplitTokenWithWordBreakTag(menuString), enabled: canSplit);
   }
}
