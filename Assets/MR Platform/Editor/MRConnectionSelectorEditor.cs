using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MRPlatform
{
    [CustomEditor(typeof(MRPlatformManager))]
    public class MRConnectionSelectorEditor : Editor
    {

        MRPlatformManager mrPlatformManager;

        private void OnEnable()
        {
            mrPlatformManager = (MRPlatformManager)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.BeginVertical();

            //EditorGUILayout.LabelField("address", mrPlatformManager.address);

            {
                string buttonDisabledReasonSuffix = "";

                if (!EditorApplication.isPlaying)
                {
                    buttonDisabledReasonSuffix = ". This can only be done in play mode";
                    GUI.enabled = false;
                }

                if (GUILayout.Button(new GUIContent("Start Host Mode", "Starts a host instance" + buttonDisabledReasonSuffix)))
                {
                    MRConnectionManager.Singleton.StartServer(mrPlatformManager.address, mrPlatformManager.port);
                    mrPlatformManager.m_MRConnectionScreen.HideAllScreens();
                }
                if (GUILayout.Button(new GUIContent("Start Clinet Mode", "Starts a client instance" + buttonDisabledReasonSuffix)))
                {
                    MRConnectionManager.Singleton.StartClient(mrPlatformManager.address, mrPlatformManager.port, mrPlatformManager.hmdID);
                    mrPlatformManager.m_MRConnectionScreen.HideAllScreens();
                }

            }


            EditorGUILayout.EndVertical();
        }


    }
}

