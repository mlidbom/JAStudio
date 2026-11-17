using System;

namespace JAStudio.Mcp;

/// <summary>
/// Marks a tool, or a class of tools, that works without the collection's notes, so that it runs even while the collection has none to use yet<br/>
/// (<see cref="ToolsThatReadTheNotes"/>): such as the tools that diagnose a load of the collection that fails, by reloading it or by looking at the<br/>
/// corpus checkout's files.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
sealed class WorksWithoutTheNotesAttribute : Attribute;
