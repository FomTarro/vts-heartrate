using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Animation
{
    public enum AnimationDirection {
        FORWARD,
        BACKWARD,
        ALTERNATE_FORWARD,
        ALTERNATE_BACKWARD
    }
    public abstract class AnimatedElement : MonoBehaviour {

        /// <summary>
        /// Should the element begin its animation as soon as it is enabled?
        /// </summary>
        public bool animateOnAwake = false;
        private bool _animating = false;

        /// <summary>
        /// Should animation loop?
        /// </summary>
        public bool animateLoop = false;


        [SerializeField]
        private AnimationDirection _animateDirection = AnimationDirection.FORWARD;
        /// <summary>
        /// Direction of the animation. Default is AnimationDirection.FORWARD
        /// </summary>
        public AnimationDirection Direction 
        {
            get { return _animateDirection; }
            set {    
                _animateDirection = value;
                _isAnimationForward = _animateDirection == AnimationDirection.FORWARD || _animateDirection == AnimationDirection.ALTERNATE_FORWARD;
            }
        }
        /// <summary>
        /// Is the animation going forward?
        /// </summary>
        private bool _isAnimationForward = true;

        /// <summary>
        /// Is the element currently animating?
        /// </summary>
        /// <value></value>
        public bool isAnimating
        {
            get{ return _animating; }
        }
        private bool _terminateAnimation = false;

        private Coroutine _animationRoutine;

        protected List<UnityAction> _finishCallbackList = new List<UnityAction>();

        void Awake()
        {
            Initialize();
        }

        void OnEnable()
        {
            if (animateOnAwake)
            {
                StartAnimation();
            }
        }

        void OnDisable()
        {
            StopAnimation();
            // coroutines are automatically killed on disable, so we need to invoke the graceful closeure ourselves
            _animating = false;
        }

        /// <summary>
        /// Starts the animation.
        /// Restarts the animation if already animating
        /// </summary>
        /// <param name="onFinishCallback">A callback to execute on completion of the animation.  Will not be called if the animation is terminated by disabling the object</param>
        public void StartAnimation(UnityAction onFinishCallback = null)
        {
            if (onFinishCallback != null)
            {
                _finishCallbackList.Add(onFinishCallback);
                //_onFinishCallbacks.AddListener(onFinishCallback);
            }
            if (!isAnimating && _animationRoutine == null)
            {
                _terminateAnimation = false;
                _animationRoutine = StartCoroutine(StartAnimationRoutine());
            }
            else
            {
                // maybe Reset() here?
                _terminateAnimation = false;
                StopCoroutine(_animationRoutine);
                _animating = false;
                _animationRoutine = StartCoroutine(StartAnimationRoutine());

            }
        }

        /// <summary>
        /// Halts the animation of this element
        /// </summary>
        /// <param name="executeCallbacks">If false, will clear all OnFinish callbacks. Defaults to true.</param>
        public void StopAnimation(bool executeCallbacks = true)
        {
            if(!executeCallbacks)
            {
                _finishCallbackList.Clear();
            }

            if (isAnimating)
            {
                _terminateAnimation = true;
            }
        }

        /// <summary>
        /// Internal coroutine for processing animations
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartAnimationRoutine()
        {
            if (!isAnimating)
            {
                _animating = true;
                IEnumerator anim = Animate();
                do
                {
                    yield return anim.Current;
                } while (!_terminateAnimation && anim.MoveNext());
                _animating = false;
            }

            if (animateLoop && !_terminateAnimation) {
                if (_animateDirection == AnimationDirection.ALTERNATE_BACKWARD || _animateDirection == AnimationDirection.ALTERNATE_FORWARD) {
                    _isAnimationForward = !_isAnimationForward;
                }
                StartAnimation();
            }
            else {
                // feels kinda hacky, but allows chaining easily
                // testing needed for sure
                List<UnityAction> tempList = new List<UnityAction>(_finishCallbackList);
                foreach(UnityAction action in tempList)
                {
                    action.Invoke();
                    _finishCallbackList.Remove(action);
                }
            }
            yield return null;
        }

        /// <summary>
        /// The specific implementation of the animation
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator Animate();

    /// <summary>
    /// Gets the normalized value of the passed in time
    /// If you want an animation component that animates forwards/backwards/alternate, add this in Animate()
    /// </summary>
    /// <param name="currentTime"></param>
    /// <param name="scaleTime"></param>
    /// <returns>Normalized value w/ range [0, 1]</returns>
        protected float GetNormalizedTime(float currentTime, float scaleTime) {
            switch (_animateDirection) {
                case AnimationDirection.FORWARD:
                    return currentTime / scaleTime;
                case AnimationDirection.BACKWARD:
                    return 1f - (currentTime / scaleTime);
                case AnimationDirection.ALTERNATE_FORWARD:
                    if (_isAnimationForward) {
                        return currentTime / scaleTime;
                    }
                    else {
                        return 1f - (currentTime / scaleTime);
                    }
                case AnimationDirection.ALTERNATE_BACKWARD:
                    if (_isAnimationForward) {
                        return currentTime / scaleTime;
                    }
                    else {
                        return 1f - (currentTime / scaleTime);
                    }
                default:
                    return 0.0f;
            }
        }

        /// <summary>
        /// Configure any initial parameters
        /// </summary>
        protected abstract void InitializeImplementation();

        protected bool _hasInitialized = false;
        protected void Initialize()
        {
            if(!_hasInitialized)
            {
                InitializeImplementation();
                ResetImplementation();
                _isAnimationForward = _animateDirection == AnimationDirection.FORWARD || _animateDirection == AnimationDirection.ALTERNATE_FORWARD;
                _hasInitialized = true;
            }
        }

        /// <summary>
        /// Resets the element back to a default state
        /// </summary>
        public void ResetDefaults(){

            Initialize();
            ResetImplementation();
        }

        protected abstract void ResetImplementation();

    }
}