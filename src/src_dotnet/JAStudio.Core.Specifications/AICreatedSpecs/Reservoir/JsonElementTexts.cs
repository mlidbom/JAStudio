using System.Text.Json;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>Reading a JSON string value in a spec, to assert on it.</summary>
static class JsonElementTexts
{
   /// <summary><paramref name="element"/>'s string value, or <c>(null)</c> when it's JSON null, so that a spec expecting a text fails saying so.</summary>
   public static string Text(this JsonElement element)
   {
      //todo:compze: Compze.Must's string assertions take no string?, and GetString gives one. Once they do, assert on GetString() and delete this.
      return element.GetString() ?? "(null)";
   }
}
