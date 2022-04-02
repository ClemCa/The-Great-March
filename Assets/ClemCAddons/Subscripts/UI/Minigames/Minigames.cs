using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luminosity.IO;
using System.Threading.Tasks;

namespace ClemCAddons
{
    namespace Minigames
    {
        public class TickTockMinigame // PLACEHOLDER
        {
            public void Disable()
            {

            }
            public void Enable()
            {

            }
            public bool Valid;
        }
        public enum Minigame
        {
            none,
            hold,
            mash,
            QTE,
            timing,
            ExternalSource
        }

        public struct MinigameData
        {
            public object[] Values;
            public T Get<T>(int i)
            {
                return (T)Values[i];
            }
            public string GetString(int i)
            {
                return (string)Values[i];
            }
            public float GetFloat(int i)
            {
                return (float)Values[i];
            }
            public int GetInt(int i)
            {
                return (int)Values[i];
            }
            public bool  GetBool(int i)
            {
                return (bool)Values[i];
            }
        }

        public class Minigames : MonoBehaviour
        {
            [SerializeField] private Image _progressBar;
            [SerializeField] private TMPro.TMP_Text _text;
            [SerializeField] private TickTockMinigame _tickTock;
            [SerializeField] private bool _enabled = false;
            private Minigame _currentMinigame = Minigame.none;
            private MinigameData _currentData = new MinigameData();
            private float _currentValue = 0f;
            public delegate void MiniGameCallback();
            private bool _down = false;
            private float _downTime = 0f;
            private int _step = 0;

            private string _title = "";

            void Start()
            {
                UpdateBar();
            }

            void Update()
            {
                if (_enabled)
                {
                    switch (_currentMinigame)
                    {
                        case Minigame.none:
                            break;
                        case Minigame.hold:
                            Hold();
                            break;
                        case Minigame.mash:
                            Mash();
                            break;
                        case Minigame.QTE:
                            QTE();
                            break;
                        case Minigame.timing:
                            Timing();
                            break;
                        case Minigame.ExternalSource:
                            ExternalSource();
                            break;
                    }
                    UpdateBar();
                }
            }

            public void SetTitle(string title)
            {
                _title = title;
            }

            public void StartMinigame(Minigame type, MinigameData data, float startValue = 0f)
            {
                if (type == Minigame.timing)
                {
                    _tickTock.Enable();
                }
                StartDelay(type, data, startValue);
            }

            private async void StartDelay(Minigame type, MinigameData data, float startValue)
            {
                await Task.Delay(10);
                _currentMinigame = type;
                _currentData = data;
                _currentValue = startValue;
                _step = 0;
                _enabled = true;
            }

            public void StopMinigame(bool keepValue = false)
            {
                _enabled = false;
                _title = "";
                if (!keepValue)
                {
                    _currentValue = 0f;
                }
                UpdateBar();
            }

            public void StopMinigame(float restValue)
            {
                _enabled = false;
                _title = "";
                _currentValue = restValue;
                UpdateBar();
            }

            public MinigameData GenerateHoldData(string input, float riseSpeed, float fallSpeed, MiniGameCallback onSuccess, bool shouldStopOnSuccess = true)
            {
                return new MinigameData()
                {
                    Values = new object[] { input, riseSpeed, fallSpeed, onSuccess, shouldStopOnSuccess }
                };
            }

            private void Hold()
            {
                // expects:
                // 0 = input
                // 1 = up speed
                // 2 = down speed
                // 3 = MinigameCallback function reference to invoke on success
                // 4 = stop on success
                if (InputManager.GetButton(_currentData.GetString(0)) != false || InputManager.GetAxis(_currentData.GetString(0)) != 0)
                {
                    _currentValue = Mathf.Clamp01(_currentValue + (_currentData.GetFloat(1) * Time.deltaTime));
                } else
                {
                    _currentValue = Mathf.Clamp01(_currentValue - (_currentData.GetFloat(2) * Time.deltaTime));
                }
                if(_currentValue == 1)
                {
                    if (_currentData.GetBool(4))
                    {
                        _tickTock.Disable();
                        _enabled = false;
                    }
                    else
                    {
                        _currentValue = 0;
                        _tickTock.Disable();
                    }
                    _title = "";
                    _currentValue = 0;
                    _currentData.Get<MiniGameCallback>(3).Invoke();
                }
            }

