using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClemCAddons
{
    namespace Utilities
    {
        public class ProximityCaller : MonoBehaviour
        {
            public enum Type
            {
                Bool,
                Float,
                Int,
                String
            }
            [Header("Activation")]
            [SerializeField] private float[] _triggersDistances;
            [SerializeField] private GameObject _triggerObject;
            [SerializeField] private bool _drawDebug = false;
            [SerializeField] private Color _debugColor = new Color(0, 0, 0, 0.1f);
            [Header("Call(s) under distance")]
            [SerializeField] CallInformation[] _callsInformation;

            public struct CallInformation
            {
                public GameObject _methodContainer;
                public string _typeName;
                public string _methodToCall;
                public Type _parameterType;
                public bool[] _parametersB;
                public float[] _parametersF;
                public int[] _parametersI;
                public string[] _parametersS;
                [NonSerialized] public object[] _parameters;
                [NonSerialized] public bool _triggered;
            }

            void Start()
            {
                for (int i = 0; i < _callsInformation.Length; i++)
                {
                    switch (_callsInformation[i]._parameterType)
                    {
                        case Type.Bool:
                            _callsInformation[i]._parameters = Array.ConvertAll(_callsInformation[i]._parametersB, new Converter<bool, object>(BtoObject));
                            break;
                        case Type.Float:
                            _callsInformation[i]._parameters = Array.ConvertAll(_callsInformation[i]._parametersF, new Converter<float, object>(FtoObject));
                            break;
                        case Type.Int:
                            _callsInformation[i]._parameters = Array.ConvertAll(_callsInformation[i]._parametersI, new Converter<int, object>(ItoObject));
                            break;
                        case Type.String:
                            _callsInformation[i]._parameters = Array.ConvertAll(_callsInformation[i]._parametersS, new Converter<string, object>(StoObject));
                            break;
                    }
                }
            }

            void Update()
            {
                float distance = transform.Distance(_triggerObject);
                for (int i = 0; i < _callsInformation.Length; i++)
                {
                    if(distance < _triggersDistances[i] && _callsInformation[i]._triggered == false)
                    {
                        _callsInformation[i]._triggered = true;
                        _callsInformation[i]._methodContainer.GetComponent(_callsInformation[i]._typeName).GetType().GetMethod(_callsInformation[i]._methodToCall).Invoke(_callsInformation[i]._methodContainer.GetComponent(_callsInformation[i]._typeName), _callsInformation[i]._parameters);
                    } else if (distance > _triggersDistances[i] && _callsInformation[i]._triggered == true)
                    {
                        _callsInformation[i]._triggered = false;
                    }
                }
                    
            }
            [ExecuteInEditMode]
            void OnDrawGizmos()
            {
                for (int i = 0; i < _callsInformation.Length; i++)
                {
                    if (_drawDebug)
                        EditorTools.DrawSphereInEditor(_triggerObject.transform.position, _triggersDistances[i], _debugColor);
                }
            }

            private object BtoObject(bool pf)
            {
                return pf;
            }
            private object FtoObject(float pf)
            {
                return pf;
            }
            private object ItoObject(int pf)
            {
                return pf;
            }
            private object StoObject(string pf)
            {
                return pf;
            }
        }
    }
}