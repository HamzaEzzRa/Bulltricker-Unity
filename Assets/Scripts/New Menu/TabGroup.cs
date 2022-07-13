using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    #region Enums
    private enum TabName {
        HOME,
        ONLINE,
        SETTINGS,
        PROFILE
    }
    #endregion

    #region Members
    public static TabGroup instance;
    [HideInInspector] public TabButton selectedTab;
    public List<TabButton> tabButtons;
    public Color idleColor, hoverColor, selectedColor, disabledColor;
    public Color32 textColorIdle, textColorHover, textColorDisabled;
    private TweenHandler tweenHandler;
    private bool isOnWindows, isBoardHidden;
    public static bool isOnFront = true;
    #endregion

    #region Awake
    private void Awake()
    {
        instance = this;
        if (ApplicationUtil.Platform == RuntimePlatform.WindowsPlayer)
        {
            GameObject UILeftPaddle = GameObject.Find("Left"), UIRightPaddle = GameObject.Find("Right");
            UILeftPaddle.GetComponentInChildren<TMPro.TMP_Text>().SetText("Q");
            UIRightPaddle.GetComponentInChildren<TMPro.TMP_Text>().SetText("E");
            isOnWindows = true;
        }
        else if (ApplicationUtil.Platform == RuntimePlatform.Android || ApplicationUtil.Platform == RuntimePlatform.OSXPlayer)
        {
            // "←" "→"
            GameObject UILeftPaddle = GameObject.Find("Left"), UIRightPaddle = GameObject.Find("Right");
            TMPro.TMP_Text leftArrow = UILeftPaddle.GetComponentInChildren<TMPro.TMP_Text>();
            leftArrow.SetText("←"); leftArrow.fontSize = 56; leftArrow.alignment = TMPro.TextAlignmentOptions.Midline;
            TMPro.TMP_Text rightArrow = UIRightPaddle.GetComponentInChildren<TMPro.TMP_Text>();
            rightArrow.SetText("→"); rightArrow.fontSize = 56; rightArrow.alignment = TMPro.TextAlignmentOptions.Midline;
            isOnWindows = false;
        }
    }
    #endregion

    #region OnTabEnter
    public void OnTabEnter(TabButton button)
    {
        if (button.isActive)
        {
            ResetTabs();
            if (selectedTab == null || selectedTab != button)
            {
                button.backgroundImage.color = hoverColor;
                button.buttonText.color = textColorHover;
            }
        }
    }
    #endregion

    #region OnTabExit
    public void OnTabExit()
    {
        ResetTabs();
    }
    #endregion

    #region OnTabSelected
    public void OnTabSelected(TabButton button)
    {
        if (button.isActive)
        {
            if (selectedTab != null)
            {
                if (tabButtons.IndexOf(button) < tabButtons.IndexOf(selectedTab))
                {
                    AudioManager.instance.PlaySound("Slide_UI_1");
                }
                else if (tabButtons.IndexOf(button) > tabButtons.IndexOf(selectedTab))
                {
                    AudioManager.instance.PlaySound("Slide_UI_2");
                }
            }
            if (selectedTab != button)
            {
                selectedTab = button;
                ResetTabs();
                button.backgroundImage.color = selectedColor;
                button.buttonText.color = textColorHover;
                button.page.SetActive(true);
                for (int i = 0; i < button.page.transform.childCount; i++)
                {
                    Transform childTransform = button.page.transform.GetChild(i);
                    childTransform.gameObject.SetActive(true);
                    tweenHandler = button.page.transform.GetChild(i).GetComponent<TweenHandler>();
                    if (tweenHandler != null)
                    {
                        if (!childTransform.name.Contains("Profile"))
                        {
                            tweenHandler.ScaleOnPageSelection();
                            if (isBoardHidden)
                            {
                                tweenHandler.UnhideBoardOnPageSelection(EventHandler.instance.boardObject, EventHandler.instance.boardOriginalScale);
                                isBoardHidden = false;
                            }
                        }
                        else if (!isBoardHidden)
                        {
                            tweenHandler.RotateOnPageSelection(-90.0f * Vector3.up, Vector3.zero);
                            tweenHandler.HideBoardOnPageSelection(EventHandler.instance.boardObject);
                            isBoardHidden = true;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < childTransform.childCount; j++)
                        {
                            tweenHandler = childTransform.GetChild(j).GetComponent<TweenHandler>();
                            if (tweenHandler != null)
                            {
                                tweenHandler.ScaleOnPageSelection(0.9f);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region OnTabDeactivate
    public void OnTabDeactivate(TabButton button)
    {
        button.backgroundImage.color = disabledColor;
        button.buttonText.color = textColorDisabled;
        button.isActive = false;
    }
    #endregion

    #region ResetTabs
    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
            {
                continue;
            }
            if (button != null && button.isActive)
            {
                button.GetComponent<Image>().color = idleColor;
                button.GetComponentInChildren<TMPro.TMP_Text>().color = textColorIdle;
                button.page.SetActive(false);
                for (int i = 0; i < button.page.transform.childCount; i++)
                {
                    button.page.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }
    #endregion

    #region Update
    void Update()
    {
        if (selectedTab != null)
        {
            if (isOnWindows && isOnFront)
            {
                int index = tabButtons.IndexOf(selectedTab);
                if (Input.GetKeyDown(KeyCode.Q))
                    PaddleButton.Paddle(index, PaddleDirection.LEFT);
                if (Input.GetKeyDown(KeyCode.E))
                    PaddleButton.Paddle(index, PaddleDirection.RIGHT);
            }
        }
    }
    #endregion

    #region InitTabs
    public void InitTabs(bool isOnline)
    {
        if (!isOnline || selectedTab == null)
        {
            OnTabSelected(tabButtons[(int)TabName.HOME]);
            if (!isOnline)
                OnTabDeactivate(tabButtons[(int)TabName.ONLINE]);
        }
        EventHandler.instance.boardObject.SetActive(true);
    }
    #endregion
}