            public MinigameData GenerateTimingData(string input, float tickSpeed, MiniGameCallback onSuccess, MiniGameCallback onFailure, bool shouldStopOnSuccess = true)
            {
                return new MinigameData()
                {
                    Values = new object[] { input, tickSpeed, onSuccess, onFailure, shouldStopOnSuccess }
                };
            }

            private void Timing()
            {
                // expects:
                // 0 = input
                // 1 = tick speed
                // 3 = MinigameCallback function reference to invoke on failure
                // 3 = MinigameCallback function reference to invoke on success
                // 4 = stop on success
                if (InputManager.GetButtonDown(_currentData.GetString(0)))
                {
                    _currentValue = 1;
                }
                else
                {
                    _currentValue = 0;
                }
                if (_currentValue == 1)
                {
                    if (_tickTock.Valid)
                    {
                        if (_currentData.GetBool(4))
                        {
                            _enabled = false;
                            _tickTock.Disable();
                        }
                        _title = "";
                        _currentValue = 0;
                        _currentData.Get<MiniGameCallback>(2).Invoke();
                    }
                    else
                    {
                        _enabled = false;
                        _tickTock.Disable();
                        _title = "";
                        _currentValue = 0;
                        _currentData.Get<MiniGameCallback>(3).Invoke();
                    }
                }
            }

            public MinigameData GenerateMashData(string input, float jumpValue, float fallSpeed, MiniGameCallback onSuccess, float downTimeBetweenPresses = 0f, bool shouldStopOnSuccess = true)
            {
                return new MinigameData()
                {
                    Values = new object[] { input, jumpValue, downTimeBetweenPresses, fallSpeed, onSuccess, shouldStopOnSuccess }
                };
            }

            private void Mash()
            {
                // expects:
                // 0 = input
                // 1 = press value
                // 2 = downtime (can be 0)
                // 3 = down speed
                // 4 = MinigameCallback function reference to invoke on success
                // 5 = stop on success
                _currentValue = Mathf.Clamp01(_currentValue - (_currentData.GetFloat(3) * Time.deltaTime));
                if ((InputManager.GetButton(_currentData.GetString(0)) != false || InputManager.GetAxis(_currentData.GetString(0)) != 0) && !_down && _downTime <= 0)
                {
                    _down = true;
                    _downTime += _currentData.GetFloat(2);
                    _currentValue = Mathf.Clamp01(_currentValue + (_currentData.GetFloat(1)));
                } else if (InputManager.GetButton(_currentData.GetString(0)) == false &&  InputManager.GetAxis(_currentData.GetString(0)) == 0)
                {
                    _down = false;
                }
                _downTime = Mathf.Max(0, _downTime - Time.deltaTime);
                if (_currentValue == 1)
                {
                    if (_currentData.GetBool(5))
                    {
                        _enabled = false;
                    } else
                    {
                        _currentValue = 0;
                    }
                    _title = "";
                    _currentValue = 0;
                    _currentData.Get<MiniGameCallback>(4).Invoke();
                }
            }

            public MinigameData GenerateQTEData(string[] inputs, float stepIncrease, float fallSpeed, MiniGameCallback onSuccess, MiniGameCallback onFailure, bool repeatOnSuccess = false, bool repeatOnFailure = false, string[] stepTitles = default)
            {
                if(stepTitles == default)
                {
                    stepTitles = new string[] { "QTE" };
                }
                return new MinigameData()
                {
                    Values = new object[] { inputs, stepIncrease, fallSpeed, onSuccess, onFailure, repeatOnSuccess, repeatOnFailure, stepTitles }
                };
            }

