using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class BluetoothAdapter : MonoBehaviour
{
    #region Scan Status
    //devices
    private bool _isScanningDevices = false;
    private BleApi.ScanStatus _deviceScanStatus = BleApi.ScanStatus.AVAILABLE;
    [System.Serializable]
    public class DeviceScanCompleteEvent : UnityEvent<List<BleDevice>>{}
    public DeviceScanCompleteEvent onDeviceScanComplete;

    //services
    private bool _isScanningServices = false;
    private BleApi.ScanStatus _serviceScanStatus = BleApi.ScanStatus.AVAILABLE;
    [System.Serializable]
    public class ServiceScanCompleteEvent : UnityEvent<List<string>>{}
    public ServiceScanCompleteEvent onServiceScanComplete;

    //characteristics
    private bool _isScanningCharacteristics = false;
    private BleApi.ScanStatus _characteristicsScanStatus = BleApi.ScanStatus.AVAILABLE;

    #endregion


    private Dictionary<string, BleDevice> _devices = new Dictionary<string, BleDevice>();
    private List<string> _services = new List<string>();
    Dictionary<string, string> characteristicNames = new Dictionary<string, string>();

    [SerializeField]
    private UnityEngine.UI.Text _debugText = null;


    // Start is called before the first frame update
    private void Start()
    {
        this.onDeviceScanComplete.AddListener((list) => {
            string names = "";
            foreach(BleDevice device in list){
                names += device.ToString() + "\n";
            }
            this._debugText.text = names; 
            StartServiceScan(list[0]);
        });
        this.onServiceScanComplete.AddListener((list) => {
            string names = "";
            foreach(string uuid in list){
                Debug.Log(uuid);
                if(uuid.ToLower().Contains("180d-")){
                    names += uuid + "\n";
                }
            }
            this._debugText.text = names; 
        });
        ToggleDeviceScan();
    }

    private void OnApplicationQuit()
    {
        BleApi.Quit();
    }

    // Update is called once per frame
    private void Update()
    {
        // device scanning
        if(this._isScanningDevices)
        {
            BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
            this._deviceScanStatus = BleApi.PollDevice(ref res, false);
            // Debug.Log(this._status);
            if(this._deviceScanStatus == BleApi.ScanStatus.AVAILABLE){
                // Debug.Log(JsonUtility.ToJson(res));
                if (!this._devices.ContainsKey(res.id)){
                    this._devices[res.id] =  new BleDevice();
                    this._devices[res.id].id = res.id;
                }
                if (res.nameUpdated){
                    this._devices[res.id].name = res.name;
                }
                if (res.isConnectableUpdated){
                    this._devices[res.id].isConnectable = res.isConnectable;
                }
                // consider only devices which have a name and which are connectable
                if (!string.IsNullOrEmpty(this._devices[res.id].name) && this._devices[res.id].isConnectable){
                    // add new device to list
                    // Debug.Log(this._devices[res.id].name);
                }
            }
            else if(this._deviceScanStatus == BleApi.ScanStatus.FINISHED){
                this._isScanningDevices = false;
                // deviceScanButtonText.text = "Scan devices";
                // deviceScanStatusText.text = "finished";
                if(this.onDeviceScanComplete != null){
                    onDeviceScanComplete.Invoke(new List<BleDevice>(this._devices.Values));
                }
            }
        }
        // service scanning
        if(this._isScanningServices)
        {
            BleApi.Service res = new BleApi.Service();
            this._serviceScanStatus = BleApi.PollService(out res, false);
            if(this._serviceScanStatus == BleApi.ScanStatus.AVAILABLE)
            {
                this._services.Add(res.uuid);
            }
            else if (this._serviceScanStatus == BleApi.ScanStatus.FINISHED)
            {
                this._isScanningServices = false;
                if(this.onServiceScanComplete != null){
                    this.onServiceScanComplete.Invoke(this._services);
                    Debug.Log("Finished service scan!");
                }
            }
        }
        // characteristic scanning
        if(this._isScanningCharacteristics){
            BleApi.Characteristic res = new BleApi.Characteristic();
            this._characteristicsScanStatus = BleApi.PollCharacteristic(out res, false);
            if (this._characteristicsScanStatus == BleApi.ScanStatus.AVAILABLE)
            {
                string name = res.userDescription != "no description available" ? res.userDescription : res.uuid;
                characteristicNames[name] = res.uuid;
            }
            else if (this._characteristicsScanStatus == BleApi.ScanStatus.FINISHED)
            {
                this._isScanningCharacteristics = false;
            }
        }
    }


    public void ToggleDeviceScan(){
        if(!this._isScanningDevices){
            // start new scan
            this._isScanningDevices = true;
            BleApi.StartDeviceScan();
            Debug.Log("Starting device scan...");
        }
        else
        {
            // stop scan
            this._isScanningDevices = false;
            BleApi.StopDeviceScan();
            Debug.Log("Stopping device scan...");
        }
    }

    public void StartServiceScan(BleDevice device)
    {
        if(!this._isScanningServices)
        {
            this._services.Clear();
            // start new scan
            this._isScanningServices = true;
            BleApi.ScanServices(device.id);
            Debug.Log("Starting service scan...");
        }
    }

    // public void StartCharacteristicScan(BleDevice, )
    // {
    //     if (!this._isScanningCharacteristics)
    //     {
    //         // start new scan
    //         BleApi.ScanCharacteristics(selectedDeviceId, selectedServiceId);
    //         this._isScanningCharacteristics = true;;
    //     }
    // }

    public class BleDevice{
        public string name;
        public string id;
        public bool isConnectable;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public class BleService{
        public BleDevice device;
        public string id;
        
        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    public class BleCharacteristic{
        public BleDevice device;
        public BleService service;
        public byte value;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
