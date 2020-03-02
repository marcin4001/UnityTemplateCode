using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Xml;

public class Terminal : MonoBehaviour
{
    public Camera mainCamera;
    public Camera gunCamera;
    public Camera terminalCamera;
    public Canvas hUD;
    public TMP_InputField inputField;
    public TextMeshProUGUI textLogo;
    public TextMeshProUGUI textMain;
    public Settings_options settings;
    private ChangeWeapon weapon;
    private FlashLight flashLight;
    private PlayerMove move;
    private MissionDocument mDoc;
    private EquipmentSystem eq;
    private InteractionUni interaction;
    private XmlDocument terminalXML;
    private bool active = false;
    private string textTerminal;
    private int indexChar = 0;
    private AudioSource source;
    private int lang;

    // Start is called before the first frame update
    void Start()
    {
        interaction = FindObjectOfType<InteractionUni>();
        terminalCamera.enabled = false;
        weapon = FindObjectOfType<ChangeWeapon>();
        flashLight = FindObjectOfType<FlashLight>();
        move = FindObjectOfType<PlayerMove>();
        mDoc = FindObjectOfType<MissionDocument>();
        eq = FindObjectOfType<EquipmentSystem>();
        TextAsset asset = (TextAsset)Resources.Load(@"TerminalSCP\SCPTerminal");
        terminalXML = new XmlDocument();
        terminalXML.LoadXml(asset.text);
        textLogo.text = terminalXML.GetElementsByTagName("title")[0].InnerText;
        settings = FindObjectOfType<Settings_options>();
        source = GetComponent<AudioSource>();
        LoadText("back", 0);
    }

    public void GetNameInter()
    {
        interaction.SetTextInteract("Use Terminal");
    }

    public void Interact()
    {
        if (settings == null) settings = FindObjectOfType<Settings_options>();
        mainCamera.enabled = false;
        terminalCamera.enabled = true;
        ActiveComp(false);
        inputField.ActivateInputField();
        active = true;
        inputField.interactable = true;
        lang = settings.pLStoryLine;
        LoadText("back", lang);
    }

    public void ActiveComp(bool value)
    {
        gunCamera.enabled = value;
        weapon.ActiveWeapon(value);
        flashLight.active = value;
        move.active = value;
        move.rotActive = value;
        mDoc.missionDocActive = value;
        eq.active = value;
        hUD.enabled = value;
    }
    // Update is called once per frame
    void Update()
    {

        if(active)
        {
            if(!inputField.isFocused)inputField.ActivateInputField();
            if(indexChar < textTerminal.Length)
            {
                textMain.text += textTerminal[indexChar];
                indexChar++;
            }
        }
        else
        {
            if (inputField.isFocused) inputField.DeactivateInputField();
        }

        if(Time.timeScale < 0.1f) inputField.DeactivateInputField();

        if (Input.GetKeyDown(KeyCode.End))
        {
            mainCamera.enabled = true;
            terminalCamera.enabled = false;
            ActiveComp(true);
            inputField.DeactivateInputField();
            inputField.interactable = false;
            textMain.text = "";
            indexChar = 0;
            active = false;
        }
    }

    public void inputTerminal(string value)
    {
        inputField.text = "";
        inputField.ActivateInputField();
        string key = value.ToLower().Trim().Replace(" ","");
        bool isLoad = LoadText(key, lang);
        if (!isLoad && Input.GetKeyDown(KeyCode.Return)) source.Play();
    }

    private bool LoadText(string key, int lang)
    {
        if (key == "") return true;
        XmlNodeList temp = terminalXML.GetElementsByTagName(key); 
        if(temp.Count == 0) return false;
        else
        {
            textTerminal = temp[0].ChildNodes[lang].InnerText;
            textMain.text = "";
            indexChar = 0;
            return true;
        }
    }
}
