import abc
from System.IO import DirectoryInfo, FileInfo
from System.Collections.Generic import IReadOnlyList_1
from System import IDisposable

class DirectoryInfoExtensions(abc.ABC):
    @staticmethod
    def DeleteWithEverythingInIt(this: DirectoryInfo) -> None: ...
    @staticmethod
    def TheFilesInPathOrder(this: DirectoryInfo, searchPattern: str) -> IReadOnlyList_1[FileInfo]: ...
    @staticmethod
    def TheFolderItself(this: DirectoryInfo) -> DirectoryInfo: ...


class ExclusiveHoldOnAFile(IDisposable):
    def Dispose(self) -> None: ...
    @staticmethod
    def TakeUnlessAnotherProcessHasIt(path: str) -> ExclusiveHoldOnAFile: ...

