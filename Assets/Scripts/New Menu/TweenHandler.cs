using UnityEngine;

public class TweenHandler : MonoBehaviour
{
    #region OnPageSelection
    public void ScaleOnPageSelection()
    {
        int index = transform.GetSiblingIndex();
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 20.0f * Time.deltaTime).setDelay(index * 0.25f);
    }

    public void ScaleOnPageSelection(float offset)
    {
        int index = transform.GetSiblingIndex();
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 20.0f * Time.deltaTime).setDelay(index * 0.25f + offset);
    }

    public void RotateOnPageSelection(Vector3 from, Vector3 to)
    {
        int index = transform.GetSiblingIndex();
        transform.eulerAngles = from;
        LeanTween.rotate(gameObject, to, 20.0f * Time.deltaTime).setDelay(index * 0.25f);
    }

    public void HideBoardOnPageSelection(GameObject boardObject)
    {
        LeanTween.scale(boardObject, Vector3.zero, 20.0f * Time.deltaTime);
        boardObject.SetActive(false);
    }

    public void UnhideBoardOnPageSelection(GameObject boardObject, Vector3 originalScale)
    {
        boardObject.SetActive(true);
        LeanTween.scale(boardObject, originalScale, 20.0f * Time.deltaTime);
    }
    #endregion
}
