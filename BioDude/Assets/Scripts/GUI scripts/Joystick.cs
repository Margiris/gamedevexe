using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Input = UnityEngine.Input;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private Image bgImage;
    private Image joystickImage;
    private Vector2 inputVector2;

    public bool isPressed;

    // Use this for initialization
    void Start()
    {
        bgImage = GetComponent<Image>();
        joystickImage = transform.GetChild(0).GetComponent<Image>();

        isPressed = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnDrag(PointerEventData pEventData)
    {
        isPressed = true;

        Vector2 position;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImage.rectTransform, pEventData.position,
            pEventData.pressEventCamera, out position)) return;

        position.x = (position.x / bgImage.rectTransform.sizeDelta.x);
        position.y = (position.y / bgImage.rectTransform.sizeDelta.y);

        inputVector2 = new Vector2(position.x * 2 - 1, position.y * 2 - 1);

        inputVector2 = (inputVector2.magnitude > 1) ? inputVector2.normalized : inputVector2;

        joystickImage.rectTransform.anchoredPosition = new Vector3(
            inputVector2.x * bgImage.rectTransform.sizeDelta.x / 4,
            inputVector2.y * bgImage.rectTransform.sizeDelta.y / 4);
    }

    public void OnPointerUp(PointerEventData pEventData)
    {
        isPressed = false;

        inputVector2 = Vector2.zero;
        joystickImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData pEventData)
    {
        OnDrag(pEventData);
    }

    public float Horizontal()
    {
        return inputVector2.x != 0 ? inputVector2.x : Input.GetAxisRaw("Horizontal");
    }

    public float Vertical()
    {
        return inputVector2.y != 0 ? inputVector2.y : Input.GetAxisRaw("Vertical");
    }
}