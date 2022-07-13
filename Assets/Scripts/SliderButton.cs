using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class SliderButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public enum SlidingDirection
    {
        EAST_WEST,
        NORTH_SOUTH
    };

    public RectTransform viewTransform;
    public SlidingDirection slidingDirection;
    public Image sliderBorder;

    public Image leftArrow, rightArrow, upArrow, downArrow;
    [Range(0.1f, 50f)] public float slidingSpeed = 15f;
    public float timeBetweenFades = 5f;

    public Color buttonIdleColor = new Color32(255, 255, 255, 255);
    public Color buttonClickColor = new Color32(220, 220, 220, 255);

    public Color sliderIdleColor;
    public Color sliderHoverColor;

    private Image buttonImage;
    private Image activeArrow;

    private Vector3 nextTargetPos;

    private void Start()
    {
        buttonImage = GetComponent<Image>();
        if (slidingDirection == SlidingDirection.EAST_WEST)
        {
            if (leftArrow != null && leftArrow.IsActive())
                activeArrow = leftArrow;
            else if (rightArrow != null && rightArrow.IsActive())
                activeArrow = rightArrow;
        }
        else if (slidingDirection == SlidingDirection.NORTH_SOUTH)
        {
            if (upArrow != null && upArrow.IsActive())
                activeArrow = upArrow;
            else if (downArrow != null && downArrow.IsActive())
                activeArrow = downArrow;
        }
        if (viewTransform != null)
            nextTargetPos = viewTransform.anchoredPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.color = buttonClickColor;
        if (viewTransform != null)
        {
            LeanTween.cancel(viewTransform);
            if (slidingDirection == SlidingDirection.NORTH_SOUTH)
            {
                if (activeArrow == upArrow)
                {
                    nextTargetPos = new Vector3(viewTransform.anchoredPosition.x, nextTargetPos.y + viewTransform.rect.height);
                    viewTransform.LeanMove(nextTargetPos, 10f / slidingSpeed)
                        .setEaseOutQuad();
                    activeArrow.transform.gameObject.SetActive(false);
                    activeArrow = downArrow;
                }
                else if (activeArrow == downArrow)
                {
                    nextTargetPos = new Vector3(viewTransform.anchoredPosition.x, nextTargetPos.y - viewTransform.rect.height);
                    viewTransform.LeanMove(nextTargetPos, 10f / slidingSpeed)
                        .setEaseOutQuad();
                    activeArrow.transform.gameObject.SetActive(false);
                    activeArrow = upArrow;
                }
            }
            else if (slidingDirection == SlidingDirection.EAST_WEST)
            {
                if (activeArrow == leftArrow)
                {
                    nextTargetPos = new Vector3(nextTargetPos.x - viewTransform.rect.width, viewTransform.anchoredPosition.y);
                    viewTransform.LeanMove(nextTargetPos, 10f / slidingSpeed)
                        .setEaseOutQuad();
                    activeArrow.transform.gameObject.SetActive(false);
                    activeArrow = rightArrow;
                }
                else if (activeArrow == rightArrow)
                {
                    nextTargetPos = new Vector3(nextTargetPos.x + viewTransform.rect.width, viewTransform.anchoredPosition.y);
                    viewTransform.LeanMove(nextTargetPos, 10f / slidingSpeed)
                        .setEaseOutQuad();
                    activeArrow.transform.gameObject.SetActive(false);
                    activeArrow = leftArrow;
                }
            }
            activeArrow.transform.gameObject.SetActive(true);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        /*if (sliderBorder != null)
            sliderBorder.color = sliderHoverColor;*/
        if (activeArrow != null)
            activeArrow.color = sliderHoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
            buttonImage.color = buttonIdleColor;
        /*if (sliderBorder != null)
            sliderBorder.color = sliderIdleColor;*/
        if (activeArrow != null)
            activeArrow.color = sliderIdleColor;
    }
}
