using UnityEngine;

namespace ArtAndTechnology2026.Input
{
    ///<summary>
    ///Webカメラ映像をWebCamTextureで取得する入力コンポーネント
    ///1920*1080, 60fps, RGBA32
    ///</summary>
    public class CameraInput : MonoBehaviour
    {
        [Header("Resolution & FPS")]
        [SerializeField] private int requestedWidth = 1920;
        [SerializeField] private int requestedHeight = 1080;
        [SerializeField] private int requestedFPS = 60;

        [Header("Device Selection")]
        [SerializeField] private string preferredDeviceName = "";
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


            string chosenName = null;
            if (!string.IsNullOrEmpty(preferredDeviceName))
            {
                foreach (var d in devices)
                {
                    if (d.name.Contains(preferredDeviceName))
                    {
                        chosenName = d.name;
                        break;
                    }
                }
                if(chosenName == null)
                {
                    Debug.LogWarning($"[CameraInput] pregerredDeviceName '{preferredDeviceName}' not found. Falling back to deviceIndex.");
                }
            }
            chosenName ??= devices[Mathf.Clamp(deviceIndex, 0, devices.Length - 1)].name;
            _webCam = new WebCamTexture(chosenName, requestedWidth, requestedHeight, requestedFPS);
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
            if(_renderTexture != null)
            {
                _renderTexture.Release();
                _renderTexture = null;
            }
        }

        ///<summary>
        ///現在のカメラテクスチャ。他コンポーネントにはこれを参照
        ///起動前はnull
        ///</summary>
        public WebCamTexture Texture => _webCam;

        ///<summary>カメラがフレームを更新しているか</summary>
        public bool IsReady => _webCam != null && _webCam.isPlaying && _webCam.width > 16 && _webCam.height > 16;

        [Header("Output")]
        [SerializeField] private bool provideRenderTexture = true;
        [SerializeField] private RenderTextureFormat renderTextureFormat = RenderTextureFormat.ARGB32;

        private RenderTexture _renderTexture;
        private bool _loggedAcrualResolution;
        private bool _wasReady;

        ///<summary>GPU上で使いたい場合用。WebCamTextureをBlitしたRenderTexture</summary>
        public RenderTexture RenderTexture => _renderTexture;
        /// <summary>カメラが使用可能状態に入ったときに呼ばれる。起動時だけでなく再接続時も</summary>
        public event System.Action CameraReady;
        /// <summary>カメラが使用不可になった時に呼ばれる</summary>
        public event System.Action CameraLost;

        private void Update()
        {
            if (!IsReady) return;
            if (!provideRenderTexture) return;
            if(_renderTexture == null ||
               _renderTexture.width != _webCam.width ||
               _renderTexture.height != _webCam.height)
            {
                if (_renderTexture != null) _renderTexture.Release();
                _renderTexture = new RenderTexture(_webCam.width, _webCam.height, 0, renderTextureFormat);
                _renderTexture.Create();
            }

            //WebCamTexture → RenderTexture. GPU 内で完結
            Graphics.Blit(_webCam, _renderTexture);

            if (!_loggedAcrualResolution)
            {
                Debug.Log($"[CameraInput] Actual camera resolution: {_webCam.width}x{_webCam.height} @ {_webCam.requestedFPS}fps, requested: {requestedWidth}x{requestedHeight} @ {requestedFPS}fps");
                _loggedAcrualResolution = true;
            }

            bool nowReady = IsReady;
            if (nowReady && !_wasReady) CameraReady?.Invoke();
            else if (!nowReady && _wasReady) CameraLost?.Invoke();
            _wasReady = nowReady;
        }

    }
}
