import abc
from System.Collections.Generic import IReadOnlyList_1
from System.IO import FileInfo, DirectoryInfo

class DirectoryInfoExtensions(abc.ABC):
    @staticmethod
    def TheFilesInPathOrder(this: DirectoryInfo, searchPattern: str) -> IReadOnlyList_1[FileInfo]: ...
    @staticmethod
    def TheFolderItself(this: DirectoryInfo) -> DirectoryInfo: ...

    class <G>$D6C32AF7ED94487242587B1DD1DFB0A8:
        def TheFilesInPathOrder(self, searchPattern: str) -> IReadOnlyList_1[FileInfo]: ...
        def TheFolderItself(self) -> DirectoryInfo: ...

        class <M>$8FE5AAD56ADE6BC6A6D324DA1B2AB6A5(abc.ABC):
            @staticmethod
            def <Extension>$(this: DirectoryInfo) -> None: ...



