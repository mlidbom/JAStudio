using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.ViewModels;

partial class VocabEditorViewModel : ObservableObject
{
   readonly VocabNote _vocab;

#pragma warning disable CS8618
   [Obsolete("Parameterless constructor is only for XAML designer support and should not be used directly.")]
   public VocabEditorViewModel() {}
#pragma warning restore CS8618

   public VocabEditorViewModel(VocabNote vocab)
   {
      _vocab = vocab;
      Title = $"Edit Vocab: {vocab.GetQuestion()}";
      LoadFromNote();
   }

   public string Title { get; }

   // --- Editable fields ---

   [ObservableProperty] string _question = "";
   [ObservableProperty] string _readings = "";
   [ObservableProperty] string _forms = "";
   [ObservableProperty] string _userAnswer = "";
   [ObservableProperty] string _userExplanation = "";
   [ObservableProperty] string _userExplanationLong = "";
   [ObservableProperty] string _userMnemonic = "";
   [ObservableProperty] string _partsOfSpeech = "";
   [ObservableProperty] string _userCompounds = "";
   [ObservableProperty] string _audioB = "";
   [ObservableProperty] string _audioG = "";
   [ObservableProperty] string _audioTts = "";
   [ObservableProperty] string _sentenceCount = "";
   [ObservableProperty] string _relatedVocab = "";
   [ObservableProperty] string _technicalNotes = "";

   // --- Read-only reference fields ---

   [ObservableProperty] string _sourceAnswer = "";
   [ObservableProperty] string _sourceMnemonic = "";
   [ObservableProperty] string _sourceReadingMnemonic = "";

   // --- Commands ---

   public IRelayCommand SaveCommand { get; set; } = null!;
   public IRelayCommand ApplyCommand { get; set; } = null!;
   public IRelayCommand CancelCommand { get; set; } = null!;

   void LoadFromNote()
   {
      Question = _vocab.Question.DisambiguationName;
      Readings = string.Join(", ", _vocab.GetReadings());
      Forms = string.Join(", ", _vocab.Forms.AllList());
      UserAnswer = _vocab.User.Answer.Value;
      UserExplanation = _vocab.User.Explanation.Value;
      UserExplanationLong = _vocab.User.ExplanationLong.Value;
      UserMnemonic = _vocab.User.Mnemonic.Value;
      PartsOfSpeech = _vocab.PartsOfSpeech.RawStringValue();
      UserCompounds = string.Join(", ", _vocab.CompoundParts.All());

      SourceAnswer = _vocab.SourceAnswer.Value;
      SourceMnemonic = _vocab.SourceMnemonic.Value;
      SourceReadingMnemonic = _vocab.SourceReadingMnemonic.Value;

      AudioB = _vocab.Audio.First.RawValue();
      AudioG = _vocab.Audio.Second.RawValue();
      AudioTts = _vocab.Audio.Tts.RawValue();
      SentenceCount = _vocab.MetaData.SentenceCount.ToString();
      RelatedVocab = _vocab.RelatedNotes.RawJson;
      TechnicalNotes = _vocab.TechnicalNotes.Value;
   }

   public void Save()
   {
      _vocab.Question.Set(Question.Trim());
      _vocab.SetReadings(SplitCommaSeparated(Readings));
      _vocab.Forms.SetList(SplitCommaSeparated(Forms));
      _vocab.User.Answer.Set(UserAnswer);
      _vocab.User.Explanation.Set(UserExplanation);
      _vocab.User.ExplanationLong.Set(UserExplanationLong);
      _vocab.User.Mnemonic.Set(UserMnemonic);
      _vocab.PartsOfSpeech.SetRawStringValue(PartsOfSpeech);
      _vocab.CompoundParts.Set(SplitCommaSeparated(UserCompounds));
      _vocab.SourceAnswer.Set(SourceAnswer);
      _vocab.SourceMnemonic.Set(SourceMnemonic);
      _vocab.SourceReadingMnemonic.Set(SourceReadingMnemonic);
      _vocab.Audio.First.SetRawValue(AudioB);
      _vocab.Audio.Second.SetRawValue(AudioG);
      _vocab.Audio.Tts.SetRawValue(AudioTts);
      if(int.TryParse(SentenceCount, out var count)) _vocab.MetaData.SetSentenceCount(count);
      _vocab.RelatedNotes.RawJson = RelatedVocab;
      _vocab.TechnicalNotes.Set(TechnicalNotes);
      _vocab.UpdateGeneratedData();
   }

   static System.Collections.Generic.List<string> SplitCommaSeparated(string value)
   {
      if(string.IsNullOrWhiteSpace(value)) return [];

      var items = value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
      return [..items];
   }
}
