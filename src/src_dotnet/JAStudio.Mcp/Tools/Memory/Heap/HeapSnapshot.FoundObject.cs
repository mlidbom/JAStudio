using Microsoft.Diagnostics.Runtime;

namespace JAStudio.Mcp.Tools.Memory.Heap;

partial class HeapSnapshot
{
   /// <summary>
   /// What the walk of a <see cref="HeapSnapshot"/> found where it expected an object: an object, or memory that reads as one but can't be<br/>
   /// read, as the snapshot's remarks tell. Only an object that <see cref="CanBeRead"/> may be counted.
   /// </summary>
   /// <param name="Object">The object, as ClrMD reads it.</param>
   /// <param name="CanBeRead">Whether it's an object whose type, size and references can be trusted.</param>
   public readonly record struct FoundObject(ClrObject Object, bool CanBeRead)
   {
      /// <summary>The name of the object's type, as the runtime has it, such as <c>System.Collections.Generic.List&lt;System.String&gt;</c>.</summary>
      public string TypeName => Object.Type!.Name ?? "(a type with no name)";
   }
}
