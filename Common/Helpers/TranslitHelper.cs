using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Helpers
{
    /// <summary>
    /// Utility for performing transliteration.
    /// </summary>
    public static class TranslitHelper
    {
        #region CharMap

        // http://base.garant.ru/70344226/#block_97
        // http://www.icao.int/publications/Documents/9303_p1_v1_cons_ru.pdf

        private static readonly Dictionary<char, string> IcaoCharMap = new Dictionary<char, string> {
            {'Á', "A"},    {'À', "A"},    {'Â', "A"},    {'Ä', "Ae"},   {'Ã', "A"},    {'Ă', "A"},
            {'Å', "Aa"},   {'Ā', "A"},    {'Ą', "A"},    {'Ć', "C"},    {'Ĉ', "C"},    {'Č', "C"},
            {'Ċ', "C"},    {'Ç', "C"},    {'Ð', "D"},    {'Ď', "D"},    {'É', "E"},    {'È', "E"},
            {'Ê', "E"},    {'Ë', "E"},    {'Ĕ', "E"},    {'Ė', "E"},    {'Ē', "E"},    {'Ę', "E"},
            {'Ĝ', "G"},    {'Ğ', "G"},    {'Ġ', "G"},    {'Ģ', "G"},    {'Ħ', "H"},    {'Ĥ', "H"},
          /*{'I', "I"},*/  {'Í', "I"},    {'Ì', "I"},    {'Î', "I"},    {'Ï', "I"},    {'Ĩ', "I"},
            {'İ', "I"},    {'Ī', "I"},    {'Į', "I"},    {'Ĭ', "I"},    {'Ĵ', "J"},    {'Ķ', "K"},
            {'Ł', "L"},    {'Ĺ', "L"},    {'Ľ', "L"},    {'Ļ', "L"},    {'Ŀ', "L"},    {'Ń', "N"},
            {'Ñ', "N"},    {'Ň', "N"},    {'Ņ', "N"},    {'η', "N"},    {'Ø', "Oe"},   {'Ó', "O"},
            {'Ò', "O"},    {'Ô', "O"},    {'Ö', "Oe"},   {'Õ', "O"},    {'Ő', "O"},    {'Ō', "O"},
            {'Ŏ', "O"},    {'Ŕ', "R"},    {'Ř', "R"},    {'Ŗ', "R"},    {'Ś', "S"},    {'Ŝ', "S"},
            {'Š', "S"},    {'Ş', "S"},    {'Ŧ', "T"},    {'Ť', "T"},    {'Ţ', "T"},    {'Ú', "U"},
            {'Ù', "U"},    {'Û', "U"},    {'Ü', "Ue"},   {'Ũ', "U"},    {'Ŭ', "U"},    {'Ű', "U"},
            {'Ů', "U"},    {'Ū', "U"},    {'Ų', "U"},    {'Ŵ', "W"},    {'Ý', "Y"},    {'Ŷ', "Y"},
            {'Ÿ', "Y"},    {'Ź', "Z"},    {'Ž', "Z"},    {'Ż', "Z"},    {'Þ', "Th"},   {'Æ', "Ae"},
            {'¬', "Ij"},   {'Œ', "Oe"},   {'ß', "Ss"},

            {'á', "a"},    {'à', "a"},    {'â', "a"},    {'ä', "ae"},   {'ã', "a"},    {'ă', "a"},
            {'å', "aa"},   {'ā', "a"},    {'ą', "a"},    {'ć', "c"},    {'ĉ', "c"},    {'č', "c"},
            {'ċ', "c"},    {'ç', "c"},    {'ð', "d"},    {'ď', "d"},    {'é', "e"},    {'è', "e"},
            {'ê', "e"},    {'ë', "e"},    {'ĕ', "e"},    {'ė', "e"},    {'ē', "e"},    {'ę', "e"},
            {'ĝ', "g"},    {'ğ', "g"},    {'ġ', "g"},    {'ģ', "g"},    {'ħ', "h"},    {'ĥ', "h"},
          /*{'i', "i"},*/  {'í', "i"},    {'ì', "i"},    {'î', "i"},    {'ï', "i"},    {'ĩ', "i"},
          /*{'İ', "I"},*/  {'ī', "i"},    {'į', "i"},    {'ĭ', "i"},    {'ĵ', "j"},    {'ķ', "k"},
            {'ł', "l"},    {'ĺ', "l"},    {'ľ', "l"},    {'ļ', "l"},    {'ŀ', "l"},    {'ń', "n"},
            {'ñ', "n"},    {'ň', "n"},    {'ņ', "n"},  /*{'η', "N"},*/  {'ø', "oe"},   {'ó', "o"},
            {'ò', "o"},    {'ô', "o"},    {'ö', "oe"},   {'õ', "o"},    {'ő', "o"},    {'ō', "o"},
            {'ŏ', "o"},    {'ŕ', "r"},    {'ř', "r"},    {'ŗ', "r"},    {'ś', "s"},    {'ŝ', "s"},
            {'š', "s"},    {'ş', "s"},    {'ŧ', "t"},    {'ť', "t"},    {'ţ', "t"},    {'ú', "u"},
            {'ù', "u"},    {'û', "u"},    {'ü', "ue"},   {'ũ', "u"},    {'ŭ', "u"},    {'ű', "u"},
            {'ů', "u"},    {'ū', "u"},    {'ų', "u"},    {'ŵ', "w"},    {'ý', "y"},    {'ŷ', "y"},
            {'ÿ', "y"},    {'ź', "z"},    {'ž', "z"},    {'ż', "z"},    {'þ', "th"},   {'æ', "ae"},
         /*{'¬', "Ij"},*/  {'œ', "oe"}, /*{'ß', "Ss"},*/
            
            {'А', "A"},    {'Б', "B"},    {'В', "V"},    {'Г', "G"},    {'Д', "D"},    {'Е', "E"},
            {'Ё', "E"},    {'Ж', "Zh"},   {'З', "Z"},    {'И', "I"},    {'І', "I"},    {'Й', "I"},
            {'К', "K"},    {'Л', "L"},    {'М', "M"},    {'Н', "N"},    {'О', "O"},    {'П', "P"},
            {'Р', "R"},    {'С', "S"},    {'Т', "T"},    {'У', "U"},    {'Ф', "F"},    {'Х', "Kh"},
            {'Ц', "Ts"},   {'Ч', "Ch"},   {'Ш', "Sh"},   {'Щ', "Shch"}, {'Ы', "Y"},    {'Ъ', "Ie"},
            {'Э', "E"},    {'Ю', "Iu"},   {'Я', "Ia"}, /*{'V', "Y"},*/    {'Ґ', "G"},    {'ў', "U"},
            {'´', "U"},    {'ƒ', "G"},    {'Ћ', "D"},    {'Ѕ', "Dz"},   {'Ј', "J"},    {'Ќ', "K"},
            {'Љ', "Lj"},   {'Њ', "Nj"},   {'Һ', "C"},    {'Џ', "Dz"},   {'Є', "Ie"},   {'Ї', "I"},

            {'а', "a"},    {'б', "b"},    {'в', "v"},    {'г', "g"},    {'д', "d"},    {'е', "e"},
            {'ё', "e"},    {'ж', "zh"},   {'з', "z"},    {'и', "i"},    {'і', "i"},    {'й', "i"},
            {'к', "k"},    {'л', "l"},    {'м', "m"},    {'н', "n"},    {'о', "o"},    {'п', "p"},
            {'р', "r"},    {'с', "s"},    {'т', "t"},    {'у', "u"},    {'ф', "f"},    {'х', "kh"},
            {'ц', "ts"},   {'ч', "ch"},   {'ш', "sh"},   {'щ', "shch"}, {'ы', "y"},    {'ъ', "ie"},
            {'э', "e"},    {'ю', "iu"},   {'я', "ia"}, /*{'v', "y"},*/  {'ґ', "g"},  /*{'ў', "U"},*/
          /*{'´', "U"},*//*{'ƒ', "G"},*/  {'ћ', "d"},    {'ѕ', "dz"},   {'ј', "j"},    {'ќ', "k"},
            {'љ', "lj"},   {'њ', "nj"},   {'һ', "c"},    {'џ', "dz"},   {'є', "ie"},   {'ї', "i"},
        };

        #endregion

        /// <summary>
        /// Transliterate <paramref name="input"/> symbol. Based on ICAO standard.
        /// </summary>
        public static string TransliterateIcao(char input)
        {
            if (input < 0x80)
            {
                return new string(input, 1);
            }
            string translit;
            return IcaoCharMap.TryGetValue(input, out translit) ? translit : "";
        }

        /// <summary>
        /// Transliterate <paramref name="input"/> string symbol by symbol. Based on ICAO standard.
        /// </summary>
        public static string TransliterateIcao(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return "";
            }
            if (input.All(x => x < 0x80))
            {
                return input;
            }

            List<string> result = input
                .Select(c => TransliterateIcao(c))
                .Where(s => !String.IsNullOrEmpty(s))
                .ToList();

            for (int i = 0, end = result.Count - 1; i <= end; ++i)
            {
                // if first char in part of result is upper we must choose
                // between all upper (e.g. Æ → AE) and first upper (e.g. Æ → Ae)
                if (Char.IsUpper(result[i], 0))
                {
                    bool isStart = (i == 0) || !Char.IsLetter(result[i - 1], 0);
                    bool isEnd = (i == end) || !Char.IsLetter(result[i + 1], 0);

                    // in end of the word look at previous symbol otherwise look at next symbol
                    if (isEnd && !isStart && Char.IsUpper(result[i - 1], 0) || Char.IsUpper(result[i + 1], 0))
                    {
                        // if neighbour symbol is upper make all chars in current symbol upper too
                        result[i] = result[i].ToUpper();
                    }
                }
            }

            return String.Join("", result);
        }
    }
}
