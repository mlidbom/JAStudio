namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>A type whose objects reference objects of the type <see cref="WhatHoldsTheObjectsOfAType"/> is about, and how many they reference.</summary>
/// <param name="Type">The type's name, as the runtime has it, such as <c>System.String[]</c>.</param>
/// <param name="Holders">How many of its objects reference at least one of them.</param>
/// <param name="ObjectsHeld">How many of them its objects reference, each counted once however many of its objects reference it.</param>
/// <param name="MegabytesHeld">The megabytes those take: their own size, not what they reference.</param>
/// <param name="PercentOfTheMegabytes">Their share of the megabytes every object of the type held takes, in percent.</param>
public record HolderType(string Type, long Holders, long ObjectsHeld, double MegabytesHeld, double PercentOfTheMegabytes);
