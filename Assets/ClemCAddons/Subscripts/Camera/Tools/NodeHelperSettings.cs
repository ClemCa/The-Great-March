using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClemCAddons
{
    namespace CameraAndNodes
    {
        public class NodeHelperSettings : MonoBehaviour
        {
            [SerializeField] NodeHelpSettings _settings = new NodeHelpSettings(true, true, true, true, true, false, true, true);
            public NodeHelpSettings Settings
            {
                get
                {
                    return _settings;
                }
            }
        }
    }
}
