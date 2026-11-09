using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Core.Note.Vocabulary.EditableFields;

/// <summary>
/// A vocab's <see cref="EnteredForm"/>s: the forms besides its question, each owned by the vocab or not (<see cref="VocabNoteForms"/>).<br/>
/// Its value is the forms in alphabetical order. Every change changes how sentences parse.
/// </summary>
/// <remarks>
/// Writing it adds, removes, owns and stops owning forms one at a time, as the vocab's editor does, and only those that change, since adding<br/>
/// or removing a form also adds or removes the vocab's question among the forms of the vocab whose question it is (<see cref="VocabNoteForms.Add"/>).
/// </remarks>
public sealed class EditableFormsField : EditableField<VocabNote, IReadOnlyList<EnteredForm>>
{
   public EditableFormsField() : base("forms", "Forms", changesHowSentencesParse: true) {}

   public override IReadOnlyList<EnteredForm> Read(VocabNote note) => [.. note.Forms.EnteredForms().OrderBy(it => it.Text)];

   /// <summary>Makes <paramref name="value"/> the vocab's entered forms, but for its question, which is always a form, and so never an entered one.</summary>
   public override void Write(VocabNote note, IReadOnlyList<EnteredForm> value)
   {
      var forms = note.Forms;
      var entered = value.Where(it => it.Text != note.GetQuestion()).ToList();
      var texts = entered.Select(it => it.Text).ToHashSet();

      foreach(var removed in forms.EnteredForms().Where(it => !texts.Contains(it.Text))) forms.Remove(removed.Text);
      foreach(var form in entered) WriteTheForm(form);

      return;

      void WriteTheForm(EnteredForm form)
      {
         if(!forms.EnteredFormsSet().Contains(form.Text)) forms.Add(form.Text);
         if(form.IsOwned == forms.EnteredForms().Single(it => it.Text == form.Text).IsOwned) return;

         if(form.IsOwned) forms.Own(form.Text);
         else forms.StopOwning(form.Text);
      }
   }
}
