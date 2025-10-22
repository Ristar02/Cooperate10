using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitFXController
{
    private readonly SpriteRenderer[] _renderers;
    private readonly SortingGroup _sortingGroup;
    private Color[] _baseColors;
    private const float HIT_DURATION = .1f;
    private readonly Color hitColor = new Color(1f, 0.447f, 0.447f);

    public UnitFXController(SpriteRenderer[] renderers, SortingGroup sortingGroup)
    {
        _renderers = renderers;
        _sortingGroup = sortingGroup;

        _baseColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
        {
            _baseColors[i] = _renderers[i].color;
        }
    }    

    public void Flash()
    {
        FlashRoutine().Forget();
    }

    public void BattleSetting()
    {
        if(_sortingGroup != null)
            _sortingGroup.sortingLayerName = "BattleUnit";
        else
        {
            foreach (var renderer in _renderers)
            {
                renderer.sortingLayerName = "BattleUnit";
            }
        }    
    }

    public void SortingLayer(int line)
    {
        if (_sortingGroup != null)
            _sortingGroup.sortingOrder = line;
        else
        {
            foreach (var renderer in _renderers)
            {
                renderer.sortingOrder = line;
            }
        }
    }

    private async UniTask FlashRoutine()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].color = hitColor;
        }

        await UniTask.WaitForSeconds(HIT_DURATION);

        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].color = _baseColors[i];
        }
    }
}
