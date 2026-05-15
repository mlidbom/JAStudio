using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Storage.Media;

public class MediaImportExecutor
{
   readonly MediaStorageService _storageService;
   readonly TaskRunner _taskRunner;

   public MediaImportExecutor(MediaStorageService storageService, TaskRunner taskRunner)
   {
      _storageService = storageService;
      _taskRunner = taskRunner;
   }

   public void Execute(MediaImportPlan plan)
   {
      using var scope = _taskRunner.Current("Importing media from Anki");

      scope.RunBatch(plan.FilesToImport,
                     file => _storageService.StoreFile(file.SourcePath, file.TargetDirectory, file.OriginalFileName, file.MediaType),
                     "Copying media files");
   }
}
