using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClemCAddons
{
    public class Transition
    {
        public delegate void TransitionCallback();

        private static TransitionUI TransitionRef;
        
        public static void StartTransition(GameObject targetUI, Camera optionalCamera = null)
        {
            Check();
            TransitionRef.StartTransition(targetUI, null, optionalCamera);
        }
        public static void StartTransition(TransitionCallback halfwayCallback, Camera optionalCamera = null)
        {
            Check();
            TransitionRef.StartTransition(null,new TransitionCallback[] { halfwayCallback }, optionalCamera);
        }
        public static void StartTransition(TransitionCallback[] halfwayCallback, Camera optionalCamera = null)
        {
            Check();
            TransitionRef.StartTransition(null, halfwayCallback, optionalCamera);
        }
        public static void ResetTransition()
        {
            TransitionRef.ResetTransition();
        }
        private static void Check()
        {
            if (TransitionRef != null)
                return;
            TransitionRef = GameObject.FindObjectOfType<TransitionUI>();
        }
    }

    internal class TransitionUI : MonoBehaviour
    {
        private float _pos = 0;
        private GameObject _target;
        private Transition.TransitionCallback[] _halfway;
        private Camera _optionalCamera;


        void Update()
        {
            if (_target != null)
            {
                _pos += (GetComponent<RectTransform>().sizeDelta.y + 1080) * Time.smoothDeltaTime;
                UpdatePos();
            }
        }

        internal void StartTransition(GameObject targetUI = null, Transition.TransitionCallback[] callback = null, Camera optionalCamera = null)
        {
            if (targetUI == null)
                targetUI = gameObject;
            _halfway = callback;
            _target = targetUI;
            _optionalCamera = optionalCamera;
            Utilities.Timer.StartTimer(667, 500, HalfWay, false);
        }

        internal void ResetTransition()
        {
            _target = null;
            _pos = 0;
            UpdatePos();
        }

        internal void HalfWay()
        {
            if (_halfway != null)
            {
                for (int i = 0; i < _halfway.Length; i++)
                    _halfway[i].Invoke();
                _halfway = null;
            }
            else
            {
                _target.SetActive(!_target.activeSelf);
            }
            if (_optionalCamera != null)
            {
                _optionalCamera.enabled = true;
                _optionalCamera.gameObject.SetActive(true);
            }
            Utilities.Timer.StartTimer(667, 500, ResetTransition, false);
        }

        private void UpdatePos()
        {
            GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition.SetY(_pos);
        }
    }
}

