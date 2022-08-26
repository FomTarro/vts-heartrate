using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

namespace Localization
{
    public class LocalizationManager : Singleton<LocalizationManager> {

        [Header("Strings")]
        [SerializeField]
        private TextAsset _stringTable;
        /// <summary>
        /// Character that denotes a string as a key
        /// 
        /// Used for embedded keys in other strings for dynamic replacement
        /// </summary>
        private const char REPLACEMENT_KEY_TAG = '%';
        private const int REPLACEMENT_CYCLE_MAX = 1024;

        // <summary>
        /// The default value for a string key that can't be found in the table
        /// </summary>
        public const string DUMMY_STRING = "STRING_NOT_FOUND";

        /// <summary>
        /// List of key:value pairs for loaded language sets
        /// 
        /// Each item in this list represents a distinct language set
        /// </summary>
        private List<Dictionary<string, string>> _loadedStrings;

        private int _languageIndex = 0;
        public SupportedLanguage CurrentLanguage
        {
            get { return (SupportedLanguage)(_languageIndex + 1);}
        }

        /// <summary>
        /// List of all registered LocalizedText components in the scene, 
        /// used to publish events to them when the language changes
        /// </summary>
        private static List<LocalizedText> _localizedTexts = new List<LocalizedText>();

        private bool _initialized = false;

        // Use this for initialization
        public override void Initialize()
        {
            if(!_initialized)
            {
                _loadedStrings = new List<Dictionary<string, string>>();
                ParseTable(_stringTable);
                _initialized = true;
                // refresh

                // We can either initialize by choosing the first language, 
                // or by choosing the system language
                //SwitchLanguage(_languageIndex);
                SwitchLanguage(Application.systemLanguage);
            }
        }

        public static void RegisterLocalizedText(LocalizedText text)
        {
            if(!_localizedTexts.Contains(text))
            {
                _localizedTexts.Add(text);
            }
        }

        public static void UnregisterLocalizedText(LocalizedText text)
        {
            if(_localizedTexts.Contains(text))
            {
                _localizedTexts.Remove(text);
            }
        }

        public void RefreshAllLocalizedText(){
            foreach(LocalizedText t in _localizedTexts)
            {
                if(t.gameObject.activeInHierarchy)
                {
                    t.Localize();
                }
            }
        }

        /// <summary>
        /// Switch the language to a different language index
        /// </summary>
        /// <param name="language">The numeric index in the list of languages</param>
        private void SwitchLanguage(int language)
        {
            _languageIndex = language;
            RefreshAllLocalizedText();
        }

        /// <summary>
        /// Switch the language to a different language index
        /// </summary>
        /// <param name="language">The specific supported language</param>
        public void SwitchLanguage(SupportedLanguage language)
        {
            SwitchLanguage(((int)language) - 1);
        }

        /// <summary>
        /// Switches to the best language for the given system language settings, if possible
        /// 
        /// Defaults to English if no match can be found
        /// </summary>
        /// <param name="sysLanguage">The given system language</param>
        public void SwitchLanguage(SystemLanguage language){
            SwitchToSystemLanguage(language);
        }

        /// <summary>
        /// Returns true if the current language is fullwidth, false if halfwidth
        /// </summary>
        /// <returns></returns>
        public bool IsFullwidth(){
            return false;
            // return this.CurrentLanguage == SupportedLanguage.JAPANESE;
        }

        /// <summary>
        /// Switches to the best language for the given system language settings, if possible
        /// 
        /// Defaults to English if no match can be found
        /// </summary>
        /// <param name="sysLanguage"></param>
        private void SwitchToSystemLanguage(SystemLanguage sysLanguage)
        {
            SupportedLanguage lang = SupportedLanguage.ENGLISH;
            switch(sysLanguage)
            {
                case SystemLanguage.English:
                    lang = SupportedLanguage.ENGLISH;
                    break;
                // case SystemLanguage.Japanese:
                //     lang = SupportedLanguage.JAPANESE;
                //     break;
                // case SystemLanguage.Italian:
                //     lang = SupportedLanguage.ITALIAN;
                //     break;
                // case SystemLanguage.Spanish:
                //     lang = SupportedLanguage.SPANISH;
                //     break;
                // case SystemLanguage.French:
                //     lang = SupportedLanguage.FRENCH;
                //     break;
                // case SystemLanguage.Chinese:
                // case SystemLanguage.ChineseTraditional:
                // case SystemLanguage.ChineseSimplified:
                //     lang = SupportedLanguage.CHINESE;
                //     break;
                default:
                    lang = SupportedLanguage.ENGLISH;
                    break;
            }
            SwitchLanguage(lang);
        }

