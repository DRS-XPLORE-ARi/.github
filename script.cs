using UnityEngine;
using UnityEngine.UI;
using Opc.UaFx;
using Opc.UaFx.Client;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class TestButtons : MonoBehaviour
{
    private readonly WaitForSecondsRealtime updateDelay = new(2);
    private bool updateUI = true;

    Toggle bt_toggle;
    private bool bt_pressed;
    Slider ui_slider;
    Button bt_button;
    
    OpcClient client;

    string targetSlider;
    string targetToggle;

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


        targetSlider = "ns=5;s=Arp.Plc.Eclr/iValue";
        targetToggle = "";

        client = new OpcClient("opc.tcp://172.20.4.132:4840");
        client.Security.UserIdentity = new OpcClientIdentity("admin", "4e7de71f");

        //bt_toggle.onValueChanged.AddListener(delegate {ToggleValueChanged(bt_toggle, targetToggle);});
        ui_slider.onValueChanged.AddListener(delegate {SliderValueChanged(ui_slider, targetSlider);});
        

        StartCoroutine(UpdateUIvalues());
    }

    private IEnumerator UpdateUIvalues()
    {
        while (updateUI) 
        {
            client.Connect();

            OpcValue result = client.ReadNode(targetSlider);
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
        client.WriteNode(target,OpcAttribute.Value,toggle.isOn);

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
