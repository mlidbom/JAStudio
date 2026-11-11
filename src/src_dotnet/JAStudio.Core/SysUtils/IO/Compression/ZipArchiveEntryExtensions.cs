using System.IO;
using System.IO.Compression;

namespace JAStudio.Core.SysUtils.IO.Compression;

/// <summary>Reading a zip's entries, such as the files an Anki package holds.</summary>
static class ZipArchiveEntryExtensions
{
   extension(ZipArchiveEntry @this)
   {
      /// <summary>The entry's content, as it was before the zip compressed it, read into memory whole.</summary>
      internal byte[] ReadAllBytes()
      {
         //todo:compze: nothing here is about JAStudio: Compze.Internals.SystemCE, or another of Compze's extensions of .NET, could give
         //   ZipArchiveEntry this. Once it does, use that and delete this class.
         using var stream = @this.Open();
         using var bytes = new MemoryStream();
         stream.CopyTo(bytes);
         return bytes.ToArray();
      }
   }
}