        /// <summary>
        /// Is the current language a right-to-left language, such as Arabic?
        /// </summary>
        /// <returns></returns>
        public bool IsRightToLeft()
        {
            return false;
            //return CurrentLanguage == (SupportedLanguage.ARABIC);
        }

        /// <summary>
        /// Adds the set of key/value pairs to the table, for the given language.
        /// 
        /// Useful for adding dynamic data that can be embedded in existing strings
        /// 
        /// If a key is already present, it will be replaced with the new value
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="language"></param>
        public void AddStrings(Dictionary<string, string> strings, SupportedLanguage language)
        {
            foreach(string key in strings.Keys)
            {
                if(strings[key] != null){
                    if (_loadedStrings[(int)(language-1)].ContainsKey(key))
                    {

                        _loadedStrings[(int)(language-1)][key] = strings[key];
                    }
                    else
                    {
                        _loadedStrings[(int)(language-1)].Add(key, strings[key]);
                    }
                }
            }
        }

        /// <summary>
        /// Loads a table of strings in to the master table
        /// 
        /// If a key is already present, it will be replaced with the new value
        /// </summary>
        /// <param name="strings"></param>
        /// <param name="language"></param>
        public void LoadStringTable(TextAsset stringTable)
        {
            ParseTable(stringTable);
        }

        /// <summary>
        /// Gets a string from the current language table based on the given key
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <returns></returns>
        public string GetString(string key)
        {
            return GetString(key, _languageIndex);
        }

        /// <summary>
        /// Gets a string from the given language table based on the given key.
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="language">The language to reference</param>
        /// <returns></returns>
        public string GetString(string key, SupportedLanguage language)
        {
            return GetString(key, ((int)language) - 1);
        }

        /// <summary>
        /// Gets a string from the current language table based on the given key.
        /// </summary>
        /// <param name="key">The key to look up</param>
        /// <param name="language">The language to reference</param>
        /// <returns></returns>
        private string GetString(string key, int language)
        {
            string value = DUMMY_STRING;
            key = key.ToLower();
            if (_loadedStrings != null && _loadedStrings.Count > language && _loadedStrings[language].ContainsKey(key))
            {
                value = _loadedStrings[language][key];
                value = RecursiveReplace(value, 0, language, 0);
            }
            else
            {
                Debug.LogWarning("String not found in table for key: " + key);
            }
            if(IsRightToLeft())
            {
                // here, you can insert your own method of doing RTL text cleanup.
                // I'd recommend using https://github.com/Konash/arabic-support-unity 
                // big thanks to Open Source code from Abdullah Konash!
            }
            if(IsFullwidth()){
                // List<string> split = Mikan.Mikan.Split(value);
                // value = String.Join("\u200B", split); // the no-width space character
            }
            return value;
        }

        /// <summary>
        /// Does the current language contain the given key?
        /// </summary>
        /// <param name="key">The key to check for</param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return _loadedStrings != null && _loadedStrings.Count > _languageIndex && _loadedStrings[_languageIndex].ContainsKey(key);
        }

        /// <summary>
        /// Used for dynamically replacing embedded keys in evaluated strings, for example:
        /// 
        /// %player_name => "Tom"
        /// 
        /// %greeting => "Hello, %player_name!" => "Hello, Tom!"
        /// </summary>
        /// <param name="toModify">String to search</param>
        /// <param name="startIndex">Where in the string to start replacement</param>
        /// <param name="languageIndex">Which language to do replacement from</param>
        /// <param name="cycleCount">How many levels deep we should try to replace before aborting</param>
        /// <returns></returns>
        private string RecursiveReplace(string toModify, int startIndex, int languageIndex, int cycleCount)
        {
            string modifySubstring = toModify.Substring(startIndex);
            if (cycleCount < REPLACEMENT_CYCLE_MAX && modifySubstring.Contains(REPLACEMENT_KEY_TAG.ToString()) && startIndex < toModify.Length)
            {
                string keySubstring = modifySubstring.Substring(modifySubstring.IndexOf(REPLACEMENT_KEY_TAG) + 1);
                int punctuationIndex = GetIndexOfPunctuation(keySubstring);
                int endIndex = (punctuationIndex > 0) ? Mathf.Min(punctuationIndex, keySubstring.Length) : keySubstring.Length;
                keySubstring = keySubstring.Substring(0, endIndex);
                if (_loadedStrings[languageIndex].ContainsKey(keySubstring.ToLower()))
                {
                    toModify = toModify.Replace(REPLACEMENT_KEY_TAG + keySubstring, GetString(keySubstring, languageIndex));
                }
                cycleCount++;
                return RecursiveReplace(toModify, startIndex + 1, languageIndex, cycleCount);
            }
            else
            {
                return toModify;
            }
        }

