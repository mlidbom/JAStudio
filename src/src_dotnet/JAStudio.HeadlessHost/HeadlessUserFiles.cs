using System;
using System.IO;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio's own user files: a copy of the add-on's user files, made as it starts, beside the copy of its build output it runs<br/>
/// from. It reads them as JAStudio inside Anki reads its own, such as the readings mappings, but what it writes, such as where its windows<br/>
/// were, stays in the copy, apart from the live JAStudio's.
/// </summary>
/// <remarks>
/// The add-on's user files are the live JAStudio's: the window places and the workspace's state, its tabs and its search, are saved there.<br/>
/// Rendering the headless JAStudio's windows would otherwise move Magnus's windows, and change his workspace, the next time Anki starts.
/// </remarks>
static class HeadlessUserFiles
{
   public static DirectoryInfo Folder => new(Path.Combine(AppContext.BaseDirectory, "user_files"));

   /// <summary>Replaces <see cref="Folder"/> with a copy of the user files of the add-on in <paramref name="addonFolder"/>.</summary>
   public static void ReplaceWithACopyOfTheUserFilesOf(DirectoryInfo addonFolder)
   {
      var folder = Folder;
      if(folder.Exists) folder.Delete(recursive: true);
      CopyEverything(new DirectoryInfo(Path.Combine(addonFolder.FullName, "user_files")), folder);
   }

   static void CopyEverything(DirectoryInfo from, DirectoryInfo to)
   {
      to.Create();
      foreach(var file in from.EnumerateFiles()) file.CopyTo(Path.Combine(to.FullName, file.Name));
      foreach(var subfolder in from.EnumerateDirectories()) CopyEverything(subfolder, new DirectoryInfo(Path.Combine(to.FullName, subfolder.Name)));
   }
}
