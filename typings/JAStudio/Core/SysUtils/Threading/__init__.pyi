from JAStudio.Core.TaskRunners import BackgroundTaskManager
from System import Action

class BackgroundRefresh:
    def __init__(self, backgroundTasks: BackgroundTaskManager, refresh: Action) -> None: ...
    def Run(self) -> None: ...

