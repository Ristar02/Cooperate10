using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class WaveTextManager : MonoBehaviour
{
    [Header("텍스트 설정")]
    public TextMeshProUGUI textDisplay;
    
    [Header("대사 목록")]
    [SerializeField, TextArea(2, 5)] 
    private string[] dialogues = {
    };
    
    [Header("웨이브 효과 설정")]
    [SerializeField] private float waveSpeed = 2f;
    [SerializeField] private float waveHeight = 15f;
    [SerializeField] private float waveLength = 0.5f;
    [SerializeField] private AnimationCurve wavePattern = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("대사 출력 설정")]
    [SerializeField] private float dialogueInterval = 2f;
    
    [Header("웨이브 방향")]
    [SerializeField] private WaveDirection waveDirection = WaveDirection.LeftToRight;
    
    public enum WaveDirection
    {
        LeftToRight,
        RightToLeft,
        CenterOut,
        OutToCenter
    }
    
    private Sequence dialogueSequence;
    private Tween waveTween;
    private Vector3[] originalVertices;
    private int currentDialogueIndex = 0;
    private bool isWaveActive = false;
    
    void Start()
    {
        if (textDisplay == null)
            textDisplay = GetComponent<TextMeshProUGUI>();
            
        DOTween.Init();
        StartDialogueSequence();
    }
    
    public void StartDialogueSequence()
    {
        if (dialogueSequence != null)
            dialogueSequence.Kill();
            
        dialogueSequence = DOTween.Sequence();
        currentDialogueIndex = 0;
        
        dialogueSequence.AppendCallback(() => {
            currentDialogueIndex = 0;
            ShowDialogue(dialogues[0], true); 
        });
        
        dialogueSequence.AppendInterval(dialogueInterval);
        
        for (int i = 1; i < dialogues.Length; i++)
        {
            int index = i;
            
            dialogueSequence.AppendCallback(() => {
                currentDialogueIndex = index;
                ShowDialogue(dialogues[index], false); 
            });
            
            dialogueSequence.AppendInterval(dialogueInterval);
        }
        
        
        dialogueSequence.SetLoops(-1, LoopType.Restart);
    }
    
    private void ShowDialogue(string text, bool startWave = false)
    {
        textDisplay.text = text;
        
        ResetOriginalVertices();
        
        if (startWave && !isWaveActive)
        {
            StartSequentialWaveEffect();
        }
    }
    
    private void ResetOriginalVertices()
    {
        if (isWaveActive)
        {
            textDisplay.ForceMeshUpdate();
            var textInfo = textDisplay.textInfo;
            
            if (textInfo.meshInfo.Length > 0)
            {
                originalVertices = new Vector3[textInfo.meshInfo[0].vertices.Length];
                System.Array.Copy(textInfo.meshInfo[0].vertices, originalVertices, originalVertices.Length);
            }
        }
    }
    
    public void StartSequentialWaveEffect()
    {
        if (isWaveActive) return; 
        
        isWaveActive = true;
        
        waveTween = DOTween.To(() => 0f, x => UpdateSequentialWave(x), 360f, waveSpeed)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }
    
    private void UpdateSequentialWave(float time)
    {
        textDisplay.ForceMeshUpdate();
        var textInfo = textDisplay.textInfo;
        
        if (originalVertices == null || originalVertices.Length != textInfo.meshInfo[0].vertices.Length)
        {
            originalVertices = new Vector3[textInfo.meshInfo[0].vertices.Length];
            System.Array.Copy(textInfo.meshInfo[0].vertices, originalVertices, originalVertices.Length);
        }
        
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            
            if (!charInfo.isVisible) continue;
            
            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
            
            float charOffset = GetCharacterOffset(i, textInfo.characterCount);
            float waveTime = time + charOffset;
            
            float waveValue = Mathf.Sin(waveTime * Mathf.Deg2Rad) * waveHeight;
            waveValue *= wavePattern.Evaluate((waveTime % 360f) / 360f);
            
            for (int j = 0; j < 4; j++)
            {
                int vertIndex = charInfo.vertexIndex + j;
                if (vertIndex < originalVertices.Length)
                {
                    verts[vertIndex] = originalVertices[vertIndex] + new Vector3(0, waveValue, 0);
                }
            }
        }
        
        // 메시 업데이트
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textDisplay.UpdateGeometry(meshInfo.mesh, i);
        }
    }
    
    private float GetCharacterOffset(int charIndex, int totalChars)
    {
        float normalizedIndex = (float)charIndex / Mathf.Max(1, totalChars - 1);
        
        switch (waveDirection)
        {
            case WaveDirection.LeftToRight:
                return normalizedIndex * 360f * waveLength;
                
            case WaveDirection.RightToLeft:
                return (1f - normalizedIndex) * 360f * waveLength;
                
            case WaveDirection.CenterOut:
                float distanceFromCenter = Mathf.Abs(normalizedIndex - 0.5f) * 2f;
                return distanceFromCenter * 360f * waveLength;
                
            case WaveDirection.OutToCenter:
                float distanceToCenter = 1f - Mathf.Abs(normalizedIndex - 0.5f) * 2f;
                return distanceToCenter * 360f * waveLength;
                
            default:
                return normalizedIndex * 360f * waveLength;
        }
    }
    
    public void StartCompleteWaveEffect()
    {
        // 펀치 효과 제거, 웨이브 속도만 증가
        if (waveTween != null)
        {
            waveTween.Kill();
        }
        
        // 더 빠른 웨이브 효과로 변경
        waveTween = DOTween.To(() => 0f, x => UpdateSequentialWave(x), 360f, waveSpeed * 1.5f)
            .SetLoops(-1, LoopType.Incremental)
            .SetEase(Ease.Linear);
    }
    
    public void StopWaveEffect()
    {
        isWaveActive = false;
        
        if (waveTween != null)
        {
            waveTween.Kill();
            waveTween = null;
        }
        
        // 텍스트를 원래 상태로 복원
        if (originalVertices != null)
        {
            textDisplay.ForceMeshUpdate();
            var textInfo = textDisplay.textInfo;
            
            if (textInfo.meshInfo.Length > 0)
            {
                var meshInfo = textInfo.meshInfo[0];
                if (originalVertices.Length == meshInfo.vertices.Length)
                {
                    System.Array.Copy(originalVertices, meshInfo.vertices, originalVertices.Length);
                    meshInfo.mesh.vertices = meshInfo.vertices;
                    textDisplay.UpdateGeometry(meshInfo.mesh, 0);
                }
            }
        }
    }
    
    // 웨이브 방향 변경
    public void ChangeWaveDirection(WaveDirection newDirection)
    {
        waveDirection = newDirection;
    }

    void OnDestroy()
    {
        if (dialogueSequence != null)
            dialogueSequence.Kill();
        StopWaveEffect();
    }
}