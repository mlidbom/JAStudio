namespace JAStudio.Core.Note.KanjiReadings;

/// <summary>
/// One reading of a kanji: its <see cref="Kana"/>, and whether it's primary.<br/>
/// The primary readings are the ones the kanji is studied by: its default mnemonic is made of them, and the kanji's page highlights them<br/>
/// in its vocab's readings. The other readings are secondary.
/// </summary>
/// <param name="Kana">
/// How the kanji is read, in kana, with KANJIDIC's marks where the reading has them: a <c>.</c> before the okurigana (<c>あわ.せる</c>),<br/>
/// and a <c>-</c> where the reading is a prefix or a suffix (<c>-なが.ら</c>).
/// </param>
/// <param name="IsPrimary">Whether it's one of the primary readings the kanji is studied by, rather than a secondary one.</param>
public record KanjiReading(string Kana, bool IsPrimary);
