using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogSystem : MonoBehaviour
{
	[SerializeField]
	private	Speaker[]		speakers;					// 대화를 출력하는 캐릭터들의 UI 배열
	[SerializeField]
	private	DialogData[]	dialogs;					// 현재 읽을 모든 대사 배열
	[SerializeField]
	private	bool			isAutoStart = true;			// 자동 시작 여부
	[SerializeField]
	private	float			typingSpeed = 0.1f;			// 텍스트 타이핑 효과의 재생 속도
	[SerializeField]
	private	bool			enableTouchToContinue = true; // 터치로 계속 진행 가능 여부

	private	bool			isFirst = true;				// 최초 1회만 호출하기 위한 변수
	private	int				currentDialogIndex = -1;	// 현재 대사 순번
	private	int				currentSpeakerIndex = 0;	// 현재 말을 하는 화자(Speaker)의 speakers 배열 순번
	private	bool			isTypingEffect = false;		// 텍스트 타이핑 효과가 재생중인지 여부
	private Touch			currentTouch;				// 현재 터치 정보
	private bool			isTouchProcessed = false;	// 터치 처리 완료 여부

	private void Awake()
	{
		Setup();
	}

	private void Setup()
	{
		// 모든 대화 관련 게임오브젝트 비활성화
		for ( int i = 0; i < speakers.Length; ++ i )
		{
			SetActiveObjects(speakers[i], false);
			// 캐릭터 이미지는 보이도록 설정
			speakers[i].spriteRenderer.gameObject.SetActive(true);
		}
	}

	public bool UpdateDialog()
	{
		// 대사 읽기가 시작된 후 1회만 호출
		if ( isFirst == true )
		{
			// 초기화. 캐릭터 이미지는 활성화하고, 대사 관련 UI는 모두 비활성화
			Setup();

			// 자동 시작(isAutoStart=true)으로 설정되어 있으면 첫 번째 대사 출력
			if ( isAutoStart ) SetNextDialog();

			isFirst = false;
		}

		// 터치 입력 처리
		HandleTouchInput();

		return false;
	}

	private void HandleTouchInput()
	{
		if (!enableTouchToContinue) return;
        
		if (Input.touchCount > 0)
		{
			currentTouch = Input.GetTouch(0);

		
			if (currentTouch.phase == TouchPhase.Began && !isTouchProcessed)
			{
				ProcessTouchInput();
				isTouchProcessed = true;
			}
		}
		else
		{
			// 터치가 끝나면 다시 처리 가능하도록 설정
			isTouchProcessed = false;
		}
        
		#if UNITY_EDITOR
		if (Input.GetMouseButtonDown(0) && !isTouchProcessed)
		{
			ProcessTouchInput();
		}
		#endif
	}

	private void ProcessTouchInput()
	{
		// 텍스트 타이핑 효과가 재생중일때 터치하면 타이핑 효과 중단
		if ( isTypingEffect == true )
		{
			SkipTypingEffect();
			return;
		}

		// 대사가 더 있을 경우 다음 대사 출력
		if ( dialogs.Length > currentDialogIndex + 1 )
		{
			SetNextDialog();
		}
		// 대사가 더 이상 없을 경우 모든 오브젝트를 비활성화하고 true 반환
		else
		{
			EndDialog();
		}
	}

	private void SkipTypingEffect()
	{
		isTypingEffect = false;
		
		// 타이핑 효과를 중단하고, 현재 대사 전체를 출력한다
		StopCoroutine("OnTypingText");
		speakers[currentSpeakerIndex].textDialogue.text = dialogs[currentDialogIndex].dialogue;
		// 대사가 완료되었을 때 나타나는 커서 활성화
		speakers[currentSpeakerIndex].objectArrow.SetActive(true);
	}

	private void EndDialog()
	{
		// 현재 대화에 참여했던 모든 캐릭터, 대화 관련 UI를 화면에 안 보이게 비활성화
		for ( int i = 0; i < speakers.Length; ++ i )
		{
			SetActiveObjects(speakers[i], false);
			// SetActiveObjects()에서 캐릭터 이미지를 건드리지 않는 부분이 있어서 별도로 비활성화 호출
			speakers[i].spriteRenderer.gameObject.SetActive(false);
		}
	}

	private void SetNextDialog()
	{
		// 현재 화자의 대화 관련 오브젝트 비활성화
		SetActiveObjects(speakers[currentSpeakerIndex], false);

		// 다음 대사를 출력하도록 설정
		currentDialogIndex ++;

		// 현재 화자 순번 설정
		currentSpeakerIndex = dialogs[currentDialogIndex].speakerIndex;

		// 현재 화자의 대화 관련 오브젝트 활성화
		SetActiveObjects(speakers[currentSpeakerIndex], true);
		// 현재 화자 이름 텍스트 설정
		speakers[currentSpeakerIndex].textName.text = dialogs[currentDialogIndex].name;
		// 현재 화자의 대사 텍스트 설정 (타이핑 효과 시작)
		StartCoroutine("OnTypingText");
	}

	private void SetActiveObjects(Speaker speaker, bool visible)
	{
		speaker.imageDialog.gameObject.SetActive(visible);
		speaker.textName.gameObject.SetActive(visible);
		speaker.textDialogue.gameObject.SetActive(visible);

		// 화살표는 대사가 출력되었을 때만 활성화하기 때문에 항상 false
		speaker.objectArrow.SetActive(false);

		// 캐릭터 선택 시 투명도 조절
		Color color = speaker.spriteRenderer.color;
		color.a = visible == true ? 1 : 0.2f;
		speaker.spriteRenderer.color = color;
	}

	private IEnumerator OnTypingText()
	{
		int index = 0;
		
		isTypingEffect = true;
		
		// 텍스트를 한글자씩 타이핑치듯이 재생
		while ( index < dialogs[currentDialogIndex].dialogue.Length )
		{
			speakers[currentSpeakerIndex].textDialogue.text = dialogs[currentDialogIndex].dialogue.Substring(0, index);

			index ++;
		
			yield return new WaitForSeconds(typingSpeed);
		}

		isTypingEffect = false;

		// 대사가 완료되었을 때 나타나는 커서 활성화
		speakers[currentSpeakerIndex].objectArrow.SetActive(true);
	}

	// 공개 메서드 - 외부에서 대화 종료 확인
	public bool IsDialogFinished()
	{
		return currentDialogIndex >= dialogs.Length - 1 && !isTypingEffect;
	}

	// 공개 메서드 - 외부에서 타이핑 속도 조절
	public void SetTypingSpeed(float speed)
	{
		typingSpeed = Mathf.Max(0.01f, speed); // 최소값 제한
	}

	// 공개 메서드 - 터치 입력 활성화/비활성화
	public void SetTouchEnabled(bool enabled)
	{
		enableTouchToContinue = enabled;
	}
}

[System.Serializable]
public struct Speaker
{
	public	SpriteRenderer	spriteRenderer;		// 캐릭터 이미지 (청취/화자 상태 구분)
	public	Image			imageDialog;		// 대화창 Image UI
	public	TextMeshProUGUI	textName;			// 해당 순서에 말하는 캐릭터 이름 출력 Text UI
	public	TextMeshProUGUI	textDialogue;		// 해당 순서 대사 출력 Text UI
	public	GameObject		objectArrow;		// 대사가 완료되었을 때 나타나는 커서 오브젝트
}

[System.Serializable]
public struct DialogData
{
	public	int		speakerIndex;	// 이름과 대사를 출력할 화자 DialogSystem의 speakers 배열 순번
	public	string	name;			// 캐릭터 이름
	[TextArea(3, 5)]
	public	string	dialogue;		// 대사
}