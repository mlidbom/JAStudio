using System.Collections.Generic;

namespace JAStudio.Core.LanguageServices.TokenizerDictionaries.Comparing;

public sealed partial class Tally<TThing>
{
   /// <summary>A thing a <see cref="Tally{TThing}"/> counts: how many times it was found, and a few different examples of it.</summary>
   public sealed class Counted
   {
      readonly List<string> _examples = [];

      internal Counted(TThing thing) => Thing = thing;

      /// <summary>What was found.</summary>
      public TThing Thing { get; }

      /// <summary>How many times it was found.</summary>
      public int Count { get; private set; }

      /// <summary>Up to <see cref="ExamplesKept"/> different examples of it, the first kept first.</summary>
      public IReadOnlyList<string> Examples => _examples;

      internal void Add(int count, IEnumerable<string> examples)
      {
         Count += count;
         foreach(var example in examples)
            if(_examples.Count < ExamplesKept && !_examples.Contains(example))
               _examples.Add(example);
      }
   }
}
