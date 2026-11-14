using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

      /// <summary>
      /// The files in this folder and in every folder in it whose names match <paramref name="searchPattern"/>, in the ordinal order of their paths:<br/>
      /// the same order on every platform. What JAStudio reads in the order a folder lists its files would otherwise differ between platforms,<br/>
      /// such as which of two vocab with the same form comes first.
      /// </summary>
      /// <remarks>
      /// The system lists a folder's files in an order of its own: Windows' NTFS by name, Linux's ext4 by a hash of the name. For the corpus's note<br/>
      /// files, whose folders and names are lowercase hexadecimal, NTFS's order is this one, so on Windows nothing is read in another order than before.
      /// </remarks>
      public IReadOnlyList<FileInfo> TheFilesInPathOrder(string searchPattern) =>
         //review-naming: TheFilesInPathOrder is a name I chose (2026-11-14).
         [.. @this.EnumerateFiles(searchPattern, SearchOption.AllDirectories).OrderBy(it => it.FullName, StringComparer.Ordinal)];
   }
}
