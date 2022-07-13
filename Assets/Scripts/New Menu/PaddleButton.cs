using UnityEngine;
using UnityEngine.EventSystems;

#region Enums
public enum PaddleDirection
{
    LEFT,
    RIGHT
}
#endregion

public class PaddleButton : MonoBehaviour, IPointerClickHandler
{
    #region OnPointerClick
    public void OnPointerClick(PointerEventData eventData)
    {
        int index = TabGroup.instance.tabButtons.IndexOf(TabGroup.instance.selectedTab);
        if (gameObject.name.Equals("Left"))
            Paddle(index, PaddleDirection.LEFT);
        else if (gameObject.name.Equals("Right"))
            Paddle(index, PaddleDirection.RIGHT);
    }
    #endregion

    #region Paddle
    public static void Paddle(int index, PaddleDirection direction)
    {
        if (direction == PaddleDirection.LEFT)
        {
            while (index > 0 && !TabGroup.instance.tabButtons[index - 1].isActive)
                index--;
            if (index > 0 && TabGroup.instance.tabButtons[index - 1].isActive)
                TabGroup.instance.OnTabSelected(TabGroup.instance.tabButtons[index - 1]);
        }
        else if (direction == PaddleDirection.RIGHT)
        {
            while (index < TabGroup.instance.tabButtons.Count - 1 && !TabGroup.instance.tabButtons[index + 1].isActive)
                index++;
            if (index < TabGroup.instance.tabButtons.Count - 1 && TabGroup.instance.tabButtons[index + 1].isActive)
                TabGroup.instance.OnTabSelected(TabGroup.instance.tabButtons[index + 1]);
        }
    }
    #endregion
}
