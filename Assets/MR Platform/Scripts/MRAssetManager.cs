using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Configuration;

namespace MRPlatform
{
    public class MRAssetManager : NetworkBehaviour
    {

        // Make MRConnectionManager a singleton;
        private static MRAssetManager _instance;
        public static MRAssetManager Singleton { get { return _instance; } }

        [SerializeField] List<AssetReferenceGameObject> dynamicAssets;
        private Dictionary<string, GameObject> loadedAssets;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            } else {
                _instance = this;
            }

            loadedAssets = new Dictionary<string, GameObject>();
        }

        public void LoadAssets()
        {
            Debug.Log("Loading assets...");
            foreach (var asset in dynamicAssets) {
                if (asset.Asset == null) {
                    asset.LoadAssetAsync().Completed += (handle => AddAssetToNetworkPrefabs(handle, asset));
                }
            }
        }

        private void AssetsLoaded(AsyncOperationHandle<IList<GameObject>> handle) {
            Debug.Log("Assets loaded!");
            Debug.Log(handle.Result);
        }

        private void AddAssetToNetworkPrefabs(AsyncOperationHandle<GameObject> handle, AssetReferenceGameObject asset) {
            /*  
                NOTE: Below is a temporaty hack. The implementation of future versions of MLAPI may change,
                therefore this may not be needed in the future. In short, this part of the code helps to 
                register assets loaded using the AddressableAssets system into the NetworkConfig before
                the actual connection is established. This should only be done on the client side, to
                prevent the server from complaining about mismatched NetworkConfig and rejecting the client.

                My hypothesis is that there are two hashes used in MLAPI. One belongs to NetworkPrefabs, and
                is used by NetworkConfig to check if the client matches with host during connection. The
                other belongs to the NetworkObjects, and is used by the NetworkManager to synchronize clients.
                If we run the code below on both the server and the client, the NetworkPrefab hashes will be
                different, since they are created independently. Then during connection, the server will 
                reject the client due to mismatched NetworkConfigs. But if we only do this on the client, the
                client will connect just fine, because no changes are made to the NetworkConfig during
                connection time. After the connection is established, all synchronization is done using 
                NetworkObject hashes, which stays the same for both the client and the serve, and everything
                will work fine.
            */
            if (NetworkManager.Singleton.IsClient) {
                NetworkPrefab networkPrefab = new NetworkPrefab();
                networkPrefab.Prefab = handle.Result;
                NetworkManager.Singleton.NetworkConfig.NetworkPrefabs.Add(networkPrefab);
            }

            loadedAssets.Add(asset.ToString(), handle.Result);
        }

        private void CheckAllJobsFinished() {

        }

        public void Spawn(AssetReferenceGameObject asset, Vector3 position = default(Vector3), Quaternion rotation = default(Quaternion)) {
            if (NetworkManager.Singleton.IsServer) {
                asset.InstantiateAsync(position, rotation).Completed += AssetSpawned;
            } else {
                SpawnServerRpc(asset.ToString(), position, rotation);
            }
        }

        private void AssetSpawned(AsyncOperationHandle<GameObject> handle) {
            // After loading & instantiating an asset from the server, call its NetworkObject.Spawn().
            handle.Result.GetComponent<NetworkObject>().Spawn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SpawnServerRpc(string assetHash, Vector3 position, Quaternion rotation) {
            GameObject assetPrefab = loadedAssets[assetHash];
            GameObject spawnedAsset = Object.Instantiate(assetPrefab);
            spawnedAsset.GetComponent<NetworkObject>().Spawn();
        }

        public void Despawn() {

        }
    }
}