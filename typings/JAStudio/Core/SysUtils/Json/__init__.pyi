import typing, clr
from System.Text.Encodings.Web import JavaScriptEncoder
from System.Text.Json import JsonElement
from System.Collections.Generic import IEnumerable_1, List_1, HashSet_1
from System import Func_2, Func_1

class FullUnicodeJsonEncoder(JavaScriptEncoder):
    Instance : FullUnicodeJsonEncoder
    @property
    def MaxOutputCharactersPerInputCharacter(self) -> int: ...
    def FindFirstCharacterToEncode(self, text: clr.Reference[str], textLength: int) -> int: ...
    def TryEncodeUnicodeScalar(self, unicodeScalar: int, buffer: clr.Reference[str], bufferLength: int, numberOfCharactersWritten: clr.Reference[int]) -> bool: ...
    def WillEncode(self, unicodeScalar: int) -> bool: ...


class JsonReader:
    def __init__(self, element: JsonElement) -> None: ...
    @staticmethod
    def FromJson(json: str) -> JsonReader: ...
    # Skipped GetInt due to it being static, abstract and generic.

    GetInt : GetInt_MethodGroup
    class GetInt_MethodGroup:
        @typing.overload
        def __call__(self, key: str, defaultValue: typing.Optional[int] = ...) -> int:...
        @typing.overload
        def __call__(self, keys: IEnumerable_1[str], defaultValue: typing.Optional[int] = ...) -> int:...

    # Skipped GetObject due to it being static, abstract and generic.

    GetObject : GetObject_MethodGroup
    class GetObject_MethodGroup:
        def __getitem__(self, t:typing.Type[GetObject_1_T1]) -> GetObject_1[GetObject_1_T1]: ...

        GetObject_1_T1 = typing.TypeVar('GetObject_1_T1')
        class GetObject_1(typing.Generic[GetObject_1_T1]):
            GetObject_1_T = JsonReader.GetObject_MethodGroup.GetObject_1_T1
            def __call__(self, key: str, factory: Func_2[JsonReader, GetObject_1_T], defaultFactory: Func_1[GetObject_1_T] = ...) -> GetObject_1_T:...


    # Skipped GetObjectList due to it being static, abstract and generic.

    GetObjectList : GetObjectList_MethodGroup
    class GetObjectList_MethodGroup:
        def __getitem__(self, t:typing.Type[GetObjectList_1_T1]) -> GetObjectList_1[GetObjectList_1_T1]: ...

        GetObjectList_1_T1 = typing.TypeVar('GetObjectList_1_T1')
        class GetObjectList_1(typing.Generic[GetObjectList_1_T1]):
            GetObjectList_1_T = JsonReader.GetObjectList_MethodGroup.GetObjectList_1_T1
            @typing.overload
            def __call__(self, key: str, factory: Func_2[JsonReader, GetObjectList_1_T], defaultValue: List_1[GetObjectList_1_T] = ...) -> List_1[GetObjectList_1_T]:...
            @typing.overload
            def __call__(self, keys: IEnumerable_1[str], factory: Func_2[JsonReader, GetObjectList_1_T], defaultValue: List_1[GetObjectList_1_T] = ...) -> List_1[GetObjectList_1_T]:...


    # Skipped GetString due to it being static, abstract and generic.

    GetString : GetString_MethodGroup
    class GetString_MethodGroup:
        @typing.overload
        def __call__(self, key: str, defaultValue: str = ...) -> str:...
        @typing.overload
        def __call__(self, keys: IEnumerable_1[str], defaultValue: str = ...) -> str:...

    # Skipped GetStringList due to it being static, abstract and generic.

    GetStringList : GetStringList_MethodGroup
    class GetStringList_MethodGroup:
        @typing.overload
        def __call__(self, key: str, defaultValue: List_1[str] = ...) -> List_1[str]:...
        @typing.overload
        def __call__(self, keys: IEnumerable_1[str], defaultValue: List_1[str] = ...) -> List_1[str]:...

    # Skipped GetStringSet due to it being static, abstract and generic.

    GetStringSet : GetStringSet_MethodGroup
    class GetStringSet_MethodGroup:
        @typing.overload
        def __call__(self, key: str, defaultValue: List_1[str] = ...) -> HashSet_1[str]:...
        @typing.overload
        def __call__(self, keys: IEnumerable_1[str], defaultValue: List_1[str] = ...) -> HashSet_1[str]:...


