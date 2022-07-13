using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

namespace Michsky.UI.ModernUIPack
{
    public class CustomInputField : MonoBehaviour, IPointerEnterHandler
    {
        [Header("RESOURCES")]
        public GameObject fieldTrigger;
        private TMP_InputField inputText;
        private Animator inputFieldAnimator;

        // [Header("SETTINGS")]
        private bool isEmpty = true;
        private bool isClicked = false;
        private readonly string inAnim = "In";
        private readonly string outAnim = "Out";

        void Start()
        {
            inputFieldAnimator = gameObject.GetComponent<Animator>();
            inputText = gameObject.GetComponent<TMP_InputField>();

            // Check if text is empty or not
            if (inputText.text.Length <= 0)
                isEmpty = true;

            else
                isEmpty = false;

            // Animate if it's empty
            if (isEmpty == true)
                inputFieldAnimator.Play(outAnim);

            else
                inputFieldAnimator.Play(inAnim);
        }

        void Update()
        {
            if (inputText.text.Length >= 1)
            {
                isEmpty = false;
                inputFieldAnimator.Play(inAnim);
            }

            else if (isClicked == false)
            {
                inputFieldAnimator.Play(outAnim);
            }
        }

        public void Animate()
        {
            isClicked = true;
            inputFieldAnimator.Play(inAnim);
            fieldTrigger.SetActive(true);
        }

        public void FieldTrigger()
        {
            if (isEmpty == true)
            {
                inputFieldAnimator.Play(outAnim);
                fieldTrigger.SetActive(false);
                isClicked = false;
            }

            else
            {
                fieldTrigger.SetActive(false);
                isClicked = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Animate();
        }
    }
}