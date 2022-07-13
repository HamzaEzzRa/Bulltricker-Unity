using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    #region Members
    public GameObject page;
    [HideInInspector] public Image backgroundImage;
    [HideInInspector] public TMPro.TMP_Text buttonText;
    public bool isActive;
    #endregion

    #region PointerEventHandler
    public void OnPointerClick(PointerEventData eventData)
    {
        if (TabGroup.isOnFront)
        {
            TabGroup.instance.OnTabSelected(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TabGroup.isOnFront)
        {
            TabGroup.instance.OnTabEnter(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TabGroup.isOnFront)
        {
            TabGroup.instance.OnTabExit();
        }
    }
    #endregion

    #region Start
    void Start()
    {
        backgroundImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<TMPro.TMP_Text>();
        isActive = true;
    }
    #endregion
}