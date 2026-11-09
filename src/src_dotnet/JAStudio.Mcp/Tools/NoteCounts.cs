using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;

namespace JAStudio.Mcp.Tools;

/// <summary>How many notes of one type a collection has, and how many of them are being studied: have a card in Anki that isn't suspended.</summary>
public record NoteCounts(int Notes, int Studying)
{
   public static NoteCounts Of(IReadOnlyCollection<JPNote> notes) => new(notes.Count, notes.Count(it => it.IsStudying()));
}
