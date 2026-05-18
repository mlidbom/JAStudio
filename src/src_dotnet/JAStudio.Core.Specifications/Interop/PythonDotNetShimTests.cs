using System.Collections.Generic;
using Compze.Must;
using JAStudio.PythonInterop;
using JAStudio.PythonInterop.Utilities;
using Xunit;

namespace JAStudio.Core.Specifications.Interop;

[NCrunch.Framework.RequiresCapability("PYTHON_VENV")]
public class PythonDotNetShimTests
{
   [Fact] public void RoundTrippingStringListResultInIdenticalList()
   {
      List<string> expected = ["1", "2", "3"];
      var other = (List<string>)PythonDotNetShim.StringList.ToDotNet(
         PythonDotNetShim.StringList.ToPython(expected));

      other.Must().DeepEqual(expected);

      //this is way too slow, we need another aproach for our amounts of data
      PythonEnvironment.Use(() =>
      {
         for(var i = 0; i < 10000; i++)
         {
            other = (List<string>)PythonDotNetShim.StringList.ToDotNet(PythonDotNetShim.StringList.ToPython(expected));
         }
      });
   }

   [Fact] public void RoundTrippingStringStringDictsResultInIdenticalDict()
   {
      var expected = new Dictionary<string, string>
                     {
                        ["1"] = "2",
                        ["3"] = "4"
                     };
      var other = (Dictionary<string, string>)PythonDotNetShim.StringStringDict.ToDotNet(
         PythonDotNetShim.StringStringDict.ToPython(expected));

      other.Must().DeepEqual(expected);

      //this is way too slow, we need another approach for our amounts of data
      PythonEnvironment.Use(() =>
      {
         for(var i = 0; i < 10000; i++)
         {
            other = (Dictionary<string, string>)PythonDotNetShim.StringStringDict.ToDotNet(PythonDotNetShim.StringStringDict.ToPython(expected));
         }
      });
   }

   [Fact] public void RoundTrippingLongListResultInIdenticalList()
   {
      List<long> expected = [1L, 2L, 3L];
      var other = (List<long>)PythonDotNetShim.LongList.ToDotNet(
         PythonDotNetShim.LongList.ToPython(expected));

      other.Must().DeepEqual(expected);

      PythonEnvironment.Use(() =>
      {
         for(var i = 0; i < 10000; i++)
         {
            other = (List<long>)PythonDotNetShim.LongList.ToDotNet(PythonDotNetShim.LongList.ToPython(expected));
         }
      });
   }
}