        private void ReadLine(int lineIndex, List<string> line)
        {
            line[0] = line[0].Trim().ToLower();
            if (line[0] != "")
            {
                if(GetIndexOfPunctuation(line[0]) == -1){
                    for (int i = 1; i < line.Count; i++)
                    {
                        if (_loadedStrings.Count < i)
                        {
                            _loadedStrings.Add(new Dictionary<string, string>());
                        }
                        if (line[i].Trim() != "")
                        {
                            try 
                            {
                                line[i] = line[i].Replace("\\n", "\n");
                                if(_loadedStrings[i - 1].ContainsKey(line[0]))
                                {
                                    _loadedStrings[i - 1][line[0]] = line[i];
                                }
                                else
                                {
                                    _loadedStrings[i - 1].Add(line[0], line[i]);
                                }
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning(e.Message + "\n" + line[0] + "\n" + line[i]);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Key: " + line[0] + " contains disallowed punctuation!");
                }
            }
        }

        private void ParseTable(TextAsset stringTable)
        {
            string tableContent = stringTable.text;
            int tableLength = tableContent.Length;
            // read char by char and when a , or \n, perform appropriate action
            int currentCharIndex = 0; // index in the file
            List<string> currentLine = new List<string>(); // current line of data
            int currentLineCount = 0;
            StringBuilder currentItem = new StringBuilder();
            bool inQuotes = false; // managing quotes
            char currentChar;
            while (currentCharIndex < tableLength)
            {
                currentChar = tableContent[currentCharIndex++];
                switch (currentChar)
                {
                    case '"':
                        if (!inQuotes)
                        {
                            inQuotes = true;
                        }
                        else
                        {
                            if (currentCharIndex == tableLength)
                            {
                                // end of file
                                inQuotes = false;
                                goto case '\n';
                            }
                            else if (tableContent[currentCharIndex] == '"')
                            {
                                // double quote, save one
                                currentItem.Append("\"");
                                currentCharIndex++;
                            }
                            else
                            {
                                // leaving quotes section
                                inQuotes = false;
                            }
                        }
                        break;
                    case '\r':
                        // ignore it completely
                        break;
                    case ',':
                        goto case '\n';
                    case '\n':
                        if (inQuotes)
                        {
                            // inside quotes, this characters must be included
                            currentItem.Append(currentChar);
                        }
                        else
                        {
                            // end of current item
                            currentLine.Add(currentItem.ToString());
                            currentItem.Length = 0;
                            if (currentChar == '\n' || currentCharIndex == tableLength)
                            {
                                // also end of line, call line reader
                                ReadLine(currentLineCount++, currentLine);
                                currentLine.Clear();
                            }
                        }
                        break;
                    default:
                        // other cases, add char
                        currentItem.Append(currentChar);
                        break;
                }
            }
        }


        private static Regex REGEX_PUNCTUATION_FILTER = new Regex("[「」?.,!#\"/\\ '<>#\t\n;:]+");
        /// <summary>
        /// Return the index of the first punctuation character in a string. Returns -1 if none can be found.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static int GetIndexOfPunctuation(string input)
        {
            if(REGEX_PUNCTUATION_FILTER.IsMatch(input))
            {
                return REGEX_PUNCTUATION_FILTER.Match(input).Index;
            }
            else
            {
                return -1;
            }
        }


    }

    /// <summary>
    /// Enum representing language of localization
    /// 
    /// int value corresponds the column of the language in the source CSV file (therefore 1 indexed, as column 0 is the key list)
    /// </summary>
    public enum SupportedLanguage : int
    {
        ENGLISH = 1,
        // JAPANESE = 2,
    }

}