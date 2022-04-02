using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using ClemCAddons.Utilities;
namespace ClemCAddons
{
    namespace CameraAndNodes
    {
        public class CameraTools
        {
            public delegate void OnCompleted();
            #region SwitchCamera
            public static async void SwitchCamera(Camera originCamera, Camera destinationCamera, float speed, OnCompleted onCompleted)
            {
                GameObject t = originCamera.gameObject;
                GameObject res = destinationCamera.gameObject;
                Vector3 originalPosition = originCamera.transform.position;
                Quaternion originalRotation = originCamera.transform.rotation;
                if (t != null)
                {
                    t.SetActive(false);
                }
                if (res != null)
                {
                    res.SetActive(false);
                }
                await GameTools.MoveTo(originCamera.transform, destinationCamera.transform.position, destinationCamera.transform.rotation, speed);
                originCamera.transform.position = originalPosition;
                originCamera.transform.rotation = originalRotation;
                if (t != null)
                {
                    t.SetActive(true);
                }
                if (res != null)
                {
                    res.SetActive(true);
                }
                destinationCamera.enabled = true;
                originCamera.enabled = false;
                onCompleted.Invoke();
            }
            public static async void SwitchCamera(Camera originCamera, Camera destinationCamera, float speed)
            {
                GameObject t = originCamera.gameObject;
                GameObject res = destinationCamera.gameObject;
                Vector3 originalPosition = originCamera.transform.position;
                Quaternion originalRotation = originCamera.transform.rotation;
                if (t != null)
                {
                    t.SetActive(false);
                }
                if (res != null)
                {
                    res.SetActive(false);
                }
                await GameTools.MoveTo(originCamera.transform, destinationCamera.transform.position, destinationCamera.transform.rotation, speed);
                originCamera.transform.position = originalPosition;
                originCamera.transform.rotation = originalRotation;
                if (t != null)
                {
                    t.SetActive(true);
                }
                if (res != null)
                {
                    res.SetActive(true);
                }
                destinationCamera.enabled = true;
                originCamera.enabled = false;
            }
            public static async Task SwitchCameraAsync(Camera originCamera, Camera destinationCamera, float speed)
            {
                GameObject t = originCamera.gameObject;
                GameObject res = destinationCamera.gameObject;
                Vector3 originalPosition = originCamera.transform.position;
                Quaternion originalRotation = originCamera.transform.rotation;
                if (t != null)
                {
                    t.SetActive(false);
                }
                if (res != null)
                {
                    res.SetActive(false);
                }
                await GameTools.MoveTo(originCamera.transform, destinationCamera.transform.position, destinationCamera.transform.rotation, speed);
                originCamera.transform.position = originalPosition;
                originCamera.transform.rotation = originalRotation;
                if (t != null)
                {
                    t.SetActive(true);
                }
                if (res != null)
                {
                    res.SetActive(true);
                }
                destinationCamera.enabled = true;
                originCamera.enabled = false;
            }
            #endregion SwitchCamera
            #region move camera position only
            public static void MoveCamera(Camera camera, Vector3 destination, float speed)
            {
                _ = GameTools.MoveTo(camera.transform, destination, speed);
            }

            public static void MoveCamera(Camera camera, Vector3[] points, float speed)
            {
                _ = GameTools.MoveTo(camera.transform, points, speed);
            }

            public static void MoveCameraLocal(Camera camera, Vector3 destination, float speed)
            {
                _ = GameTools.MoveTo(camera.transform, camera.transform.parent.position + destination, speed);
            }

            public static void MoveCameraLocal(Camera camera, Vector3[] points, float speed)
            {
                _ = GameTools.MoveTo(camera.transform, points.AddValue(camera.transform.parent.position), speed);
            }

            public static async void MoveCamera(Camera camera, Vector3 destination, float speed, OnCompleted onCompleted)
            {
                await GameTools.MoveTo(camera.transform, destination, speed);
                onCompleted.Invoke();
            }

            public static async void MoveCamera(Camera camera, Vector3[] points, float speed, OnCompleted onCompleted)
            {
                await GameTools.MoveTo(camera.transform, points, speed);
                onCompleted.Invoke();
            }

            public static async void MoveCameraLocal(Camera camera, Vector3 destination, float speed, OnCompleted onCompleted)
            {
                await GameTools.MoveTo(camera.transform, camera.transform.parent.position + destination, speed);
                onCompleted.Invoke();
            }

            public static async void MoveCameraLocal(Camera camera, Vector3[] points, float speed, OnCompleted onCompleted)
            {
                await GameTools.MoveTo(camera.transform, points.AddValue(camera.transform.parent.position), speed);
                onCompleted.Invoke();
            }
            #endregion move camera position only
            #region move camera position & rotation
            public static void MoveCamera(Camera camera, Vector3 destination, Quaternion destinationQuaternion, float speed)
            {
                _ = GameTools.MoveTo(camera.transform, destination, destinationQuaternion, speed);
            }

            public static void MoveCamera(Camera camera, Vector3[] points, Quaternion[] pointsQuaternion, float speed)
            {
                _ = GameTools.MoveTo(camera.transform, points, pointsQuaternion, speed);
            }

            public static void MoveCameraLocal(Camera camera, Vector3 destination, Quaternion destinationQuaternion, float speed)
            {
                _ = GameTools.MoveTo(camera.transform, camera.transform.parent.position + destination, destinationQuaternion, speed);
            }

            public static void MoveCameraLocal(Camera camera, Vector3[] points, Quaternion[] pointsQuaternion, float speed)
            {
                _ = GameTools.MoveTo(camera.transform, points.AddValue(camera.transform.parent.position), pointsQuaternion, speed);
            }

            public static async void MoveCamera(Camera camera, Vector3 destination, Quaternion destinationQuaternion, float speed, OnCompleted onCompleted)
            {
                await GameTools.MoveTo(camera.transform, destination, destinationQuaternion, speed);
                onCompleted.Invoke();
            }

            public static async void MoveCamera(Camera camera, Vector3[] points, Quaternion[] pointsQuaternion, float speed, OnCompleted onCompleted)
            {
                await GameTools.MoveTo(camera.transform, points, pointsQuaternion, speed);
                onCompleted.Invoke();
            }

            public static async void MoveCameraLocal(Camera camera, Vector3 destination, Quaternion destinationQuaternion, float speed, OnCompleted onCompleted)
            {
                await GameTools.MoveTo(camera.transform, camera.transform.parent.position + destination, destinationQuaternion, speed);
                onCompleted.Invoke();
            }

            public static async void MoveCameraLocal(Camera camera, Vector3[] points, Quaternion[] pointsQuaternion, float speed, OnCompleted onCompleted)
            {
                await GameTools.MoveTo(camera.transform, points.AddValue(camera.transform.parent.position), pointsQuaternion, speed);
                onCompleted.Invoke();
            }
            #endregion move camera position & rotation
        }
    }
}