namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>A type of object on the managed heap, with how many of its objects the heap holds, and what they take.</summary>
/// <param name="Type">The type's name, as the runtime has it, such as <c>System.Collections.Generic.List&lt;System.String&gt;</c>.</param>
/// <param name="Objects">How many objects of the type the heap holds.</param>
/// <param name="Megabytes">The megabytes they take: each object's own size, not what it references.</param>
/// <param name="PercentOfTheHeap">Their share of all the heap's megabytes, in percent.</param>
public record HeapType(string Type, long Objects, double Megabytes, double PercentOfTheHeap);
