using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Animation
{

    [RequireComponent(typeof(CanvasGroup))]
    public class AnimatedFlash : AnimatedElement {

        // make this an array of groups? 
        private CanvasGroup _group;
        public CanvasGroup GroupToFade
        {
            get { return _group; }
        }
        
        private float _initialAlpha;

        [Tooltip("The time in seconds to fade in. Will use global UI Fade Time if set to less than zero")]
        /// <summary>
        /// The time in seconds to fade in. Will use global UI Fade Time if set to less than zero
        /// </summary>
        public float fadeInTime;

        [Tooltip("The time in seconds to fade out. Will use global UI Fade Time if set to less than zero")]
        /// <summary>
        /// The time in seconds to fade out. Will use global UI Fade Time if set to less than zero
        /// </summary>
        public float fadeOutTime;

        [Tooltip("Extra time in seconds to wait after completing the midpoint tasks before fading back in.")]
        /// <summary>
        /// Extra time in seconds to wait after completing the midpoint tasks before fading back in.
        /// </summary>
        public float paddingTime;


        [Tooltip("The callbacks to invoke after fade-out and before fade-in")]
        private UnityEvent _onMidway = new UnityEvent();

        [Tooltip("The callbacks to invoke when fade-in begins")]
        private UnityEvent _onFadeIn = new UnityEvent();

        private List<Func<bool>> _waitConditions = new List<Func<bool>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="waitCond">A condition to wait for before fading back in</param>
        /// <param name="onMidway">An action to execute once fade-out has completely finished</param>
        /// <param name="onFadeIn">An action to execute once fade-in begins</param>
        /// <param name="onFinishCallback">An action to execute once the entire animation has finished</param>
        public void StartAnimation(Func<bool> waitCond, UnityAction onMidway, UnityAction onFadeIn, UnityAction onFinishCallback)
        {
            _waitConditions.Add(waitCond);
            if(onMidway != null)
            {
                _onMidway.AddListener(onMidway);
            }
            if(onFadeIn != null)
            {
                _onFadeIn.AddListener(onFadeIn);
            }
            base.StartAnimation(onFinishCallback);
        }

        protected override IEnumerator Animate()
        {
            this._group.interactable = false;
            fadeOutTime = (fadeOutTime >= 0) ? fadeOutTime : UIManager.UI_CYCLE_TIME;
            fadeInTime = (fadeInTime >= 0) ? fadeInTime : UIManager.UI_CYCLE_TIME;
            paddingTime = (paddingTime >= 0) ? paddingTime : 0;

            float fadeInRemainingTime = fadeInTime * (1f - _group.alpha);

            IEnumerator fade = LerpUtils.FloatEaseLerp(_group.alpha, 1, fadeInRemainingTime, SetAlpha);
            while (fade.MoveNext())
            {
                yield return fade.Current;
            }

            // stay here until we've met all conditions needed before moving on
            while(_waitConditions.Count > 0)
            {   
                this._group.interactable = true;
                // weed out conditions that are satisfied
                List<Func<bool>> stillPendingConditions = new List<Func<bool>>();
                foreach(Func<bool> waitCond in _waitConditions)
                {
                    if(waitCond != null)
                    { 
                        // use this to bypass conditions that raise errors
                        bool isConditionMet = true;
                        try
                        {
                            isConditionMet = waitCond.Invoke();
                        }
                        catch(System.Exception e)
                        {
                            Debug.LogError(e.Message);
                        }
                        if(!isConditionMet)
                        {
                            stillPendingConditions.Add(waitCond);
                        }
                    }
                }
                _waitConditions = stillPendingConditions;

                // invoke whatever we still need to do
                // this is done every cycle in case another call invokes this flash while one is already occuring 
                // we remove all listeners each cycle to make sure we only do each requested action once
                _onMidway.Invoke();
                _onMidway.RemoveAllListeners();
                yield return new WaitForEndOfFrame();
            }

            // extra frame buffer
            yield return new WaitForSeconds(paddingTime);

            _onFadeIn.Invoke();
            _onFadeIn.RemoveAllListeners();
            yield return new WaitForEndOfFrame();

            fade = LerpUtils.FloatEaseLerp(_group.alpha, 0, fadeOutTime, SetAlpha);
            while (fade.MoveNext())
            {
                yield return fade.Current;
            }
            this._group.interactable = false;
        }

        protected override void InitializeImplementation()
        {
            _group = GetComponent<CanvasGroup>();
            _initialAlpha = _group.alpha;
        }

        protected override void ResetImplementation()
        {
            _group.alpha = _initialAlpha;
        }

        public void SetAlpha(float alpha)
        {
            if(_group == null)
            {
                _group = GetComponent<CanvasGroup>();
            }
            _group.alpha = alpha;
        }

        public void ClearAllWaitConditions()
        {
            _waitConditions.Clear();
        }
    }
}