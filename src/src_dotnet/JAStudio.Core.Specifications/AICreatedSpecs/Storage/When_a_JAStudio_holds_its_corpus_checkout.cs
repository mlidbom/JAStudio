using System;
using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Storage;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Storage;

/// <summary>
/// A running JAStudio holds the corpus checkout it works on (<see cref="CorpusCheckoutLock"/>), so that no two JAStudios, such as JAStudio inside Anki and<br/>
/// the standalone JAStudio on the same source checkout, save notes to it at the same time.
/// </summary>
public class When_a_JAStudio_holds_its_corpus_checkout : IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"JAStudio_corpus_lock_{Guid.NewGuid():N}"));
   protected readonly string CorpusCheckout;
   protected readonly CorpusCheckoutLock TheFirstJAStudiosHold;

   public When_a_JAStudio_holds_its_corpus_checkout()
   {
      CorpusCheckout = _folder.CreateSubdirectory("jas_database").FullName;
      TheFirstJAStudiosHold = CorpusCheckoutLock.On(CorpusCheckout);
   }

   protected string TheLockFile => CorpusCheckout + ".lock";

   [XF] public void another_JAStudio_on_it_is_refused() => Invoking(() => CorpusCheckoutLock.On(CorpusCheckout)).Must().Throw<InvalidOperationException>();
   [XF] public void a_JAStudio_on_another_corpus_checkout_is_not_refused() => CorpusCheckoutLock.On(_folder.CreateSubdirectory("another_corpus").FullName).Dispose();
   [XF] public void the_lock_file_is_beside_the_corpus_checkout() => TheLockFile.Must().Satisfy(File.Exists);

   public class and_lets_go_of_it : When_a_JAStudio_holds_its_corpus_checkout
   {
      public and_lets_go_of_it() => TheFirstJAStudiosHold.Dispose();

      [XF] public void the_lock_file_is_gone() => TheLockFile.Must().Satisfy(it => !File.Exists(it));
      [XF] public void another_JAStudio_can_hold_it() => CorpusCheckoutLock.On(CorpusCheckout).Dispose();
   }

   public void Dispose()
   {
      TheFirstJAStudiosHold.Dispose();
      _folder.Delete(recursive: true);
   }
}
