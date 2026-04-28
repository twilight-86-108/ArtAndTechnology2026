using UnityEngine;

namespace ArtAndTechnology2026.Core.Debugging
{   
    /// <summary>
    /// 
    /// </summary>
    public class CameraInputViewer : MonoBehaviour
    {
        [SerializeField] private ArtAndTechnology2026.Input.CameraInput source;
        [SerializeField] private MeshRenderer target;

        // Update is called once per frame
        private void Update()
        {
            if (!source.IsReady || target == null) return;
            if (target.material.mainTexture != source.Texture)
            {
                target.material.mainTexture = source.Texture;
            }
        }
    }

}

