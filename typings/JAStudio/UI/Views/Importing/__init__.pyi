import abc
from JAStudio.Core import TemporaryServiceCollection

class AutomaticImportRunReports(abc.ABC):
    @staticmethod
    def StartShowing(services: TemporaryServiceCollection) -> None: ...

