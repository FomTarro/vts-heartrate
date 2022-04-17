using UnityEngine.UI;
using UnityEngine;
using System.Text;

namespace Localization
{
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour {

        [SerializeField]
        private string key = string.Empty;

        private Text _text;

        void Awake()
        {
            _text = GetComponent<Text>();
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

        /// <summary>
        /// Updates the display of the associated text object based on current language settings
        /// </summary>
        public void Localize()
        {
            if (_text == null)
            {
                _text = GetComponent<Text>();
            }
            string val = LocalizationManager.Instance.GetString(key);
            _text.text = val;
        }
    }
}