            public int GetQTEStep()
            {
                return _step;
            }

            private void QTE()
            {
                // expects:
                // 0 = string[] inputs
                // 1 = step increase
                // 2 = down speed (can be 0)
                // 3 = MinigameCallback function reference to invoke on success
                // 4 = MinigameCallback function reference to invoke on failure
                // 5 = repeat on success
                // 6 = repeat on failure
                // 7 = steps titles (optional)
                if (_currentData.Get<string[]>(7).Length > _step+1)
                {
                    _title = _currentData.Get<string[]>(7)[_step];
                } else
                {
                    _title = _currentData.Get<string[]>(7)[0];
                }
                if (InputManager.GetButtonDown(_currentData.Get<string[]>(0)[_step]))
                {
                    _currentValue = Mathf.Clamp01(_currentValue + _currentData.GetFloat(1));
                    _step++;
                }
                _currentValue = Mathf.Clamp01(_currentValue - (_currentData.GetFloat(2) * Time.deltaTime));
                if (_currentValue == 1)
                {
                    if (_currentData.GetBool(5))
                    {
                        _step = 0;
                        _currentValue = 0;
                    }
                    else
                    {
                        _enabled = false;
                    }
                    _currentData.Get<MiniGameCallback>(3).Invoke();
                }
                if(_step >= _currentData.Get<string[]>(0).Length)
                {
                    if (_currentData.GetBool(5))
                    {
                        _step = 0;
                        _currentValue = 0;
                    } else
                    {
                        _enabled = false;
                    }
                    _title = "";
                    _currentValue = 0;
                    if(_currentData.Get<MiniGameCallback>(4) != null)
                    {
                        _currentData.Get<MiniGameCallback>(4).Invoke();
                    }
                }
            }

            private void UpdateBar()
            {
                _progressBar.fillAmount = _currentValue;
                _text.text = _title;
            }

            public void ExternalStep()
            {
                _currentValue = Mathf.Clamp01(_currentValue + (_currentData.GetFloat(1)));
                _step++;
            }

            // External step is meant to be called by a complex, external minigame.
            // It allows for a lot of creative freedom and only splits by step, so it can be done with a single step
            public void ExternalStep(int step)
            {
                if (step == _step + 1)
                {
                    _currentValue = Mathf.Clamp01(_currentValue + (_currentData.GetFloat(1)));
                }
                else if (_currentData.Get<bool>(3))
                {
                    if (_currentData.Get<bool>(6))
                    {
                        _step = 0;
                    }
                    _title = "";
                    _currentValue = 0;
                    _currentData.Get<MiniGameCallback>(6).Invoke();
                }
            }

            public MinigameData GenerateExternalData(float stepJump, int numberOfSteps, float fallSpeed, bool canFail, MiniGameCallback onSuccess, bool stopOnSuccess = true, bool resetOnFail = true, MiniGameCallback onFailure = null)
            {
                return new MinigameData()
                {
                    Values = new object[] { stepJump, numberOfSteps, fallSpeed, canFail, onSuccess, stopOnSuccess, resetOnFail, onFailure }
                };
            }

            private void ExternalSource()
            {
                // expects:
                // 0 = press value
                // 1 = steps
                // 2 = down speed (can be 0)
                // 3 = can fail
                // 4 = MinigameCallback function reference to invoke on success
                // 5 = stop on success
                // 6 = reset on fail
                // 7 = MinigameCallback function reference to invoke on failure
                _currentValue = Mathf.Clamp01(_currentValue - (_currentData.GetFloat(2) * Time.deltaTime));
                if (_currentValue == 1)
                {
                    if (_currentData.GetBool(5))
                    {
                        _enabled = false;
                    }
                    else
                    {
                        _currentValue = 0;
                    }
                    _title = "";
                    _currentValue = 0;
                    _currentData.Get<MiniGameCallback>(4).Invoke();
                }
            }
        }
    }
}
