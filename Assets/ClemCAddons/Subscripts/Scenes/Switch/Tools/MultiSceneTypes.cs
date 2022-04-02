using UnityEngine;
using System;
namespace ClemCAddons
{
    namespace MultiScene
    {
        public class MultiSceneTypes
        {
            [Serializable]
            public struct SceneOperation
            {
                public string Name;
                public AsyncOperation Operation;
                public SceneOperation(string name, AsyncOperation operation)
                {
                    Name = name;
                    Operation = operation;
                }
            }
        }
    }
}