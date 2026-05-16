using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.UI.ViewModels;

partial class SentenceEditorViewModel : ObservableObject
{
   readonly SentenceNote _sentence;

#pragma warning disable CS8618
   [Obsolete("Parameterless constructor is only for XAML designer support and should not be used directly.")]
   public SentenceEditorViewModel() {}
#pragma warning restore CS8618

   public SentenceEditorViewModel(SentenceNote sentence)
   {
      _sentence = sentence;
      Title = $"Edit Sentence: {sentence.GetQuestion()}";
      LoadFromNote();
   }

   public string Title { get; }

   // --- Editable fields ---

   [ObservableProperty] string _userQuestion = "";
   [ObservableProperty] string _userAnswer = "";
   [ObservableProperty] string _userComments = "";
   [ObservableProperty] string _reading = "";
   [ObservableProperty] string _audio = "";
   [ObservableProperty] string _screenshot = "";
   [ObservableProperty] string _id = "";

   // --- Read-only reference fields ---

   [ObservableProperty] string _sourceQuestion = "";
   [ObservableProperty] string _sourceAnswer = "";
   [ObservableProperty] string _sourceComments = "";

   // --- Commands ---

   public IRelayCommand SaveCommand { get; set; } = null!;
   public IRelayCommand ApplyCommand { get; set; } = null!;
   public IRelayCommand CancelCommand { get; set; } = null!;

   void LoadFromNote()
   {
      UserQuestion = _sentence.User.Question.Value;
      UserAnswer = _sentence.User.Answer.Value;
      UserComments = _sentence.User.Comments.Value;
      Reading = _sentence.Reading.Value;

      SourceQuestion = _sentence.SourceQuestion.Value;
      SourceAnswer = _sentence.SourceAnswer.Value;
      SourceComments = _sentence.SourceComments.Value;

      Audio = _sentence.Audio.RawValue();
      Screenshot = _sentence.Screenshot.RawValue();
      Id = _sentence.ExternalId.Value;
   }

   public void Save()
   {
      _sentence.User.Question.Set(UserQuestion);
      _sentence.User.Answer.Set(UserAnswer);
      _sentence.User.Comments.Set(UserComments);
      _sentence.Reading.Set(Reading);
      _sentence.SourceQuestion.Set(SourceQuestion);
      _sentence.SourceAnswer.Set(SourceAnswer);
      _sentence.SourceComments.Set(SourceComments);
      _sentence.Audio.SetRawValue(Audio);
      _sentence.Screenshot.SetRawValue(Screenshot);
      _sentence.ExternalId.Set(Id);
      _sentence.UpdateGeneratedData();
   }
}
