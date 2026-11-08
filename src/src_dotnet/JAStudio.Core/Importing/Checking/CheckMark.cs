namespace JAStudio.Core.Importing.Checking;

/// <summary>
/// The mark a <see cref="FieldCheck"/> shows for one checked field of a raw note, such as its external id or its audio, in the import report.<br/>
/// The declaration order is the order of severity: a field with several files shows the worst of their marks.
/// </summary>
public enum CheckMark
{
   /// <summary>As it should be: a green ✓.</summary>
   Fine,

   /// <summary>Not checked, or nothing wrong but worth knowing, such as an imported note's file that no note uses any more: a grey –.</summary>
   Neutral,

   /// <summary>Already in JAStudio, so the raw note is skipped: an orange ✗.</summary>
   AlreadyInJAStudio,

   /// <summary>A problem, which stops the whole run: a red ✗.</summary>
   Problem
}
