using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using MLAPI;

namespace MRPlatform
{
    public class MRConnectionScreen : MonoBehaviour
    {
        public float animationSpeed = 10f;


        public CanvasGroup homeScreen, serverScreen, clientScreen;
        public Image background;

        public Button startServerButton, connectClientButton;
        public InputField serverAddressField, serverPortField, clientAddressField, clientPortField, clientHmdIdField;

        float homeAlpha = 1f;
        float serverAlpha = 0f;
        float clientAlpha = 0f;

        public void Start() {
            ValidateInput("");
        }

        public void Update() {
            if (homeScreen.alpha != homeAlpha) {
                homeScreen.alpha += (homeAlpha - homeScreen.alpha) * Time.deltaTime * animationSpeed;
                if (Mathf.Abs(homeAlpha - homeScreen.alpha) < 0.01f) homeScreen.alpha = homeAlpha;
            }

            if (serverScreen.alpha != serverAlpha) {
                serverScreen.alpha += (serverAlpha - serverScreen.alpha) * Time.deltaTime * animationSpeed;
                if (Mathf.Abs(serverAlpha - serverScreen.alpha) < 0.01f) serverScreen.alpha = serverAlpha;
            }

            if (clientScreen.alpha != clientAlpha) {
                clientScreen.alpha += (clientAlpha - clientScreen.alpha) * Time.deltaTime * animationSpeed;
                if (Mathf.Abs(clientAlpha - clientScreen.alpha) < 0.01f) clientScreen.alpha = clientAlpha;
            }
        }

        public void TransitionToServerScreen() {
            homeAlpha = 0f;
            homeScreen.blocksRaycasts = false;
            homeScreen.interactable = false;

            serverAlpha = 1f;
            serverScreen.blocksRaycasts = true;
            serverScreen.interactable = true;

            clientAlpha = 0f;
            clientScreen.blocksRaycasts = false;
            clientScreen.interactable = false;

            background.CrossFadeAlpha(1f, 0.5f, false);
        }

        public void TransitionToClientScreen() {
            homeAlpha = 0f;
            homeScreen.blocksRaycasts = false;
            homeScreen.interactable = false;

            serverAlpha = 0f;
            serverScreen.blocksRaycasts = false;
            serverScreen.interactable = false;

            clientAlpha = 1f;
            clientScreen.blocksRaycasts = true;
            clientScreen.interactable = true;

            background.CrossFadeAlpha(1f, 0.5f, false);
        }

        public void BackToHome() {
            homeAlpha = 1f;
            homeScreen.blocksRaycasts = true;
            homeScreen.interactable = true;

            serverAlpha = 0f;
            serverScreen.blocksRaycasts = false;
            serverScreen.interactable = false;

            clientAlpha = 0f;
            clientScreen.blocksRaycasts = false;
            clientScreen.interactable = false;

            background.CrossFadeAlpha(1f, 0.5f, false);
        }

        public void HideAllScreens() {
            // homeAlpha = 0f;
            // homeScreen.blocksRaycasts = false;
            // homeScreen.interactable = false;

            // serverAlpha = 0f;
            // serverScreen.blocksRaycasts = false;
            // serverScreen.interactable = false;

            // clientAlpha = 0f;
            // clientScreen.blocksRaycasts = false;
            // clientScreen.interactable = false;

            // background.CrossFadeAlpha(0f, 0.5f, false);

            // set canvas to inactive for succuessful VR raycasting 
            this.gameObject.SetActive(false);

            //change input module for vr input
            GameObject.Find("MR EventSystem").transform.GetComponent<InputSystemUIInputModule>().enabled = false;
            GameObject.Find("MR EventSystem").transform.GetComponent<OVRInputModule2>().enabled = true;

        }



        public void StartServer() {
            string address = serverAddressField.text;
            int port = Int32.Parse(serverPortField.text);
            MRConnectionManager.Singleton.StartServer(address, port);
            HideAllScreens();

            //Debug.Log()
        }

        public void ConnectClient() {
            string address = clientAddressField.text;
            int port = Int32.Parse(clientPortField.text);
            int hmdId = Int32.Parse(clientHmdIdField.text);
            Debug.Log("Connecting...");
            MRConnectionManager.Singleton.StartClient(address, port, hmdId);
            Debug.Log("Connected...");
            HideAllScreens();
            Debug.Log("Done...");
        }

        public void ValidateInput(string inputString) {
            if (serverAddressField.text.Length > 0) {
                try {
                    Int32.Parse(serverPortField.text);
                    startServerButton.interactable = true;
                } catch (FormatException) {
                    startServerButton.interactable = false;
                }
            } else {
                startServerButton.interactable = false;
            }
            
            if (clientAddressField.text.Length > 0) {
                try {
                    Int32.Parse(clientPortField.text);
                    Int32.Parse(clientHmdIdField.text);
                    connectClientButton.interactable = true;
                } catch (FormatException) {
                    connectClientButton.interactable = false;
                }
            } else {
                connectClientButton.interactable = false;
            }
                
        }
    }
}