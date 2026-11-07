using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;

public class VocabSpec : IEquatable<VocabSpec>
{
   public VocabSpec(
      string question,
      string? answer = null,
      List<string>? readings = null,
      List<string>? forms = null,
      List<Tag>? tags = null,
      List<string>? compounds = null,
      IEnumerable<string>? surfaceNot = null,
      IEnumerable<string>? yieldToSurface = null,
      IEnumerable<string>? prefixIn = null,
      IEnumerable<string>? prefixNot = null,
      IEnumerable<string>? suffixNot = null,
      IEnumerable<string>? tos = null)
   {
      DisambiguationName = question;
      Question = question.Split(VocabNoteQuestion.DisambiguationMarker)[0];
      Answer = answer ?? question;
      Readings = readings ?? [Question];
      ExtraForms = forms != null ? [..forms] : [];
      Tags = tags != null ? [..tags] : [];
      Compounds = compounds ?? [];
      SurfaceIsNot = surfaceNot != null ? [..surfaceNot] : [];
      YieldToSurface = yieldToSurface != null ? [..yieldToSurface] : [];
      PrefixIsNot = prefixNot != null ? [..prefixNot] : [];
      SuffixIsNot = suffixNot != null ? [..suffixNot] : [];
      Tos = tos != null ? [..tos] : [];
      RequiredPrefix = prefixIn != null ? [..prefixIn] : [];
   }

   public string DisambiguationName { get; }
   public string Question { get; }
   public string Answer { get; }
   public List<string> Readings { get; }
   public HashSet<string> ExtraForms { get; }
   public HashSet<Tag> Tags { get; }
   public List<string> Compounds { get; }
   public HashSet<string> SurfaceIsNot { get; }
   public HashSet<string> YieldToSurface { get; }
   public HashSet<string> PrefixIsNot { get; }
   public HashSet<string> SuffixIsNot { get; }
   public HashSet<string> Tos { get; }
   public HashSet<string> RequiredPrefix { get; }

   public override string ToString() => $"""VocabSpec("{Question}", "{Answer}", [{string.Join(", ", Readings.Select(r => $"\"{r}\""))}])""";

   public override bool Equals(object? obj) => Equals(obj as VocabSpec);

   public bool Equals(VocabSpec? other) =>
      other is not null
   && other.Question == Question
   && other.Answer == Answer
   && other.Readings.SequenceEqual(Readings);

   public override int GetHashCode() => Question.GetHashCode();

   public void InitializeNote(VocabNote vocabNote)
   {
      vocabNote.CompoundParts.Set(Compounds);

      if(ExtraForms.Count > 0)
      {
         var enteredForms = new HashSet<string>(vocabNote.Forms.EnteredFormsSet());
         enteredForms.UnionWith(ExtraForms);
         vocabNote.Forms.SetEnteredForms(enteredForms);
      }

      foreach(var tag in Tags)
      {
         vocabNote.Tags.Set(tag);
      }

      foreach(var excludedSurface in SurfaceIsNot)
      {
         vocabNote.MatchingConfiguration.ConfigurableRules.SurfaceIsNot.Add(excludedSurface);
      }

      foreach(var yieldSurface in YieldToSurface)
      {
         vocabNote.MatchingConfiguration.ConfigurableRules.YieldToSurface.Add(yieldSurface);
      }

      foreach(var forbiddenPrefix in PrefixIsNot)
      {
         vocabNote.MatchingConfiguration.ConfigurableRules.PrefixIsNot.Add(forbiddenPrefix);
      }

      foreach(var forbiddenSuffix in SuffixIsNot)
      {
         vocabNote.MatchingConfiguration.ConfigurableRules.SuffixIsNot.Add(forbiddenSuffix);
      }

      foreach(var requiredPrefix in RequiredPrefix)
      {
         vocabNote.MatchingConfiguration.ConfigurableRules.RequiredPrefix.Add(requiredPrefix);
      }

      vocabNote.MatchingConfiguration.ConfigurableRules.Save();

      if(Tos.Count > 0)
      {
         vocabNote.PartsOfSpeech.Set(Tos);
      }
   }
}
