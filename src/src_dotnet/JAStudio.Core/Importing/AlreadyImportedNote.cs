using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Importing;

/// <summary>
/// A <see cref="RawNote"/> that an <see cref="ImportRun"/> skips, with a warning, because it looks imported already: its external id is in the corpus,<br/>
/// or one of its media files' names is in the media layer. A name match may also be a different file with the same name, which couldn't be stored either.
/// </summary>
public class AlreadyImportedNote(RawNote rawNote, string question, string whatMatched)
{
   public RawNote RawNote { get; } = rawNote;
   public string Question { get; } = question;

   /// <summary>What's already in JAStudio, such as "external id anime_…_000000346 is in the corpus".</summary>
   public string WhatMatched { get; } = whatMatched;

   public override string ToString() => $"{RawNote} ({Question}): {WhatMatched}";
}
