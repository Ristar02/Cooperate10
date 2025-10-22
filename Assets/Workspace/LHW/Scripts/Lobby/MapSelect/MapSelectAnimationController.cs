using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public class MapSelectAnimationController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Reference")]
    [SerializeField] private StageUIController _UIController;
    [SerializeField] private GameObject _mapSelectPanel;

    [Header("RectTransform")]
    [SerializeField] private Scrollbar _scrollBar;

    [Header("MapDescription PopUp")]
    [SerializeField] private GameObject[] _popUps;

    [Header("MapDescription UI")]
    [SerializeField] private TMP_Text _mapNameText;
    [SerializeField] private TMP_Text _mapDescriptionText;

    [Header("Map Image")]
    [SerializeField] private GameObject[] _mapImage;

    [Header("Button")]
    [SerializeField] private Button _selectButton;
    [SerializeField] private Button _backButton;

    [Header("Offset")]
    [SerializeField] private float _mapImgShrinkDuration = 3f;
    [SerializeField] private float _mapImgShrinkSize = 1.5f;
    [SerializeField] private float _mapInfoActivateDuration = 0.5f;

    // Init
    const int SIZE = 7;
    private float[] _pos = new float[SIZE];
    private float _distance;

    // 드래그 중 지정 변수
    private float _currentPos;
    private float _targetPos;
    private float _scrollSpeed;

    private int _targetIndex;

    // 드래그 중 여부
    private bool _isDrag;

    private Coroutine _dragCoroutine;

    public Action OnTargetPosSelected;

    private List<MapData> _mapData;

    private void Start()
    {
        _mapData = Manager.Data.MapDB.ReturnAllMapData();

        _distance = 1f / (SIZE - 1);
        for (int i = 0; i < SIZE; i++)
        {
            _pos[i] = _distance * i;

            _mapImage[i].GetComponent<Image>().sprite = _mapData[i].MapImage;
        }
        _selectButton.onClick.AddListener(SelectMap);
        _backButton.onClick.AddListener(Back);

        InActivateMapInfo();

        _mapSelectPanel.SetActive(false);
    }

    private void Update()
    {
        if (!_isDrag)
        {
            _scrollBar.value = Mathf.Lerp(_scrollBar.value, _targetPos, Time.deltaTime * 5f);
            if (Mathf.Abs(_scrollBar.value - _targetPos) < 0.01)
            {
                _scrollBar.value = _targetPos;
                MapInfoUIUpdate(_targetIndex);
                OnTargetPosSelected?.Invoke();
            }
        }

        DragScaleAnimation();
    }

    private void OnEnable()
    {
        OnTargetPosSelected += ActivateMapInfo;
    }

    private void OnDisable()
    {
        OnTargetPosSelected -= ActivateMapInfo;
    }

    #region Drag Event

    public void OnBeginDrag(PointerEventData eventData) => InActivateMapInfo();


    public void OnDrag(PointerEventData eventData) => _isDrag = true;

    /// <summary>
    /// 드래그 종료 시점의 속도를 저장하여,
    /// 해당 속도가 일정 수준 이하로 떨어졌을 때 드래그를 멈추고 맵을 지정함.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        _scrollSpeed = eventData.delta.x;
        _dragCoroutine = StartCoroutine(DragCoroutine());
    }

    /// <summary>
    /// 현재 스크롤바의 위치를 기준으로 가장 가까운 위치를 반환
    /// </summary>
    /// <returns></returns>
    private float SetPos()
    {
        for (int i = 0; i < SIZE; i++)
        {
            if (_scrollBar.value < _pos[i] + _distance * 0.5f && _scrollBar.value > _pos[i] - _distance * 0.5f)
            {
                _targetIndex = i;
                return _pos[i];
            }
        }
        return 0;
    }

    private void DragScaleAnimation()
    {
        _currentPos = SetPos();
        for(int i = 0; i < SIZE; i++)
        {
            _mapImage[i].transform.DOScale(1f - _mapImgShrinkSize * (Mathf.Abs(_pos[i] - _currentPos)), Time.deltaTime * _mapImgShrinkDuration);
        }
    }

    /// <summary>
    /// 드래그가 종료되었을 때, 해당 드래그의 속도에 따라
    /// 타겟의 위치를 지정
    /// </summary>
    /// <returns></returns>
    private IEnumerator DragCoroutine()
    {
        while (_scrollSpeed > 20)
        {
            _scrollSpeed -= Time.deltaTime * 250;
            yield return null;
        }

        _targetPos = SetPos();
        _isDrag = false;
        _dragCoroutine = null;
    }

    #endregion

    #region MapInfo PopUp

    private void ActivateMapInfo()
    {
        for(int i = 0; i < _popUps.Length; i++)
        {
            _popUps[i].transform.DOScale(1f, _mapInfoActivateDuration);
            _popUps[i].gameObject.SetActive(true);
        }
    }
    
    private void InActivateMapInfo()
    {
        for(int i = 0; i < _popUps.Length; i++)
        {
            _popUps[i].transform.DOScale(0f, _mapInfoActivateDuration);
            _popUps[i].gameObject.SetActive(false);
        }
    }

    #endregion

    #region MapInfo UIUpdate

    private void MapInfoUIUpdate(int index)
    {
        _mapNameText.text = _mapData[index].MapName;
        _mapDescriptionText.text = _mapData[index].MapDescription;
    }

    #endregion

    #region Button Click

    private void SelectMap()
    {
        _UIController.MapUIUpdate(_targetIndex);
        _mapSelectPanel.SetActive(false);
    }

    private void Back()
    {
        _mapSelectPanel.SetActive(false);
    }

    #endregion
}