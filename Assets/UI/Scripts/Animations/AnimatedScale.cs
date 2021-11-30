using System.Collections;
using UnityEngine;

namespace UI.Animation
{
    public class AnimatedScale : AnimatedElement {

        [SerializeField]
        private AnimationCurve _curve = null;
        private float _curveDuration;

        private Vector3 _defaultScale;
        public Vector3 InitialScale 
        {
            get { return _defaultScale; }
        }
        private bool _defaultScaleSet = false;

        public Vector3 newScale = Vector3.one * 0.95f;
        public float scaleTime = 0.1f;

        private float _normalizedTime = 0.0f;
        public float NormalizedTime
        {
            get { return _normalizedTime; }
        }

        public void SetCurve(AnimationCurve newCurve)
        {
            _normalizedTime = 0.0f;
            _curve = newCurve;
            _curveDuration = _curve.keys[_curve.length - 1].time;
        }

        protected override void ResetImplementation()
        {
            if (_curve != null && _curve.length > 1)
            {
                _curveDuration = _curve.keys[_curve.length - 1].time;
            }
            else
            {
                SetCurve(LerpUtils.EASE_CURVE);
            }
            transform.localScale = _defaultScale;
        }

        protected override IEnumerator Animate()
        {
            _normalizedTime = 0.0f;
            float currentTime = 0;
            scaleTime = (scaleTime < 0) ? 0.35f : scaleTime;
            Vector3 startScale = animateLoop ? _defaultScale : transform.localScale;
            while (currentTime <= (scaleTime / _curveDuration))
            {
                currentTime += Time.deltaTime;
                _normalizedTime = GetNormalizedTime(currentTime, scaleTime);
                float eval = _curve.Evaluate(_normalizedTime);
                transform.localScale = Vector3.Lerp(startScale, newScale, eval);
      
                yield return null;
            }
        }

        protected override void InitializeImplementation()
        {
            if (!(animateLoop && _defaultScaleSet)) {
                _defaultScale = transform.localScale;
                _defaultScaleSet = true;
            }
        }
    }
}
