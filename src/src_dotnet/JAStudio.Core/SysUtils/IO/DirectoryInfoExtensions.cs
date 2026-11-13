using System.IO;

namespace JAStudio.Core.SysUtils.IO;

/// <summary>What JAStudio needs to know about a folder that <see cref="DirectoryInfo"/> doesn't tell.</summary>
public static class DirectoryInfoExtensions
{
   extension(DirectoryInfo @this)
   {
      /// <summary>
      /// The folder this path leads to, with every link along it followed, such as a junction or a symbolic link: so that every path to a folder gives<br/>
      /// the same one. Anki loads JAStudio through a link to the add-on folder, <c>addons21\jastudio_module</c>, which leads to a source checkout's<br/>
      /// <c>src</c> folder: through it, the corpus checkout is <c>addons21\jastudio_module\jas_database</c>, and itself, <c>C:\Dev\JAStudio\src\jas_database</c>.
      /// </summary>
      public DirectoryInfo TheFolderItself()
      {
         var folder = new DirectoryInfo(Path.TrimEndingDirectorySeparator(Path.GetFullPath(@this.FullName)));
         if(folder.Parent is not {} folderItIsIn) return folder; // A root, such as C:\, which is no link, and which ResolveLinkTarget fails on.

         var withTheLinksAboveItFollowed = new DirectoryInfo(Path.Combine(folderItIsIn.TheFolderItself().FullName, folder.Name));
         return withTheLinksAboveItFollowed.ResolveLinkTarget(returnFinalTarget: true) is DirectoryInfo linkedTo
                   ? linkedTo.TheFolderItself()
                   : withTheLinksAboveItFollowed;
      }
   }
}
