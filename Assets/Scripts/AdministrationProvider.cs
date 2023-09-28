using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SimpleJSON;
using TMPro;
using UnityEngine.UI;

public class AdministrationProvider : MonoBehaviour
{
    [SerializeField]
    private string fileName;
    private JSONNode baseNode = null;
    private JSONNode langNode = null;
    private JSONNode panelNode = null;
    private string curLang;
    private string _currentTable;
    public Color color;

    [SerializeField]
    private Transform tabel;
    [SerializeField]
    private GameObject tabelRowPref;
    [SerializeField]
    private GameObject _tableNamePref;
    [SerializeField]
    private TMP_Text topText;
    [SerializeField]
    private TMP_Text topTagText;

    private int n = 0;
    private List<GameObject> tabelRows = new List<GameObject>();
    [SerializeField]
    private Transform[] _tables;

    [SerializeField]
    private PanelBtn[] _panelBtns;

    private string _streamingAssetsPath;

    [SerializeField]
    private bool _isDebug = false;
    void Start()
    {
        if (!_isDebug)
        {
            var dataFolder = new DirectoryInfo(Application.dataPath);
            var projectFolder = dataFolder.Parent.Parent;
            _streamingAssetsPath = projectFolder.FullName;
        }
        else _streamingAssetsPath = Application.streamingAssetsPath;

        Debug.Log(Path.Combine(_streamingAssetsPath, fileName + ".json"));
        string jsonStr = File.ReadAllText(Path.Combine(_streamingAssetsPath, fileName + ".json"));
        baseNode = JSONNode.Parse(jsonStr);
        Debug.Log(baseNode);
        langNode = baseNode["language_1"];
        curLang = "language_1";
        foreach (PanelBtn panelBtn in _panelBtns)
        {
            panelBtn.switchToggle(langNode["PanelsActived"][panelBtn._panelKey].AsBool);
        }
        PanelBtn.onToggleSwitched += setPanelActive;
        
    }

    public void setLocalizationTabel(string key)
    {
        _currentTable = key;
        n = 0;
        foreach (GameObject tabelRow in tabelRows)
        {
            Destroy(tabelRow);
        }
        tabelRows.Clear();
        string[] keysArr = key.Split('/');
        foreach (string panelKey in keysArr)
        {
            panelNode = langNode[panelKey];
            if (panelKey == "Airplanes") localizeAircrafts(panelNode);
            else if (panelKey == "Technologies") localizeTechologies(panelNode);
            else setLocalizeRows(panelNode);
        }
    }

    private void localizeAircrafts(JSONNode node)
    {
        for (int i = 0; i < node.Count; i++)
        {
            GameObject rowName = Instantiate(_tableNamePref, tabel);
            TableName tabelName = rowName.GetComponent<TableName>();
            tabelName.id = i;
            tabelName.toggle.gameObject.SetActive(true);
            tabelName.blockName.text = node[i]["MozaicCard"]["ModelName"];
            tabelName.toggle.isOn = node[i]["enabled"].AsBool;
            tabelName.toggle.onValueChanged.AddListener((bool state) => { changeAircraftEnabled(state, tabelName.id); });
            tabelRows.Add(rowName);
            setLocalizeRows(node[i]);
        }
    }

    private void localizeTechologies(JSONNode node)
    {
        for (int i = 0; i < node.Count; i++)
        {
            GameObject rowName = Instantiate(_tableNamePref, tabel);
            TableName tabelName = rowName.GetComponent<TableName>();
            tabelName.toggle.gameObject.SetActive(false);
            tabelName.blockName.text = node[i]["TechnologyName"];
            tabelRows.Add(rowName);
            setLocalizeRows(node[i]);
        }
    }
    private void setLocalizeRows(JSONNode node)
    {
        for (int i = 0; i < node.Count; i++)
        {
            setLocalizeRows(node[i]);
            if (node[i].Count == 0 && !node[i].IsBoolean)
            {
                Debug.Log(node[i]);
                GameObject row = Instantiate(tabelRowPref, tabel);
                TabelRow tabelRow = row.GetComponent<TabelRow>();
                tabelRow.num.text = n.ToString();
                tabelRow.inputField.text = node[i];
                tabelRow.node = node[i];
                tabelRow.inputField.onEndEdit.AddListener(delegate { changeValue(tabelRow); });
                tabelRows.Add(row);
            }
            n++;
            
        }
    }

    private void changeValue(TabelRow tabelRow)
    {
        tabelRow.node.Value = tabelRow.inputField.text;
        saveJson();
    }

    private void changeAircraftEnabled(bool state, int aircraftInd)
    {
        baseNode["language_1"]["Airplanes"][aircraftInd]["enabled"].AsBool = state;
        baseNode["language_2"]["Airplanes"][aircraftInd]["enabled"].AsBool = state;
        Debug.Log(aircraftInd);
        saveJson();
    }

    private void saveJson()
    {
        File.WriteAllText(Path.Combine(_streamingAssetsPath, fileName + ".json"), baseNode.ToString());
    }

    public void changeLang()
    {
        if (curLang == "language_2")
        {
            topText.text = "Редактировние русской версии";
            topTagText.text = "EN";
            curLang = "language_1";
        }
        else
        {
            topText.text = "Редактировние английской версии";
            topTagText.text = "RU";
            curLang = "language_2";
        }
        langNode = baseNode[curLang];
        setLocalizationTabel(_currentTable);
    }

    public void SetColor(int num)
    {
        foreach (Transform table in _tables)
        {
            table.GetChild(0).GetComponent<RawImage>().color = Color.white;
            table.GetChild(2).GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }
        _tables[num].GetChild(0).GetComponent<RawImage>().color = color;
        _tables[num].GetChild(2).GetComponentInChildren<TextMeshProUGUI>().color = color;
    }

    public void setPanelActive(string key, bool isActive)
    {
        Debug.Log("Ok");
        baseNode["language_1"]["PanelsActived"][key].AsBool = isActive;
        baseNode["language_2"]["PanelsActived"][key].AsBool = isActive;
        saveJson();
    }

    public void exitApp()
    {
        Application.Quit();
    }
}
