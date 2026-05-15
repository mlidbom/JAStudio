using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace JAStudio.Core.SysUtils.Json;

/// <summary>
/// JSON reader for deserializing JSON data. Mirrors the Python JsonReader class.
/// </summary>
public class JsonReader
{
   readonly JsonElement _element;

   public JsonReader(JsonElement element) => _element = element;

   JsonElement GetProperty(string prop, JsonElement? defaultValue = null)
   {
      if(_element.TryGetProperty(prop, out var property))
      {
         return property;
      }

      if(defaultValue.HasValue)
      {
         return defaultValue.Value;
      }

      throw new KeyNotFoundException($"Property '{prop}' not found in the JSON.");
   }

   JsonElement GetProperty(IEnumerable<string> props, JsonElement? defaultValue = null)
   {
      foreach(var prop in props)
      {
         if(_element.TryGetProperty(prop, out var property))
         {
            return property;
         }
      }

      if(defaultValue.HasValue)
      {
         return defaultValue.Value;
      }

      throw new KeyNotFoundException($"None of the following keys were found in the JSON: {string.Join(", ", props)}");
   }

   public string GetString(string key, string? defaultValue = null)
   {
      try
      {
         var prop = GetProperty(key, defaultValue != null ? JsonDocument.Parse($"\"{defaultValue}\"").RootElement : null);
         return prop.ValueKind == JsonValueKind.String ? prop.GetString() ?? "" : "";
      }
      catch(KeyNotFoundException)
      {
         if(defaultValue != null) return defaultValue;
         throw;
      }
   }

   public string GetString(IEnumerable<string> keys, string? defaultValue = null)
   {
      try
      {
         var prop = GetProperty(keys, defaultValue != null ? JsonDocument.Parse($"\"{defaultValue}\"").RootElement : null);
         return prop.ValueKind == JsonValueKind.String ? prop.GetString() ?? "" : "";
      }
      catch(KeyNotFoundException)
      {
         if(defaultValue != null) return defaultValue;
         throw;
      }
   }

   public int GetInt(string key, int? defaultValue = null)
   {
      try
      {
         var prop = GetProperty(key, defaultValue.HasValue ? JsonDocument.Parse(defaultValue.Value.ToString()).RootElement : null);
         if(prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var intValue))
         {
            return intValue;
         }

         return 0;
      }
      catch(KeyNotFoundException)
      {
         if(defaultValue.HasValue) return defaultValue.Value;
         throw;
      }
   }

   public int GetInt(IEnumerable<string> keys, int? defaultValue = null)
   {
      try
      {
         var prop = GetProperty(keys, defaultValue.HasValue ? JsonDocument.Parse(defaultValue.Value.ToString()).RootElement : null);
         if(prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var intValue))
         {
            return intValue;
         }

         return 0;
      }
      catch(KeyNotFoundException)
      {
         if(defaultValue.HasValue) return defaultValue.Value;
         throw;
      }
   }

   public List<string> GetStringList(string key, List<string>? defaultValue = null)
   {
      try
      {
         var prop = GetProperty(key);
         if(prop.ValueKind == JsonValueKind.Array)
         {
            return prop.EnumerateArray()
                       .Where(item => item.ValueKind == JsonValueKind.String)
                       .Select(item => item.GetString() ?? "")
                       .ToList();
         }

         return defaultValue ?? [];
      }
      catch(KeyNotFoundException)
      {
         if(defaultValue != null) return defaultValue;
         throw;
      }
   }

   public List<string> GetStringList(IEnumerable<string> keys, List<string>? defaultValue = null)
   {
      try
      {
         var prop = GetProperty(keys);
         if(prop.ValueKind == JsonValueKind.Array)
         {
            return prop.EnumerateArray()
                       .Where(item => item.ValueKind == JsonValueKind.String)
                       .Select(item => item.GetString() ?? "")
                       .ToList();
         }

         return defaultValue ?? [];
      }
      catch(KeyNotFoundException)
      {
         if(defaultValue != null) return defaultValue;
         throw;
      }
   }

   public HashSet<string> GetStringSet(string key, List<string>? defaultValue = null) => GetStringList(key, defaultValue).ToHashSet();

   public HashSet<string> GetStringSet(IEnumerable<string> keys, List<string>? defaultValue = null) => GetStringList(keys, defaultValue).ToHashSet();

   public List<T> GetObjectList<T>(string key, Func<JsonReader, T> factory, List<T>? defaultValue = null)
   {
      try
      {
         var prop = GetProperty(key);
         if(prop.ValueKind == JsonValueKind.Array)
         {
            return prop.EnumerateArray()
                       .Where(item => item.ValueKind == JsonValueKind.Object)
                       .Select(item => factory(new JsonReader(item)))
                       .ToList();
         }

         return defaultValue ?? [];
      }
      catch(KeyNotFoundException)
      {
         if(defaultValue != null) return defaultValue;
         throw;
      }
   }

   public List<T> GetObjectList<T>(IEnumerable<string> keys, Func<JsonReader, T> factory, List<T>? defaultValue = null)
   {
      try
      {
         var prop = GetProperty(keys);
         if(prop.ValueKind == JsonValueKind.Array)
         {
            return prop.EnumerateArray()
                       .Where(item => item.ValueKind == JsonValueKind.Object)
                       .Select(item => factory(new JsonReader(item)))
                       .ToList();
         }

         return defaultValue ?? [];
      }
      catch(KeyNotFoundException)
      {
         if(defaultValue != null) return defaultValue;
         throw;
      }
   }

   public T? GetObject<T>(string key, Func<JsonReader, T> factory, Func<T>? defaultFactory = null)
   {
      try
      {
         var prop = GetProperty(key);
         if(prop.ValueKind == JsonValueKind.Object)
         {
            return factory(new JsonReader(prop));
         }

         return defaultFactory != null ? defaultFactory() : default;
      }
      catch(KeyNotFoundException)
      {
         return defaultFactory != null ? defaultFactory() : default;
      }
   }

   public static JsonReader FromJson(string json)
   {
      var doc = JsonDocument.Parse(json);
      return new JsonReader(doc.RootElement);
   }
}

static class JsonHelper
{
   public static Dictionary<string, object> JsonToDict(string json)
   {
      if(string.IsNullOrEmpty(json))
      {
         return new Dictionary<string, object>();
      }

      using var doc = JsonDocument.Parse(json);
      return ElementToDict(doc.RootElement);
   }

   static Dictionary<string, object> ElementToDict(JsonElement element)
   {
      var dict = new Dictionary<string, object>();

      foreach(var property in element.EnumerateObject())
      {
         dict[property.Name] = ConvertElement(property.Value);
      }

      return dict;
   }

   static object ConvertElement(JsonElement element)
   {
      return element.ValueKind switch
      {
         JsonValueKind.String => element.GetString() ?? "",
         JsonValueKind.Number => element.TryGetInt32(out var intValue) ? intValue : element.GetDouble(),
         JsonValueKind.True   => true,
         JsonValueKind.False  => false,
         JsonValueKind.Array  => element.EnumerateArray().Select(ConvertElement).ToList(),
         JsonValueKind.Object => ElementToDict(element),
         JsonValueKind.Null   => null!,
         _                    => throw new ArgumentException("Unsupported element") //code used to `be => null!` which just discards any element we do not understand. Yikes.
      };
   }

   public static string DictToJson(Dictionary<string, object> dict) => JsonSerializer.Serialize(dict, DictJsonOptions);

   static readonly JsonSerializerOptions DictJsonOptions = new()
                                                           {
                                                              Encoder = FullUnicodeJsonEncoder.Instance,
                                                           };
}
