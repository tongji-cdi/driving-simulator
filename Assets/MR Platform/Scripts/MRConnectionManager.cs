using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.Transports.UNET;

namespace MRPlatform
{
    public class MRConnectionManager : MonoBehaviour
    {
        // Make MRConnectionManager a singleton;
        private static MRConnectionManager _instance;
        public static MRConnectionManager Singleton { get { return _instance; } }

        UNetTransport transport;
        public int rigidBodyId = 1;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }
        }


        public void Start () {
            transport = NetworkManager.Singleton.GetComponent<UNetTransport>();
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback) {
            bool approved = true;  // Default to approval for now. connectionData is just hmdId.
            bool createPlayerObject = true;
            
            int hmdId = System.BitConverter.ToInt32(connectionData, 0);
            if (hmdId < 0) {
                approved = false;
            } else {
                rigidBodyId = hmdId;
            }

            ulong? prefabHash = null; // Use default player prefab, which should be just one.

            callback(createPlayerObject, prefabHash, approved, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        }

        public void StartServer(string address, int port)
        {
            // NOTE: If we are starting a server, don't call MRAssetManager.LoadAssets.
            // See the comment in MRAssetManager.AddAssetToNetworkPrefabs to see why.

            // Set network transport config from parameters.
            gameObject.GetComponent<UNetTransport>().ConnectAddress = address;
            gameObject.GetComponent<UNetTransport>().ConnectPort = port;

            // Really connect.
            NetworkManager.Singleton.StartHost();
        }

        public void StartClient(string address, int port, int hmdId)
        {
            // First we load dynamic assets, because they must be added to network prefabs before connection
            if (MRAssetManager.Singleton) {
                MRAssetManager.Singleton.LoadAssets();
            }

            // Set network transport config from parameters.
            gameObject.GetComponent<UNetTransport>().ConnectAddress = address;
            gameObject.GetComponent<UNetTransport>().ConnectPort = port;
            rigidBodyId = hmdId;

            // Send hmdId as connectionData
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(hmdId);

            // Really connect.
            NetworkManager.Singleton.StartClient();
        }
    }
}

