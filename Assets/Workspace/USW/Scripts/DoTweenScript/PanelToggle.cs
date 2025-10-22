using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PanelToggle : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private GameObject panelRoot;         
    [SerializeField] private DOTweenAnimation anim;        

    void Awake()
    {
        button.onClick.AddListener(TogglePanel);
    }

    void TogglePanel()
    {
        bool next = !panelRoot.activeSelf;
        if (next)
        {
            panelRoot.SetActive(true);      
            anim.DORestart();               
        }
        else
        {
           
            anim.DOKill();                  
            panelRoot.SetActive(false);
        }
    }
}