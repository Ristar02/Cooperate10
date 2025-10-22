using UnityEngine;
using UnityEngine.UI;

public class TestPieceAddButton : MonoBehaviour
{
    // 임시로 데이터를 직접 참조해서 조작하는 방식을 사용
    // 이후 파이어베이스와 데이터베이스를 연결하여 데이터 매니저 같은 것이 구성되면
    // 해당 데이터를 가져와서 조작하는 방식으로 구현 가능할 것으로 예상됨
    /*
    [SerializeField] CharacterUpgradeUnit _unit;

    private void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(AddPiece);
    }

    private void AddPiece()
    {
        TempDataManager.Instance.AddPiece();
    }
    */
}
