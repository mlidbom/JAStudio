using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Storage.Media;

namespace JAStudio.UI.ViewModels;

partial class NoteTypeImportTabViewModel : ObservableObject
{
   readonly Func<List<EditableImportRule>, List<ImportRule>> _buildRules;

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
      Func<List<EditableImportRule>, List<ImportRule>> buildRules)
   {
      NoteTypeName = noteTypeName;
      FieldNames = fieldNames;
      _buildRules = buildRules;
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

      var rules = _buildRules(Rules.ToList());
      foreach(var rule in Rules) rule.MatchCount = 0;

      var unmapped = new Dictionary<(string Source, string Field), int>();
      var totalMapped = 0;

      foreach(var file in _allScannedFiles)
      {
         ImportRule? matchingRule = null;
         if(!string.IsNullOrEmpty(file.SourceTag))
         {
            try { matchingRule = rules.TryResolve(SourceTag.Parse(file.SourceTag), file.FieldName); }
            catch { /* skip files with unparseable source tags */ }
         }

         if(matchingRule != null)
         {
            var editableRule = FindMatchingEditableRule(matchingRule);
            if(editableRule != null) editableRule.MatchCount++;
            totalMapped++;
         }
         else
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

   EditableImportRule? FindMatchingEditableRule(ImportRule rule) =>
      Rules.FirstOrDefault(r =>
                              r.IsValid &&
                              r.SourceTagPrefix == rule.Prefix.ToString() &&
                              r.SelectedField == rule.FieldName &&
                              r.TargetDirectory == rule.TargetDirectory);

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

