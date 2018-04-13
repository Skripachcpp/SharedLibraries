using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WorkingTools.Classes
{
    public static class CodeConverter
    {
        private static Dictionary<string, string> PrepareTranslit()
        {
            var conversionScheme = new Dictionary<string, string>();
            // /* выпиливаем недопустимые в url-э символы
            //conversionScheme.Add("«", "");
            //conversionScheme.Add("»", "");

            //conversionScheme.Add("\n", "");
            //conversionScheme.Add("\r", "");
            //conversionScheme.Add("\t", "");

            //conversionScheme.Add("/", "");
            //conversionScheme.Add(@"\", "");
            //conversionScheme.Add("|", "");

            //conversionScheme.Add("'", "");
            //conversionScheme.Add("\"", "");
            //conversionScheme.Add("`", "");

            //conversionScheme.Add("]", "");
            //conversionScheme.Add("[", "");
            //conversionScheme.Add("{", "");
            //conversionScheme.Add("}", "");
            //conversionScheme.Add("(", "");
            //conversionScheme.Add(")", "");
            //conversionScheme.Add("<", "");
            //conversionScheme.Add(">", "");

            //conversionScheme.Add("@", "");
            //conversionScheme.Add("#", "");
            //conversionScheme.Add("№", "");
            //conversionScheme.Add("&", "");
            //conversionScheme.Add("^", "");
            //conversionScheme.Add("%", "");
            //conversionScheme.Add("$", "");
            //conversionScheme.Add("*", "");
            //conversionScheme.Add("~", "");

            //conversionScheme.Add("?", "");
            //conversionScheme.Add("!", "");
            //conversionScheme.Add(".", "");
            //conversionScheme.Add(",", "");
            //conversionScheme.Add(";", "");
            //conversionScheme.Add(":", "");

            //conversionScheme.Add("=", "");
            //conversionScheme.Add("+", "");
            // */
            conversionScheme.Add("í", "i");
            conversionScheme.Add("á", "a");
            conversionScheme.Add("ń", "n");
            conversionScheme.Add("ѓ", "r");
            conversionScheme.Add("ä", "a");
            conversionScheme.Add("ö", "o");
            conversionScheme.Add("ü", "u");
            conversionScheme.Add("ї", "i");
            conversionScheme.Add("ӥ", "j");
            conversionScheme.Add("ĉ", "c");
            conversionScheme.Add("ĝ", "g");
            conversionScheme.Add("ĥ", "h");
            conversionScheme.Add("ĵ", "j");
            conversionScheme.Add("î", "i");
            conversionScheme.Add("ê", "e");
            conversionScheme.Add("ŝ", "s");
            conversionScheme.Add("â", "a");
            conversionScheme.Add("ž", "z");
            conversionScheme.Add("ě", "e");
            conversionScheme.Add("š", "s");
            conversionScheme.Add("č", "c");
            conversionScheme.Add("ǎ", "a");
            conversionScheme.Add("ѯ", "s");
            conversionScheme.Add("ő", "o");
            conversionScheme.Add("ű", "u");
            conversionScheme.Add("ѷ", "v");
            conversionScheme.Add("å", "a");
            conversionScheme.Add("ů", "u");
            conversionScheme.Add("ż", "z");
            conversionScheme.Add("ė", "e");
            conversionScheme.Add("ḃ", "b");
            conversionScheme.Add("ḋ", "d");
            conversionScheme.Add("ḟ", "f");
            conversionScheme.Add("ṁ", "m");
            conversionScheme.Add("ṗ", "p");
            conversionScheme.Add("ṫ", "t");
            conversionScheme.Add("ñ", "n");
            conversionScheme.Add("õ", "o");
            conversionScheme.Add("ᾶ", "a");
            conversionScheme.Add("ā", "a");
            conversionScheme.Add("ă", "a");
            conversionScheme.Add("ӂ", "zh");
            conversionScheme.Add("ğ", "g");
            conversionScheme.Add("ŭ", "u");
            conversionScheme.Add("ὡ", "w");
            conversionScheme.Add("ῥ", "p");
            conversionScheme.Add("ὀ", "o");
            conversionScheme.Add("ả", "a");
            conversionScheme.Add("ḥ", "h");
            conversionScheme.Add("ș", "s");
            conversionScheme.Add("ț", "t");
            conversionScheme.Add("r̥", "r");
            conversionScheme.Add("u̯", "u");
            conversionScheme.Add("a̱", "a");
            conversionScheme.Add("s̬", "s");
            conversionScheme.Add("ᾳ", "a");
            conversionScheme.Add("ơ", "o");
            conversionScheme.Add("ґ", "r");
            conversionScheme.Add("ç", "c");
            conversionScheme.Add("ş", "s");
            conversionScheme.Add("ģ", "g");
            conversionScheme.Add("ę", "e");
            conversionScheme.Add("đ", "d");
            conversionScheme.Add("ø", "o");
            conversionScheme.Add("ł", "l");
            conversionScheme.Add("ҝ", "k");
            conversionScheme.Add("ɫ", "l");

            conversionScheme.Add("ú", "u");
            conversionScheme.Add("é", "e");

            conversionScheme.Add("1", "1");
            conversionScheme.Add("2", "2");
            conversionScheme.Add("3", "3");
            conversionScheme.Add("4", "4");
            conversionScheme.Add("5", "5");
            conversionScheme.Add("6", "6");
            conversionScheme.Add("7", "7");
            conversionScheme.Add("8", "8");
            conversionScheme.Add("9", "9");
            conversionScheme.Add("0", "0");

            conversionScheme.Add("–", "_");
            conversionScheme.Add("-", "_");
            conversionScheme.Add(" ", "_");

            //транслитирация русского языка
            conversionScheme.Add("а", "a");
            conversionScheme.Add("б", "b");
            conversionScheme.Add("в", "v");
            conversionScheme.Add("г", "g");
            conversionScheme.Add("д", "d");
            conversionScheme.Add("е", "e");
            conversionScheme.Add("ё", "yo");
            conversionScheme.Add("ж", "zh");
            conversionScheme.Add("з", "z");
            conversionScheme.Add("и", "i");
            conversionScheme.Add("й", "j");
            conversionScheme.Add("к", "k");
            conversionScheme.Add("л", "l");
            conversionScheme.Add("м", "m");
            conversionScheme.Add("н", "n");
            conversionScheme.Add("о", "o");
            conversionScheme.Add("п", "p");
            conversionScheme.Add("р", "r");
            conversionScheme.Add("с", "s");
            conversionScheme.Add("т", "t");
            conversionScheme.Add("у", "u");
            conversionScheme.Add("ф", "f");
            conversionScheme.Add("х", "h");
            conversionScheme.Add("ц", "c");
            conversionScheme.Add("ч", "ch");
            conversionScheme.Add("ш", "sh");
            conversionScheme.Add("щ", "sch");
            conversionScheme.Add("ъ", "j");
            conversionScheme.Add("ы", "i");
            conversionScheme.Add("ь", "j");
            conversionScheme.Add("э", "e");
            conversionScheme.Add("ю", "yu");
            conversionScheme.Add("я", "ya");

            //английский оставляем без изменений
            conversionScheme.Add("a", "a");
            conversionScheme.Add("b", "b");
            conversionScheme.Add("c", "c");
            conversionScheme.Add("d", "d");
            conversionScheme.Add("e", "e");
            conversionScheme.Add("f", "f");
            conversionScheme.Add("g", "g");
            conversionScheme.Add("h", "h");
            conversionScheme.Add("i", "i");
            conversionScheme.Add("j", "j");
            conversionScheme.Add("k", "k");
            conversionScheme.Add("l", "l");
            conversionScheme.Add("m", "m");
            conversionScheme.Add("n", "n");
            conversionScheme.Add("o", "o");
            conversionScheme.Add("p", "p");
            conversionScheme.Add("q", "q");
            conversionScheme.Add("r", "r");
            conversionScheme.Add("s", "s");
            conversionScheme.Add("t", "t");
            conversionScheme.Add("u", "u");
            conversionScheme.Add("v", "v");
            conversionScheme.Add("w", "w");
            conversionScheme.Add("x", "x");
            conversionScheme.Add("y", "y");
            conversionScheme.Add("z", "z");
            // */

            return conversionScheme;
        }

        private static readonly Dictionary<string, string> _conversionScheme = PrepareTranslit();

        //TODO: тут могут проскочить какие ни будь спец символы .. надо бы регулярку запелить, но пока не до того
        public static string Convert(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText)) return null;
            sourceText = sourceText.Clean();
            if (string.IsNullOrEmpty(sourceText)) return null;
            sourceText = sourceText.ToLower();

            var ans = new StringBuilder();
            foreach (char t in sourceText)
            {
                if (_conversionScheme.ContainsKey(t.ToString())) ans.Append(_conversionScheme[t.ToString()]);
                //else ans.Append(t.ToString());//символы не найденные в таблице сопоставлений игнориреум
            }

            return ans.ToString().ToLower();
        }
    }

    public static class StringExt
    {
        private static readonly Regex trimSpaceRegex = new Regex(@"[ ]+", RegexOptions.Compiled);
        public static string Clean(this string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return null;

            v = v.Replace("\n", "").Replace("\r", "").Replace("\t", "").Replace(' ', ' ').Replace("й", "й").Trim(' ');//да, тут должно быть 2 Trim, и там не 2 пробелла, они чем то отличаются
            v = trimSpaceRegex.Replace(v, " ");//удаляем двойные пробеллы

            if (string.IsNullOrWhiteSpace(v)) return null;

            return v;
        }
    }
}
