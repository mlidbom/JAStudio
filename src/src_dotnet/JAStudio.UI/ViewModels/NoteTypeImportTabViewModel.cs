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

   List<NoteMediaFieldImportState> _noteMediaFieldImportStates = [];

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

   [RelayCommand]
   void AddRule()
   {
      var rule = new EditableImportRule();
      rule.RemoveSelfCommand = new RelayCommand(() => RemoveRule(rule));
      Rules.Add(rule);
      SortRules();
      Rebuild();
   }

   [RelayCommand]
   void RemoveRule(EditableImportRule rule)
   {
      Rules.Remove(rule);
      Rebuild();
   }

   internal void SetImportState(List<NoteMediaFieldImportState> scans)
   {
      _noteMediaFieldImportStates = scans;
      Rebuild();
   }

   public void Rebuild()
   {
      var rules = _buildRules(Rules.ToList());
      foreach(var rule in Rules) rule.MatchCount = 0;

      var unmapped = new Dictionary<(string Source, string Field), int>();
      var totalMapped = 0;

      foreach(var scan in _noteMediaFieldImportStates)
      {
         scan.MatchingRule = rules.TryResolve(scan.SourceTag, scan.FieldName);

         if(scan.IndexedAttachment != null) continue; // already in JAStudio index, not relevant to rule mapping

         if(scan.MatchingRule != null)
         {
            var editableRule = FindMatchingEditableRule(scan.MatchingRule);
            if(editableRule != null) editableRule.MatchCount++;
            totalMapped++;
         }
         else
         {
            var key = (scan.SourceTag.ToString(), scan.FieldName);
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

record UnmappedMediaGroup(string SourcePrefix, string FieldName, int FileCount);


