using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
namespace JAStudio.Core.Storage.Media;

public class ImportRule(SourceTag prefix, string fieldName, string targetDirectory)
{
   public SourceTag Prefix { get; } = prefix;
   [JsonPropertyName("field")]
   public string FieldName { get; } = fieldName;
   public string TargetDirectory { get; } = targetDirectory;
}

public static class ImportRulesCE
{
   public static ImportRule? TryResolve(this IEnumerable<ImportRule> @this, SourceTag sourceTag, string fieldName) =>
      @this.OrderByDescending(r => r.Prefix.Segments.Count)
           .FirstOrDefault(r => r.FieldName == fieldName && sourceTag.IsContainedIn(r.Prefix));
}
