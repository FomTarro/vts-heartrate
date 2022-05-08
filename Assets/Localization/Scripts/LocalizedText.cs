using UnityEngine.UI;
using UnityEngine;
using System.Text;
using TMPro;

namespace Localization
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour {

        [SerializeField]
        private string key = string.Empty;
        public string Key { get { return this.key; } }

        private TMP_Text _text;

        [SerializeField]
        private string _prefix = "";

        [SerializeField]
        private string _postfix = "";

        void Awake()
        {
            _text = GetComponent<TMP_Text>();
            LocalizationManager.RegisterLocalizedText(this);
        }

        void OnEnable()
        {
            Localize();
        }

        void OnDestroy()
        {
            LocalizationManager.UnregisterLocalizedText(this);
        }

        /// <summary>
        /// Change the lookup key for the text and re-localize
        /// </summary>
        /// <param name="newKey"></param>
        public void ChangeKey(string newKey)
        {
            key = newKey;
            Localize();
        }

        public void SetPostfix(string postfix){
            this._postfix = postfix;
        }

        /// <summary>
        /// Updates the display of the associated text object based on current language settings
        /// </summary>
        public void Localize()
        {
            if (_text == null)
            {
                _text = GetComponent<TMP_Text>();
            }
            string val = LocalizationManager.Instance.GetString(key);
            _text.text = _prefix + val + _postfix;
        }
    }
}