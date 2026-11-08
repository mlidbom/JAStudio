namespace JAStudio.Core.Importing.Checking;

/// <summary>
/// A fault in an <see cref="ImportProfile"/> itself, such as an unmapped question, found when an <see cref="ImportRun"/> is prepared.<br/>
/// While a profile has any, its raw notes aren't even selected, and nothing can be imported.
/// </summary>
public class ImportProfileProblem(string description)
{
   public string Description { get; } = description;

   public override string ToString() => Description;
}
