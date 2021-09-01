using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using MLAPI;
using MLAPI.SceneManagement;


namespace MRPlatform { 

    public class MRSceneManager : NetworkBehaviour
    {
    
        public void SwitchScene () {
            if (NetworkManager.Singleton.IsServer) {
                NetworkSceneManager.SwitchScene("SampleScene2");
            }
        }

        void Update () {
            if (Input.GetKeyUp(KeyCode.Space)) {
                Debug.Log("Space pressed.");
                SwitchScene();
            }
        }

    }

}