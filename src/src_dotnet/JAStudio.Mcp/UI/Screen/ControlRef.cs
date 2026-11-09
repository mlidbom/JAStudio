namespace JAStudio.Mcp.UI.Screen;

/// <summary>
/// The name the screen-tools give a control on the screen, such as <c>c12</c>: the description of the screen lists each control by its ref, and<br/>
/// the tools that act on a control take it. <see cref="ControlRefs"/> gives each control its ref.
/// </summary>
/// <param name="Text">The ref as the tools write and take it, such as <c>c12</c>.</param>
public sealed record ControlRef(string Text)
{
   public override string ToString() => Text;
}
