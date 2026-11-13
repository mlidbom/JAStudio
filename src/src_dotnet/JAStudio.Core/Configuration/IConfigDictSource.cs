using System.Collections.Generic;

namespace JAStudio.Core.Configuration;

/// <summary>Where JAStudio's configuration is kept: each value under its <see cref="ConfigurationValue{T}.Name"/>, as <see cref="ConfigurationStore"/> holds them.</summary>
public interface IConfigDictSource
{
   Dictionary<string, object> Load();

   /// <summary>Keeps <paramref name="configuration"/>, every value, after one of them changed.</summary>
   void Persist(IReadOnlyDictionary<string, object> configuration);
}
