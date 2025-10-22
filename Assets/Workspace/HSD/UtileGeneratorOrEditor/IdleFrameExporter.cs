using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using Cysharp.Threading.Tasks;

public class IdleFrameExporter : MonoBehaviour
{
    public Camera captureCamera;        // 투명 배경용 카메라
    public Vector2 imageSize = new Vector2(256, 256);
    public string savePath = "Assets/ExportedIdle/";
    public GameObject[] prefabs;           // 캡쳐할 프리팹

    [ContextMenu("Create Image")]
    public void StartExport()
    {
        CaptureIdleFrame();
    }

    private void CaptureIdleFrame()
    {
        foreach (var prefab in prefabs)
        {
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            Vector3 pos = captureCamera.transform.position;
            pos.z = 0;
            pos.y -= Mathf.Abs(prefab.transform.localScale.x) / 2;

            GameObject instance = Instantiate(prefab, pos, Quaternion.identity);

            Animator animator = instance.GetComponentInChildren<Animator>();    

            captureCamera.clearFlags = CameraClearFlags.SolidColor;
            captureCamera.backgroundColor = new Color(0, 0, 0, 0);

            RenderTexture rt = new RenderTexture((int)imageSize.x, (int)imageSize.y, 24, RenderTextureFormat.ARGB32);
            captureCamera.targetTexture = rt;

            RenderTexture.active = rt;
            captureCamera.Render();

            Texture2D tex = new Texture2D((int)imageSize.x, (int)imageSize.y, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            string filePath = Path.Combine(savePath, prefab.name + "_Icon.png");
            File.WriteAllBytes(filePath, tex.EncodeToPNG());

            Debug.Log($"Idle Frame saved: {filePath}");

            RenderTexture.active = null;
            captureCamera.targetTexture = null;
            DestroyImmediate(rt);
            DestroyImmediate(tex);
            DestroyImmediate(instance);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
    }
}
