using System;
using System.IO;
using System.IO.Hashing;
using Microsoft.AspNetCore.Hosting;

namespace JAStudio.Web;

/// <summary>
/// The card pages' own scripts and stylesheets, served from the web root, such as <c>/js/card.js</c>: their paths as the pages load them, each with a<br/>
/// version taken from the file's content, such as <c>/js/card.js?v=3F2A9C1B04D7E655</c>.
/// </summary>
/// <remarks>
/// A browser that keeps a file in its cache loads it anew only once its path changes. The server sends the files with no caching headers, so a web<br/>
/// view counts its cached copy as fresh for days without asking for it again: the workspace's WebView2 kept running an old <c>card.js</c> after a<br/>
/// restart (2026-11-13). A path versioned by the content changes with the file, whatever a cache holds.
/// </remarks>
sealed class WebRootFiles
{
   readonly IWebHostEnvironment _environment;

   public WebRootFiles(IWebHostEnvironment environment) => _environment = environment;

   /// <summary><paramref name="path"/>, a path in the web root such as <c>/js/card.js</c>, with the version of the file's content as it is now.</summary>
   public string VersionedPath(string path)
   {
      var file = _environment.WebRootFileProvider.GetFileInfo(path);
      if(!file.Exists) throw new FileNotFoundException($"The web root has no file {path}.", path);

      using var content = file.CreateReadStream();
      var hash = new XxHash64();
      hash.Append(content);
      return $"{path}?v={Convert.ToHexString(hash.GetCurrentHash())}";
   }
}
