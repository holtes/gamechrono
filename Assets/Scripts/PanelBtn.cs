using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PanelBtn : MonoBehaviour
{
    public string _panelKey;
    [SerializeField]
    private Button _toggleOn;
    [SerializeField]
    private Button _toggleOff;

    public static event Action<string, bool> onToggleSwitched;
    void Start()
    {
        _toggleOn.onClick.AddListener(() => { onToggleSwitched?.Invoke(_panelKey, true); });
        _toggleOff.onClick.AddListener(() => { onToggleSwitched?.Invoke(_panelKey, false); });
    }

    public void switchToggle(bool isActive)
    {
        _toggleOn.gameObject.SetActive(!isActive);
        _toggleOff.gameObject.SetActive(isActive);
    }
}
