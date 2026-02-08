using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// This button based toggle button will switch sprites on the basis of is on toggle
/// </summary>
public class UIToggleButton : MonoBehaviour
{
    public UnityEvent<bool> OnToggleChanged;

    public bool isOn = false;
    public Button targetButton;
    public Image targetImage;
    public Sprite graphicOn, graphicOff;

    private void Start()
    {
        targetButton.onClick.AddListener(ToggleGraphic);
        SetImage();
    }

    private void ToggleGraphic()
    {
        isOn = !isOn;
        SetImage();
        OnToggleChanged?.Invoke(isOn);
    }

    private void SetImage()
    {
        if (targetImage != null)
        {
            targetImage.sprite = isOn ? graphicOn : graphicOff;
        }
    }
}