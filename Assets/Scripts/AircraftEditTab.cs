using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;

public class AircraftEditTab : MonoBehaviour
{
    public Button editBtn;
    public TMP_Dropdown _options;
    [SerializeField]
    private TMP_Dropdown _itemNum;
    [SerializeField]
    private TMP_Text _btnLabel;
    private bool _isAddOption = true;
    public bool isAddOption
    {
        get => _isAddOption;
        set
        {
            if (value == true) _btnLabel.text = "+";
            else _btnLabel.text = "-";
            _isAddOption = value;
        }
    }

    public int optionNum
    {
        get
        {
            return _options.value;
        }
    }

    public int itemNumber
    {
        get
        {
            return _itemNum.value;
        }
    }

    public void setItemNumOptions(int n)
    {
        _itemNum.ClearOptions();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < n; i++)
        {
            TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
            optionData.text = i.ToString();
            options.Add(optionData);
        }
        _itemNum.AddOptions(options);
    }

    private void Start()
    {
        _options.onValueChanged.AddListener((int index) => {
            if (index % 2 != 0)
            {
                _itemNum.gameObject.SetActive(true);
                _btnLabel.text = "-";
            }
            else
            {
                _itemNum.gameObject.SetActive(false);
                _btnLabel.text = "+";
            }
        });
    }
}
