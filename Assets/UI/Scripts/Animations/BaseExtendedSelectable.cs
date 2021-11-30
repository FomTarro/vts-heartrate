using UnityEngine;
using UnityEngine.UI;

namespace UI.InputTools
{
    [RequireComponent(typeof(Selectable))]
    public abstract class BaseExtendedSelectable<T> : MonoBehaviour where T : Selectable
    {

        [Header("Base Properties")]
        [SerializeField]
        private Graphic[] _graphics = new Graphic[0];
        private Shadow _shadow = null;
        private Image _bottomLevelImage = null;

        [SerializeField]
        private Image _topLevelImage = null;
        private void OnValidate()
        {
            // keep the "original" button image and the one that goes OVER the outline effect in sync
            _bottomLevelImage = _bottomLevelImage == null ? _bottomLevelImage = GetComponent<Image>() : _bottomLevelImage;
            if(_topLevelImage != null)
            {
                _topLevelImage.sprite = _bottomLevelImage.sprite;
                _topLevelImage.color = _bottomLevelImage.color;
            }
        }

        private bool[] _oldLayerStates = new bool[0];
        private bool[] _newLayerStates = new bool[0];
        private CanvasGroup[] _groups = new CanvasGroup[0];
        
        private T _baseSelectable = null;
        public T Selectable
        {
            get { return _baseSelectable == null ? _baseSelectable = GetComponent<T>() : _baseSelectable; }
        }

        private void OnEnable()
        {
            _groups = GetComponentsInParent<CanvasGroup>();
            _oldLayerStates = new bool[_groups.Length+1];
            _oldLayerStates[0] = true;
            _newLayerStates = new bool[_oldLayerStates.Length];
            _shadow = GetComponent<Shadow>();
            OnEnableImplementation();
        }

        protected abstract void OnEnableImplementation();

        // Update is called once per frame
        private void LateUpdate()
        {
            _newLayerStates = GetLayerStates();
            // if any of the states have changed
            if(!SequenceEquals(_oldLayerStates, _newLayerStates))
            {
                // if no layers are not interactable
                if(IsInteractable(_newLayerStates))
                {
                    if(_shadow != null)
                    {
                        _shadow.enabled = true;
                    }
                    Selectable.targetGraphic.CrossFadeColor(Selectable.colors.colorMultiplier * Selectable.colors.normalColor, 0f, true, true);
                    foreach(Graphic g in _graphics)
                    {
                        switch(g){
                            case Text text:
                                text.CrossFadeAlpha(text.color.a, 0f, true);
                                break;
                            default:
                                g.CrossFadeColor(Selectable.colors.colorMultiplier * Selectable.colors.normalColor, 0f, true, true);
                                break;
                        }
                    }
                }
                else
                {
                    if(_shadow != null)
                    {
                        _shadow.enabled = false;
                    }
                    Selectable.targetGraphic.CrossFadeColor(Selectable.colors.colorMultiplier * Selectable.colors.disabledColor, 0f, true, true);
                    foreach(Graphic g in _graphics)
                    {
                        switch(g){
                            case Text text:
                                text.CrossFadeAlpha(Selectable.colors.disabledColor.a, 0f, true);
                                break;
                            default:
                                g.CrossFadeColor(Selectable.colors.colorMultiplier * Selectable.colors.disabledColor, 0f, true, true);
                                break;
                        }
                    }
                }
                _oldLayerStates = _newLayerStates;
            }
            UpdateImplementation();
        }

        protected abstract void UpdateImplementation();

        /// <summary>
        /// Is the selectable currently interactable?
        /// </summary>
        /// <returns></returns>
        public bool IsInteractable()
        {
            return IsInteractable(_newLayerStates);
        }

        private bool IsInteractable(bool[] states)
        {
            foreach(bool b in states)
            {
                if(b == false)
                {
                    return false;
                }
            }
            return true;
        }

        private bool SequenceEquals(bool[] a, bool[] b)
        {
            if(a.Length != b.Length)
            {
                return false;
            }
            bool eq = true;
            for(int i = 0; i < a.Length; i++){
                eq &= (a[i] == b[i]);
            }
            return eq;
        }

        private bool[] GetLayerStates()
        {
            bool ignoreParent = false;
            bool[] newLayerStates = new bool[_newLayerStates.Length];
            newLayerStates[0] = _baseSelectable.IsInteractable();
            int i = 1;
            foreach(CanvasGroup g in _groups)
            {
                newLayerStates[i] = ignoreParent ? true : g.interactable;
                if(g.ignoreParentGroups)
                {
                    ignoreParent = true;
                }
                i++;
            }
            return newLayerStates;
        }

    }
}
