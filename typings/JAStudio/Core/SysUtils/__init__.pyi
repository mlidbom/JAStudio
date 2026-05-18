import typing, abc
from System import Func_2, Action_1, Func_1, Action

class ActionFuncHarmonization(abc.ABC):
    # Skipped AsFunc due to it being static, abstract and generic.

    AsFunc : AsFunc_MethodGroup
    class AsFunc_MethodGroup:
        def __getitem__(self, t:typing.Type[AsFunc_1_T1]) -> AsFunc_1[AsFunc_1_T1]: ...

        AsFunc_1_T1 = typing.TypeVar('AsFunc_1_T1')
        class AsFunc_1(typing.Generic[AsFunc_1_T1]):
            AsFunc_1_TInput = ActionFuncHarmonization.AsFunc_MethodGroup.AsFunc_1_T1
            def __call__(self, this: Action_1[AsFunc_1_TInput]) -> Func_2[AsFunc_1_TInput, int]:...

        def __call__(self, this: Action) -> Func_1[int]:...



class ExStr(abc.ABC):
    @staticmethod
    def StripHtmlAndBracketMarkupAndNoiseCharacters(input: str) -> str: ...

