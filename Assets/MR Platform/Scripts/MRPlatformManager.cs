using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MRPlatform
{
    public class MRPlatformManager : MonoBehaviour
    {
        public string address = "127.0.0.1";
        public int port = 7777;
        public int hmdID = 1;

        [HideInInspector]
        public MRConnectionScreen m_MRConnectionScreen;

        private void OnEnable()
        {
            m_MRConnectionScreen = GameObject.Find("MR Connection Screen").GetComponent<MRConnectionScreen>();
        }

    }

}
