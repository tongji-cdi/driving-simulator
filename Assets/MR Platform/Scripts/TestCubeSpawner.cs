using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MLAPI;
using MRPlatform;

public class TestCubeSpawner : NetworkBehaviour
{
    [SerializeField] public AssetReferenceGameObject dynamicCube;
    [SerializeField] public AssetReferenceGameObject dynamicCube2;
    
    override public void NetworkStart()
    {
        if (NetworkManager.Singleton.IsClient && IsLocalPlayer) {
            // Debug.Log("Connected. Spawning Cube...");
            // MRAssetManager.Singleton.Spawn(dynamicCube);
            // MRAssetManager.Singleton.Spawn(dynamicCube2);
        }
    }

    private void CubeLoaded(AsyncOperationHandle<GameObject> handle) {
        Debug.Log("Cube instantiated. Spawning across all connections.");
        handle.Result.GetComponent<NetworkObject>().Spawn();
    }
}
