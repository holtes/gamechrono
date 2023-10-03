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
    private string curLangPins;
    private string _currentTable;
    public Color color;

    [SerializeField]
    private Transform tabel;
    [SerializeField]
    private GameObject _headerPinsPref;
    [SerializeField]
    private GameObject tabelRowPref;
    [SerializeField]
    private GameObject _tableNamePref;
    [SerializeField]
    private GameObject _tabelRowPinPref;
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

    private string[,] csvGrid;
    private string _streamingAssetsPath;
    private Dictionary<string, int> _pinsTypesDict = new Dictionary<string, int>()
    {
        {"componentsSupplier", 0},
        {"materialsSupplier", 1},
        {"manufacturingFactory", 2}
    };

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
        curLangPins = "Lang1";
        string contents = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "MapPoints_" + curLangPins + ".csv"));
        csvGrid = CSVReader.SplitCsvGrid(contents);
        foreach (PanelBtn panelBtn in _panelBtns)
        {
            panelBtn.switchToggle(langNode["PanelsActived"][panelBtn._panelKey].AsBool);
        }
        PanelBtn.onToggleSwitched += setPanelActive;
        setLocalizationTabel("StandBy");
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
            else if (panelKey == "Pins") localizePins();
            else setLocalizeRows(panelNode);
        }
    }

    private void localizePins()
    {
        GameObject header = Instantiate(_headerPinsPref, tabel);
        tabelRows.Add(header);
        for (int i = 1; i < csvGrid.GetLength(1) - 1; i++)
        {
            if (!_pinsTypesDict.ContainsKey(csvGrid[1, i])) continue;
            GameObject rowPinInfo = Instantiate(_tabelRowPinPref, tabel);
            TabelRowPin tabelRowPin = rowPinInfo.GetComponent<TabelRowPin>();
            tabelRowPin.id = i;
            tabelRowPin.name.text = csvGrid[0, i];
            tabelRowPin.name.onEndEdit.AddListener(delegate { changeValuePin(tabelRowPin.id, 0, tabelRowPin.name); });
            tabelRowPin.type.value = _pinsTypesDict[csvGrid[1, i]];
            tabelRowPin.type.onValueChanged.AddListener(delegate { changeValuePin(tabelRowPin.id, 1, tabelRowPin.type); });
            tabelRowPin.lon.text = csvGrid[2, i];
            tabelRowPin.lon.onEndEdit.AddListener(delegate { changeValuePin(tabelRowPin.id, 2, tabelRowPin.lon); });
            tabelRowPin.lat.text = csvGrid[3, i];
            tabelRowPin.lat.onEndEdit.AddListener(delegate { changeValuePin(tabelRowPin.id, 3, tabelRowPin.lat); });
            tabelRowPin.description.text = csvGrid[4, i];
            tabelRowPin.description.onEndEdit.AddListener(delegate { changeValuePin(tabelRowPin.id, 4, tabelRowPin.description); });
            tabelRowPin.location.text = csvGrid[5, i];
            tabelRowPin.location.onEndEdit.AddListener(delegate { changeValuePin(tabelRowPin.id, 5, tabelRowPin.location); });
            tabelRows.Add(rowPinInfo);
        }
    }

    private void changeValuePin(int row, int col, TMP_InputField item)
    {
        csvGrid[col, row] = item.text;
        Debug.Log(col + " " + row);
        saveCSV();
    }

    private void changeValuePin(int row, int col, TMP_Dropdown item)
    {
        csvGrid[col, row] = item.options[item.value].text;
        Debug.Log(col + " " + row);
        saveCSV();
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
            tabelName.aircraftEditTab.editBtn.onClick.AddListener(delegate { editTab(tabelName.id, tabelName.aircraftEditTab); });
            tabelName.aircraftEditTab._options.onValueChanged.AddListener((int index) => {
                setOptionsNum(index, tabelName.id, tabelName.aircraftEditTab);
            });
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

    private void editTab(int aircraftInd, AircraftEditTab aircraftEditTab)
    {
        JSONNode lang1Node = baseNode["language_1"]["Airplanes"][aircraftInd];
        JSONNode lang2Node = baseNode["language_2"]["Airplanes"][aircraftInd];
        string sample;
        switch (aircraftEditTab.optionNum)
        {
            case 0:
                sample = "{ \"Titl\": \"\", \"Description\": \"\" }";
                addParam(lang1Node, lang2Node, "SmallDescriptions", sample);
                break;
            case 1:
                deleteParam(lang1Node, lang2Node, "SmallDescriptions", aircraftEditTab.itemNumber);
                break;
            case 2:
                sample = "{ \"Label\": \"\", \"Value\": \"\" }";
                addParam(lang1Node, lang2Node, "FlyParameters", sample);
                break;
            case 3:
                deleteParam(lang1Node, lang2Node, "FlyParameters", aircraftEditTab.itemNumber);
                break;
            case 4:
                sample = "{ \"Label\": \"\", \"Parameters\": [ { \"Label\": \"\", \"Value\": \"\" } ] }";
                addParam(lang1Node, lang2Node, "ArmySections", sample);
                break;
            case 5:
                deleteParam(lang1Node, lang2Node, "ArmySections", aircraftEditTab.itemNumber);
                break;
        }
        saveJson();
        setLocalizationTabel(_currentTable);
    }

    private void deleteParam(JSONNode lang1Node, JSONNode lang2Node, string param, int index)
    {
        JSONArray param1Node = lang1Node["PersonalPage"][param].AsArray;
        JSONArray param2Node = lang2Node["PersonalPage"][param].AsArray;
        param1Node.Remove(index);
        param2Node.Remove(index);
    }

    private void addParam(JSONNode lang1Node, JSONNode lang2Node, string param, string sampleStr)
    {
        lang1Node = lang1Node["PersonalPage"][param];
        lang2Node = lang2Node["PersonalPage"][param];
        JSONNode sampleNode = JSONNode.Parse(sampleStr);
        lang1Node.Add(param, sampleNode);
        lang2Node.Add(param, sampleNode);
    }

    private void setOptionsNum(int index, int aircraftInd, AircraftEditTab aircraftEditTab)
    {
        switch(index)
        {
            case 1:
                aircraftEditTab.setItemNumOptions(langNode["Airplanes"][aircraftInd]["PersonalPage"]["SmallDescriptions"].Count);
                break;
            case 3:
                aircraftEditTab.setItemNumOptions(langNode["Airplanes"][aircraftInd]["PersonalPage"]["FlyParameters"].Count);
                break;
            case 5:
                aircraftEditTab.setItemNumOptions(langNode["Airplanes"][aircraftInd]["PersonalPage"]["ArmySections"].Count);
                break;
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
        saveJson();
    }

    private void saveJson()
    {
        File.WriteAllText(Path.Combine(_streamingAssetsPath, fileName + ".json"), baseNode.ToString());
    }

    private void saveCSV()
    {
        string content = "";
        for (int i = 0; i < csvGrid.GetLength(1) - 1; i++)
        {
            string row = "";
            for (int j = 0; j < 6; j++)
            {
                if (j != 5) row += csvGrid[j, i] + ",";
                else row += csvGrid[j, i];
                Debug.Log(csvGrid[j, i]);
            }
            if (csvGrid.GetLength(1) - 2 != i) row += "\n";
            content += row;
        }
        File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "MapPoints_" + curLangPins + ".csv"), content);
    }

    public void changeLang()
    {
        if (curLang == "language_2")
        {
            topText.text = "Редактировние русской версии";
            topTagText.text = "EN";
            curLang = "language_1";
            curLangPins = "Lang1";
        }
        else
        {
            topText.text = "Редактировние английской версии";
            topTagText.text = "RU";
            curLang = "language_2";
            curLangPins = "Lang2";
        }
        string contents = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "MapPoints_" + curLangPins + ".csv"));
        csvGrid = CSVReader.SplitCsvGrid(contents);
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
