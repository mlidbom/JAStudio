using System.Text.Encodings.Web;

namespace JAStudio.Core.SysUtils.Json;

/// <summary>
/// A JSON encoder that outputs all Unicode characters as-is (UTF-8), escaping only what
/// the JSON spec strictly requires: control characters (U+0000–U+001F), backslash, and
/// double-quote.
///
/// .NET's built-in JavaScriptEncoder.UnsafeRelaxedJsonEscaping still escapes Unicode
/// space-separator characters (Zs category, e.g. U+3000 IDEOGRAPHIC SPACE, U+00A0
/// NO-BREAK SPACE) by design. This encoder skips that restriction so the files remain
/// human-readable.
///
/// Supplementary characters (> U+FFFF) are represented as surrogate pairs in the UTF-16
/// char buffer. FindFirstCharacterToEncode receives raw char values (always ≤ 0xFFFF),
/// so surrogates pass through WillEncode as false and are written directly as 4-byte UTF-8
/// by the underlying writer — which is the correct behavior for UTF-8 JSON files.
/// </summary>
sealed class FullUnicodeJsonEncoder : JavaScriptEncoder
{
   internal static readonly FullUnicodeJsonEncoder Instance = new();

   FullUnicodeJsonEncoder() {}

   public override int MaxOutputCharactersPerInputCharacter => 6; // \uXXXX

   public override bool WillEncode(int unicodeScalar)
   {
      if(unicodeScalar < 0x20) return true;  // C0 control characters
      if(unicodeScalar == '"') return true;  // JSON string delimiter
      if(unicodeScalar == '\\') return true; // JSON escape introducer
      return false;
   }

   public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
   {
      for(var i = 0; i < textLength; i++)
         if(WillEncode(text[i]))
            return i;
      return -1;
   }

   public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
   {
      // Standard JSON two-char escapes
      var singleCharEscape = unicodeScalar switch
      {
         '"'  => '"',
         '\\' => '\\',
         '\b' => 'b',
         '\t' => 't',
         '\n' => 'n',
         '\f' => 'f',
         '\r' => 'r',
         _    => '\0'
      };

      if(singleCharEscape != '\0')
      {
         if(bufferLength < 2)
         {
            numberOfCharactersWritten = 0;
            return false;
         }

         buffer[0] = '\\';
         buffer[1] = singleCharEscape;
         numberOfCharactersWritten = 2;
         return true;
      }

      // Generic \uXXXX escape for remaining control chars (0x00–0x07, 0x0B, 0x0E–0x1F)
      if(bufferLength < 6)
      {
         numberOfCharactersWritten = 0;
         return false;
      }

      WriteHexEscape(unicodeScalar, buffer);
      numberOfCharactersWritten = 6;
      return true;
   }

   static unsafe void WriteHexEscape(int value, char* buf)
   {
      buf[0] = '\\';
      buf[1] = 'u';
      buf[2] = HexChar((value >> 12) & 0xF);
      buf[3] = HexChar((value >> 8) & 0xF);
      buf[4] = HexChar((value >> 4) & 0xF);
      buf[5] = HexChar(value & 0xF);
   }

   static char HexChar(int v) => (char)(v < 10 ? '0' + v : 'a' + v - 10);
}
