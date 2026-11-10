using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Audio;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Storage.Media;
using JAStudio.UI.Views.TextToSpeech;

namespace JAStudio.UI.Controls.NoteFields;

/// <summary>
/// One of a note's lists of audio files (<see cref="AudioReferenceList"/>), shown in an <see cref="AudioListEditor"/>: a row for each file, with a button<br/>
/// playing it. The corpus's own audio of a vocab or a sentence also has a "TTS..." button, which opens the <see cref="TextToSpeechDialog"/> to generate<br/>
/// TTS audio for the note: using an attempt there replaces the TTS audio in this list.
/// </summary>
partial class AudioListEditorViewModel : MediaListEditorViewModel
{
   readonly Func<AudioReferenceList> _currentList;
   readonly MediaFileIndex _media;
   readonly IAudioPlayer _player;
   readonly Action? _openTheTextToSpeechDialog;

   /// <param name="title">What the list holds, shown above it, such as "Audio".</param>
   /// <param name="currentList">Finds the list on the note as the collection holds it now.</param>
   /// <param name="media">Finds each file in the media layer, to play it.</param>
   /// <param name="player">Plays a file when its button is pressed.</param>
   /// <param name="openTheTextToSpeechDialog">Opens the <see cref="TextToSpeechDialog"/> for the note, for the list its TTS audio goes into; null for any other list.</param>
   public AudioListEditorViewModel(string title, Func<AudioReferenceList> currentList, MediaFileIndex media, IAudioPlayer player, Action? openTheTextToSpeechDialog = null) : base(title)
   {
      _currentList = currentList;
      _media = media;
      _player = player;
      _openTheTextToSpeechDialog = openTheTextToSpeechDialog;
      Refresh();
   }

   /// <summary>The list's files, in the order they're preferred in.</summary>
   public ObservableCollection<AudioRowViewModel> Items { get; } = [];

   /// <summary>Whether the list shows the "TTS..." button: it's the list the note's TTS audio goes into.</summary>
   public bool CanGenerateTextToSpeechAudio => _openTheTextToSpeechDialog is not null;

   public override void Refresh()
   {
      Items.Clear();
      foreach(var audio in _currentList().All)
         Items.Add(new AudioRowViewModel(audio, _media.Resolve(audio), _player, Remove));
   }

   [RelayCommand]
   void OpenTheTextToSpeechDialog() => (_openTheTextToSpeechDialog ?? throw new InvalidOperationException($"This {Title} isn't the list TTS audio goes into."))();

   void Remove(AudioReference audio)
   {
      _currentList().Remove(audio);
      Refresh();
   }
}
