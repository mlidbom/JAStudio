using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNote : JPNote
{
   public VocabNoteQuestion Question { get; private set; }
   public VocabNoteUserFields User { get; }
   public VocabNoteForms Forms { get; }
   public VocabNoteKanji Kanji { get; }
   public VocabNotePartsOfSpeech PartsOfSpeech { get; }
   public VocabNoteConjugator Conjugator { get; }
   public VocabNoteSentences Sentences { get; }
   public VocabNoteUserCompoundParts CompoundParts { get; }
   public Vocabulary.RelatedVocab.RelatedVocab RelatedNotes { get; }
   public VocabNoteMetaData MetaData { get; }
   public VocabNoteRegister Register { get; }
   public VocabNoteAudio Audio { get; }
   public VocabNoteMatchingConfiguration MatchingConfiguration { get; private set; }
   public VocabCloner Cloner { get; }

   public WritableStringValue SourceAnswer { get; }
   public WritableStringValue ActiveAnswer { get; }
   public WritableStringValue SourceMnemonic { get; }
   public WritableStringValue SourceReadingMnemonic { get; }
   public WritableStringValue TechnicalNotes { get; }
   public WritableStringValue References { get; }
   public WritableImageValue Image { get; }
   public WritableImageValue UserImage { get; }

   readonly List<string> _readings;

   public VocabNote(NoteServices services, VocabData? data = null) : base(services, data != null ? new VocabId(data.Id) : VocabId.New(), data?.Tags)
   {
      SourceAnswer = new WritableStringValue(data?.SourceAnswer ?? string.Empty, Guard);
      ActiveAnswer = new WritableStringValue(data?.ActiveAnswer ?? string.Empty, Guard);
      SourceMnemonic = new WritableStringValue(data?.SourceMnemonic ?? string.Empty, Guard);
      SourceReadingMnemonic = new WritableStringValue(data?.SourceReadingMnemonic ?? string.Empty, Guard);
      TechnicalNotes = new WritableStringValue(data?.TechnicalNotes ?? string.Empty, Guard);
      References = new WritableStringValue(data?.References ?? string.Empty, Guard);
      Image = new WritableImageValue(data?.Image ?? string.Empty, Guard);
      UserImage = new WritableImageValue(data?.UserImage ?? string.Empty, Guard);
      _readings = data?.Readings != null ? [..data.Readings] : [];

      Question = new VocabNoteQuestion(this, data, Guard);
      User = new VocabNoteUserFields(data, Guard);
      Forms = new VocabNoteForms(this, data, Guard);
      Kanji = new VocabNoteKanji(this);
      PartsOfSpeech = new VocabNotePartsOfSpeech(this, data, Guard);
      Conjugator = new VocabNoteConjugator(this);
      Sentences = new VocabNoteSentences(this);
      CompoundParts = new VocabNoteUserCompoundParts(this, data, Guard);
      RelatedNotes = new Vocabulary.RelatedVocab.RelatedVocab(this, data?.RelatedVocab, Guard);
      MetaData = new VocabNoteMetaData(this, data, Guard);
      Register = new VocabNoteRegister(this);
      Audio = new VocabNoteAudio(data, Guard);
      MatchingConfiguration = new VocabNoteMatchingConfiguration(this, data, Guard);
      Cloner = new VocabCloner(this);
   }

   public override void UpdateInCache()
   {
      Services.Collection.Vocab.Cache.JpNoteUpdated(this);
   }

   public override CorpusObjectData ToCorpusData() => VocabNoteConverter.ToCorpusData(this);

   public override string GetQuestion() => Question.Raw;

   public IReadOnlyList<AudioAttachment> FirstAudio   => Services.MediaFileIndex.GetAudioAttachments(Audio.First.RawValue());
   public IReadOnlyList<AudioAttachment> SecondAudio  => Services.MediaFileIndex.GetAudioAttachments(Audio.Second.RawValue());
   public IReadOnlyList<AudioAttachment> TtsAudio     => Services.MediaFileIndex.GetAudioAttachments(Audio.Tts.RawValue());
   public AudioAttachment?               PreferredAudio => FirstAudio.FirstOrDefault() ?? SecondAudio.FirstOrDefault() ?? TtsAudio.FirstOrDefault();
   public IReadOnlyList<ImageAttachment> Images       => Services.MediaFileIndex.GetImageAttachments(Image.RawValue());
   public IReadOnlyList<ImageAttachment> UserImages   => Services.MediaFileIndex.GetImageAttachments(UserImage.RawValue());

   public override List<MediaReference> MediaReferences
   {
      get
      {
         var refs = Audio.First.GetMediaReferences();
         refs.AddRange(Audio.Second.GetMediaReferences());
         refs.AddRange(Audio.Tts.GetMediaReferences());
         refs.AddRange(Image.GetMediaReferences());
         refs.AddRange(UserImage.GetMediaReferences());
         return refs;
      }
   }

   public override string GetAnswer()
   {
      var userAnswer = User.Answer.Value;
      var sourceAnswer = SourceAnswer.Value;
      return !string.IsNullOrEmpty(userAnswer) ? userAnswer : sourceAnswer;
   }

   public override void OnTagsUpdated()
   {
      MatchingConfiguration.RefreshTagBasedFlags();
   }

   public override void UpdateGeneratedData()
   {
      base.UpdateGeneratedData();

      Services.VocabNoteGeneratedData.UpdateGeneratedData(this);
   }

   public List<string> GetReadings() => _readings.ToList();

   public void SetReadings(List<string> readings) => Guard.Update(() =>
   {
      _readings.Clear();
      _readings.AddRange(readings);
   });

   public void GenerateAndSetAnswer()
   {
      var dictLookup = Services.DictLookup.LookupVocabWordOrName(this);
      if(dictLookup.FoundWords())
      {
         var generated = dictLookup.FormatAnswer();
         SourceAnswer.Set(generated);
      }

      UpdateGeneratedData();
   }

   public static VocabNote Create(NoteServices services, string question, string answer, List<string> readings, List<string> forms)
   {
      var note = new VocabNote(services);
      note.Question.Set(question);
      note.User.Answer.Set(answer);
      note.SetReadings(readings);

      if(forms.Any())
      {
         note.Forms.SetList(forms);
      }

      note.UpdateGeneratedData();
      services.Collection.Vocab.Add(note);
      return note;
   }

   public static VocabNote Create(NoteServices services, string question, string answer, params string[] readings) =>
      Create(services, question, answer, readings.ToList(), []);
}
