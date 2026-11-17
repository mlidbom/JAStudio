using Compze.Tessaging.TessageTypes;

namespace JAStudio.Core.Configuration;

/// <summary>A value of JAStudio's configuration (<see cref="JapaneseConfig"/>), or its readings mappings, changed, and the change is written to its file.</summary>
public interface IConfigurationChanged : ITevent;
