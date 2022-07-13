using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    #region Members
    public Color idleColor, hoverColor, selectedColor;
    public Image image;
    public UI UIElement;
    #endregion

    #region Start
    private void Start()
    {
        UIElement = GameObject.Find("UI").GetComponent<UI>();
        image = GetComponent<Image>();
    }
    #endregion

    #region PointerEventHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        UIElement.OnButtonSelected(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UIElement.OnButtonEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UIElement.OnButtonExit();
    }
    #endregion
}
