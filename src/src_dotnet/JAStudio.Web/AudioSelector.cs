using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Storage.Media;

namespace JAStudio.Web;

static class AudioSelector
{
   public static AudioAttachment? SelectBest(IReadOnlyList<AudioAttachment> audio) => audio.FirstOrDefault();
}
