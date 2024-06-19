using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TraceBoxButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private GameObject _traceBoxGO;
    [SerializeField]
    private Image _bg;
    [SerializeField]
    private Color _onColor;
    [SerializeField]
    private Color _offColor;
    private bool _isOn = true;
    public bool isOn
    {
        get{return _isOn;}
        set
        {
            _isOn = value;
            if(_isOn)
            {
                _bg.color = _onColor;
            }
            else
            {
                _bg.color = _offColor;
            }
        }
    }



    void Awake()
    {
        if(_isOn)
        {
            isOn = false;
        }
        else
        {
            isOn = true;
        }
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        if(_isOn)
        {
            isOn = false;
            _traceBoxGO.transform.localScale = Vector2.zero;
        }
        else
        {
            isOn = true;
            _traceBoxGO.transform.localScale = Vector2.one;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }
    
}
