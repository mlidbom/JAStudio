using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Storage.Media;

namespace JAStudio.UI.ViewModels;

partial class NoteTypeImportTabViewModel<TRule> : ObservableObject where TRule : class
{
   readonly Func<List<EditableImportRule>, List<TRule>> _buildRules;
   readonly Func<SourceTag, string, TRule?> _tryResolve;

   // All un-imported media references discovered by scanning, before rule classification
   List<ScannedMediaFile> _allScannedFiles = [];

#pragma warning disable CS8618
   [Obsolete("Parameterless constructor is only for XAML designer support and should not be used directly.")]
   // ReSharper disable once UnusedMember.Global
   public NoteTypeImportTabViewModel() {}
#pragma warning restore CS8618

   public NoteTypeImportTabViewModel(
      string noteTypeName,
      List<string> fieldNames,
      Func<List<EditableImportRule>, List<TRule>> buildRules,
      Func<SourceTag, string, TRule?> tryResolve)
   {
      NoteTypeName = noteTypeName;
      FieldNames = fieldNames;
      _buildRules = buildRules;
      _tryResolve = tryResolve;
   }

   public string NoteTypeName { get; private set; } = "";
   public List<string> FieldNames { get; }

   public ObservableCollection<EditableImportRule> Rules { get; } = [];
   public ObservableCollection<UnmappedMediaGroup> UnmappedGroups { get; } = [];

   [ObservableProperty] int _totalUnmappedCount;
   [ObservableProperty] int _totalMappedCount;
   [ObservableProperty] bool _hasScanned;

   [RelayCommand]
   void AddRule()
   {
      var rule = new EditableImportRule();
      rule.RemoveSelfCommand = new RelayCommand(() => RemoveRule(rule));
      Rules.Add(rule);
      SortRules();
      Reclassify();
   }

   [RelayCommand]
   void RemoveRule(EditableImportRule rule)
   {
      Rules.Remove(rule);
      Reclassify();
   }

   internal void SetScannedFiles(List<ScannedMediaFile> files)
   {
      _allScannedFiles = files;
      HasScanned = true;
      Reclassify();
   }

   public void Reclassify()
   {
      if(!HasScanned) return;

      var validRules = _buildRules(Rules.ToList());

      // Reset all rule match counts
      foreach(var rule in Rules) rule.MatchCount = 0;

      var unmapped = new Dictionary<(string Source, string Field), int>();
      var totalMapped = 0;

      foreach(var file in _allScannedFiles)
      {
         var matchingDomainRule = TryResolveFile(file, validRules);
         if(matchingDomainRule != null)
         {
            // Find the EditableImportRule that corresponds and increment its count
            var editableRule = FindMatchingEditableRule(matchingDomainRule);
            if(editableRule != null) editableRule.MatchCount++;
            totalMapped++;
         } else
         {
            var key = (file.SourceTag, file.FieldName);
            unmapped[key] = unmapped.GetValueOrDefault(key) + 1;
         }
      }

      TotalMappedCount = totalMapped;

      UnmappedGroups.Clear();
      foreach(var kvp in unmapped.OrderByDescending(kvp => kvp.Value))
         UnmappedGroups.Add(new UnmappedMediaGroup(kvp.Key.Source, kvp.Key.Field, kvp.Value));

      TotalUnmappedCount = UnmappedGroups.Sum(g => g.FileCount);
   }

   TRule? TryResolveFile(ScannedMediaFile file, List<TRule> validRules)
   {
      if(validRules.Count == 0) return null;
      if(string.IsNullOrEmpty(file.SourceTag)) return null;

      try
      {
         var sourceTag = SourceTag.Parse(file.SourceTag);
         return _tryResolve(sourceTag, file.FieldName);
      }
      catch
      {
         return null;
      }
   }

   EditableImportRule? FindMatchingEditableRule(TRule domainRule)
   {
      // Match by comparing the domain rule's properties back to the editable rule
      return domainRule switch
      {
         VocabImportRule vr => Rules.FirstOrDefault(r =>
                                                       r.IsValid &&
                                                       r.SourceTagPrefix == vr.Prefix.ToString() &&
                                                       r.SelectedField == vr.Field.ToString() &&
                                                       r.TargetDirectory == vr.TargetDirectory),
         SentenceImportRule sr => Rules.FirstOrDefault(r =>
                                                          r.IsValid &&
                                                          r.SourceTagPrefix == sr.Prefix.ToString() &&
                                                          r.SelectedField == sr.Field.ToString() &&
                                                          r.TargetDirectory == sr.TargetDirectory),
         KanjiImportRule kr => Rules.FirstOrDefault(r =>
                                                       r.IsValid &&
                                                       r.SourceTagPrefix == kr.Prefix.ToString() &&
                                                       r.SelectedField == kr.Field.ToString() &&
                                                       r.TargetDirectory == kr.TargetDirectory),
         _ => null
      };
   }

   internal void LoadRules(IEnumerable<EditableImportRule> rules)
   {
      foreach(var r in rules)
      {
         r.RemoveSelfCommand = new RelayCommand(() => RemoveRule(r));
         Rules.Add(r);
      }

      SortRules();
   }

   void SortRules()
   {
      var sorted = Rules.OrderBy(r => r.SourceTagPrefix, StringComparer.Ordinal)
                        .ThenBy(r => r.TargetDirectory, StringComparer.Ordinal)
                        .ToList();

      for(var i = 0; i < sorted.Count; i++)
      {
         var currentIndex = Rules.IndexOf(sorted[i]);
         if(currentIndex != i) Rules.Move(currentIndex, i);
      }
   }
}

record ScannedMediaFile(string SourceTag, string FieldName, string FileName);
record UnmappedMediaGroup(string SourcePrefix, string FieldName, int FileCount);
