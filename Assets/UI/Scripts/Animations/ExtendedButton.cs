using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UI.Animation;

namespace UI.InputTools
{

    [RequireComponent(typeof(AnimatedScale), typeof(Button))]
    public class ExtendedButton : BaseExtendedSelectable<Button>, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {

        [Header("Visuals")]
        [SerializeField]
        private AnimationCurve downCurve;
        [SerializeField]
        private AnimationCurve upCurve;

        private AnimatedScale _animScale;
        private Vector3 _desiredShrinkScale;
        
        private bool _hovering = false;
        
        [Header("Audio")]
        [SerializeField]
        private AudioClip _downClip;
        public AudioClip DownClip 
        {
            get { return _downClip; }
            set { _downClip = value; }
        }
        [SerializeField]
        private AudioClip _releaseClip;
        public AudioClip UpClip
        {
            get { return _releaseClip; }
            set { _releaseClip = value; }
        }

        [Header("Actions")]
        public UnityEvent onPointerDown;
        public UnityEvent onPointerUp;

        /// <summary>
        /// A static reference is used to make sure only the most recently clicked button's callback 
        /// is actually invoked after the animation delay
        /// </summary>
        private static Button _lastButtonClicked;

        public UnityEvent onClick
        {
            get { return Selectable.onClick; }
        }

        private UnityEvent _callback;

        private void Start()
        {
            _animScale = _animScale == null ? GetComponent<AnimatedScale>() : _animScale;
            _desiredShrinkScale = _animScale.newScale;
            _animScale.SetCurve(downCurve);
            Selectable.enabled = false;
        }

        protected override void OnEnableImplementation()
        {
            _animScale = _animScale == null ? GetComponent<AnimatedScale>() : _animScale;
            _animScale.SetCurve(downCurve);
            Selectable.enabled = false;
        }

        protected override void UpdateImplementation(){}
        
        public void OnPointerClick(PointerEventData data)
        {
    
        }

        public void OnPointerDown(PointerEventData data)
        {
            if(Selectable.IsInteractable())
            {
                //data.Use();
                _animScale.StopAnimation();
                _animScale.SetCurve(downCurve);
                _animScale.newScale = _desiredShrinkScale;
                _animScale.StartAnimation();
                // SoundManager.Instance.PlaySoundEffect(_downClip);
                if(onPointerDown != null)
                {
                    onPointerDown.Invoke();
                }
            }
        }

        public void OnPointerUp(PointerEventData data)
        {
            if(Selectable.IsInteractable())
            {
                _lastButtonClicked = Selectable;
                // SoundManager.Instance.PlaySoundEffect(_releaseClip);
                // if we release our cursor elsewhere, we probably didnt mean to click, so we shouldn't do the callback
                // callback is also a member variable so that we don't execute like ten of them if you mash the button
                _callback = (_hovering) ? Selectable.onClick : null;
                _animScale.StopAnimation();
                _animScale.SetCurve(upCurve);
                _animScale.newScale = Vector3.one;
                _animScale.StartAnimation(() =>
                { 
                    if(_callback != null && _lastButtonClicked.Equals(Selectable) && Selectable.image.raycastTarget)
                    {
                        if(onPointerUp != null)
                        {
                            onPointerUp.Invoke();
                        }
                        _callback.Invoke();
                    }
                    _callback = null;
                });
            }
        }

        public void OnPointerEnter(PointerEventData data)
        {
            _hovering = true;
        }

        public void OnPointerExit(PointerEventData data)
        {
            _hovering = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
        
        }

        public void OnEndDrag(PointerEventData eventData)
        {

        }

        public void OnBeginDrag(PointerEventData eventData)
        {

        }
    }
}
