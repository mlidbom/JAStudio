using System;
using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.SysUtils.IO;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.SysUtils.IO;

/// <summary>
/// A path that leads to a folder through a link, such as Anki's path to the corpus checkout, <c>addons21\jastudio_module\jas_database</c>, through its<br/>
/// link to a source checkout's <c>src</c> folder, gives the same folder as the path to the folder itself (<see cref="DirectoryInfoExtensions.TheFolderItself"/>).
/// </summary>
public class When_a_path_leads_to_a_folder_through_a_link : IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"JAStudio_links_{Guid.NewGuid():N}"));
   readonly DirectoryInfo _theSourceCheckoutsAddOnFolder;
   readonly DirectoryInfo _theLinkToTheAddOnFolder;

   public When_a_path_leads_to_a_folder_through_a_link()
   {
      _theSourceCheckoutsAddOnFolder = _folder.CreateSubdirectory("checkout").CreateSubdirectory("src");
      _theSourceCheckoutsAddOnFolder.CreateSubdirectory("jas_database");
      _theLinkToTheAddOnFolder = (DirectoryInfo)Directory.CreateSymbolicLink(Path.Combine(_folder.FullName, "jastudio_module"), _theSourceCheckoutsAddOnFolder.FullName);
   }

   static string TheFolderItselfThrough(DirectoryInfo folder, string subfolder) => new DirectoryInfo(Path.Combine(folder.FullName, subfolder)).TheFolderItself().FullName;

   [XF] public void a_path_through_the_link_gives_the_same_folder_as_the_path_to_the_folder_itself() =>
      TheFolderItselfThrough(_theLinkToTheAddOnFolder, "jas_database").Must().Be(TheFolderItselfThrough(_theSourceCheckoutsAddOnFolder, "jas_database"));

   [XF] public void the_link_itself_gives_the_folder_it_links_to() =>
      _theLinkToTheAddOnFolder.TheFolderItself().FullName.Must().Be(_theSourceCheckoutsAddOnFolder.TheFolderItself().FullName);

   [XF] public void the_folder_itself_is_not_the_link() =>
      TheFolderItselfThrough(_theLinkToTheAddOnFolder, "jas_database").Must().Satisfy(it => !it.StartsWith(_theLinkToTheAddOnFolder.FullName, StringComparison.Ordinal));

   public void Dispose()
   {
      _theLinkToTheAddOnFolder.Delete();
      _folder.Delete(recursive: true);
      GC.SuppressFinalize(this);
   }
}
