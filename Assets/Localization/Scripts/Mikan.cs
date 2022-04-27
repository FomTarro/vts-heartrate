using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mikan
{
    /// <summary>
    /// A C# converted version of <![CDATA[https://github.com/trkbt10/mikan.js]]> which is a converted version of <![CDATA[https://github.com/google/budou]]>
    /// </summary>
    public class Mikan
    {
        static string joshi =
            @"(でなければ|について|かしら|くらい|けれど|なのか|ばかり|ながら|ことよ|こそ|こと|さえ|しか|した|たり|だけ|だに|だの|つつ|ても|てよ|でも|とも|から|など|なり|ので|のに|ほど|まで|もの|やら|より|って|で|と|な|に|ね|の|も|は|ば|へ|や|わ|を|か|が|さ|し|ぞ|て)";

        static string keywords = @"(\&nbsp;|[a-zA-Z0-9]+\.[a-z]{2,}|[一-龠々〆ヵヶゝ]+|[ぁ-んゝ]+|[ァ-ヴー]+|[a-zA-Z0-9]+|[ａ-ｚＡ-Ｚ０-９]+)";
        static string periods = @"([\.\,。、！\!？\?]+)$";
        static string bracketsBegin = @"([〈《「『｢（(\[【〔〚〖〘❮❬❪❨(<{❲❰｛❴])";
        static string bracketsEnd = @"([〉》」』｣)）\]】〕〗〙〛}>\)❩❫❭❯❱❳❵｝])";


        /// <summary>
        /// Split words into pieces
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks>A step-by-step analysis is in <![CDATA[https://beta.observablehq.com/d/9e6e7324c7882977]]></remarks>
        public static List<string> Split(string str)
        {

            var line1 = Regex.Split(str, keywords).ToList();
            var line2 = line1.SelectMany((o, _) => Regex.Split(o, joshi)).ToList();
            var line3 = line2.SelectMany((o, _) => Regex.Split(o, bracketsBegin)).ToList();
            var line4 = line3.SelectMany((o, _) => Regex.Split(o, bracketsEnd)).ToList();
            var words = line4.Where(o => !string.IsNullOrEmpty(o)).ToList();

            var prevType = string.Empty;
            var prevWord = string.Empty;
            List<string> result = new List<string>();

            words.ForEach(word =>
            {
                var token = Regex.IsMatch(word, periods) || Regex.IsMatch(word, joshi);

                if (Regex.IsMatch(word, bracketsBegin))
                {
                    prevType = "braketBegin";
                    prevWord = word;
                    return;
                }

                if (Regex.IsMatch(word, bracketsEnd))
                {
                    result[result.Count - 1] += word;
                    prevType = "braketEnd";
                    prevWord = word;
                    return;
                }

                if (prevType == "braketBegin")
                {
                    word = prevWord + word;
                    prevWord = string.Empty;
                    prevType = string.Empty;
                }

                // すでに文字が入っている上で助詞が続く場合は結合する
                if (result.Count > 0 && token && prevType == string.Empty)
                {
                    result[result.Count - 1] += word;
                    prevType = "keyword";
                    prevWord = word;
                    return;
                }

                // 単語のあとの文字がひらがななら結合する
                if (result.Count > 1 && token || (prevType == "keyword" && Regex.IsMatch(word, @"[ぁ-んゝ]+")))
                {
                    result[result.Count - 1] += word;
                    prevType = string.Empty;
                    prevWord = word;
                    return;
                }

                result.Add(word);
                prevType = "keyword";
                prevWord = word;
            });
            return result;
        }

    }
}