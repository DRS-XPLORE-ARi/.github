using UnityEngine;
using UnityEngine.UI;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Collections.Generic;

public class OPCValues : MonoBehaviour
{
    private readonly WaitForSecondsRealtime updateDelay = new(2);
    private bool updateUI = true;

    Toggle bt_toggle;
    private bool bt_pressed;
    Slider ui_slider;
    Button bt_button;

    OpcClient client;

    List<OpcNodeId> nodeList;
    string targetSlider;
    string targetToggle;
    string deviceName = "axc-f-2152-wtp";

    // Start is called before the first frame update
    void Start()
    {
        bt_toggle = GameObject.Find("Toggle").GetComponent<Toggle>();
        ui_slider = GameObject.Find("slider").GetComponent<Slider>();
        bt_button = GameObject.Find("Button").GetComponent<Button>();

        bt_toggle.GetOrAddComponent<EventTrigger>();
        //EventTrigger trigger = GameObject.Find("Toggle").AddComponent<EventTrigger>();
        EventTrigger trigger = bt_button.GetOrAddComponent<EventTrigger>();
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        trigger.triggers.Add(pointerUp);

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        trigger.triggers.Add(pointerDown);

        OpcNodeId slider = OpcNodeId.Parse(targetSlider);
        
        targetSlider = "ns=5;s=axc-f-2152-wtp/iValue";
        targetToggle = "";

        client = new OpcClient("opc.tcp://172.20.5.240:4840");
        client.Security.UserIdentity = new OpcClientIdentity("admin", "2aa1bc26");

        //bt_toggle.onValueChanged.AddListener(delegate {ToggleValueChanged(bt_toggle, targetToggle);});
        ui_slider.onValueChanged.AddListener(delegate { SliderValueChanged(ui_slider, targetSlider); });

        nodeList = NodeListInit();
        StartCoroutine(UpdateUIvalues());
    }
    private List<OpcNodeId> NodeListInit()
    {
        List<OpcNodeId> nodeList = new()
        {   
            //Brunnen
            OpcNodeId.Parse($"s={deviceName}/I_uiLevelBE1"),
            OpcNodeId.Parse($"s={deviceName}/I_xLimitLevelBE1Max"),
            OpcNodeId.Parse($"s={deviceName}/I_xLimitLevelBE1Min"),

            //Filter
            OpcNodeId.Parse($"s={deviceName}/I_xLimitLevelFL1Max"),

            //Zwischenbehälter
            OpcNodeId.Parse($"s={deviceName}/I_uiLevelBE2"),
            OpcNodeId.Parse($"s={deviceName}/I_xLimitLevelBE2Max"),
            OpcNodeId.Parse($"s={deviceName}/I_xLimitLevelBE2Mid"),
            OpcNodeId.Parse($"s={deviceName}/I_xLimitLevelBE2Min"),

            //Hochbehälter
            OpcNodeId.Parse($"s={deviceName}/I_uiLevelBE3"),
            OpcNodeId.Parse($"s={deviceName}/I_xLimitLevelBE3Min"),
            OpcNodeId.Parse($"s={deviceName}/I_xLimitLevelBE3Max"),
            OpcNodeId.Parse($"s={deviceName}/I_uiFlowBE3a"),

            //Wegstrecke
            OpcNodeId.Parse($"s={deviceName}/I_uiPumpSpeed"),
            OpcNodeId.Parse($"s={deviceName}/I_uiPumpTemp"),
            OpcNodeId.Parse($"s={deviceName}/I_xBallValve1open"),
            OpcNodeId.Parse($"s={deviceName}/I_xBallValve1close"),
            OpcNodeId.Parse($"s={deviceName}/I_xBallValve2open"),
            OpcNodeId.Parse($"s={deviceName}/I_xBallValve2close"),
            OpcNodeId.Parse($"s={deviceName}/I_uiControlValve1"),
            OpcNodeId.Parse($"s={deviceName}/I_uiPressure1"),
            OpcNodeId.Parse($"s={deviceName}/I_uiPressure2"),
            OpcNodeId.Parse($"s={deviceName}/I_uiFlow1"),
        };
        return nodeList;
    }

    private IEnumerator UpdateUIvalues()
    {
        while (updateUI)
        {
            client.Connect();

            var ns = client.Namespaces;
            OpcValue result = client.ReadNode(targetSlider);
            var results = client.ReadNodes(nodeList);
            Debug.Log("Current slider value: " + result.Value);
            ui_slider.value = result.As<float>();

            client.Disconnect();
            yield return updateDelay;
        }
    }

    void ToggleValueChanged(Toggle toggle, string target)
    {
        client.Connect();

        Debug.Log("Toggle new value" + toggle.isOn);
        client.WriteNode(target, OpcAttribute.Value, toggle.isOn);

        client.Disconnect();
    }

    void SliderValueChanged(Slider slider, string target)
    {
        client.Connect();

        Debug.Log("Moved Slider:" + slider.value);
        ui_slider.value = slider.value;
        client.WriteNode(target, OpcAttribute.Value, (int)ui_slider.value);

        client.Disconnect();
    }

    public void OnPointerDown(PointerEventData data)
    {
        bt_pressed = true;
        Debug.Log("Button pressed = " + bt_pressed);
    }

    public void OnPointerUp(PointerEventData data)
    {
        bt_pressed = false;
        Debug.Log("Button released = " + bt_pressed);
    }
}
