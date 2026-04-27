using UnityEngine;

namespace LiminalCreature.Input
{
    ///<summary>
    ///Webカメラ映像をWebCamTextureで取得する入力コンポーネント
    ///1920*1080, 60fps, RGBA32
    ///</summary>
    public class CameraInput : MonoBehaviour
    {
        [SerializeField] private int requestedWidth = 1920;
        [SerializeField] private int requestedHeight = 1080;
        [SerializeField] private int requestedFPS = 60;
        [SerializeField] private int deviceIndex = 0;

        private WebCamTexture _webCam;

        private void OnEnable()
        {
            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.LogError("[CameraInput] No webcom found.");
                return;
            }
            foreach (var d in devices)
            {
                Debug.Log($"[CameraInput] device: {d.name} frontFacing={d.isFrontFacing}");
            }

            var index = Mathf.Clamp(deviceIndex, 0, devices.Length - 1);
            _webCam = new WebCamTexture(devices[index].name, requestedWidth, requestedHeight, requestedFPS);
            _webCam.Play();
        }

        private void OnDisable()
        {
            if (_webCam != null)
            {
                _webCam.Stop();
                Destroy(_webCam);
                _webCam = null;
            }
        }
    }
}
