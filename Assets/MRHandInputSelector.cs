using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace MRPlatform
{
    public class MRHandInputSelector : MonoBehaviour
    {
        public Transform m_leftHandAnchor;
        public Transform m_rightHandAnchor;

        [HideInInspector]
        public MRInputModule m_MRInputModule;


        private void Start()
        {
            m_MRInputModule = FindObjectOfType<MRInputModule>();
            m_MRInputModule.m_Cursor = transform.Find("LaserPointer").GetComponent<OVRCursor>();
        }

        private void Update()
        {
            if (OVRInput.GetActiveController() == OVRInput.Controller.LTouch)
            {
                SetActiveController(OVRInput.Controller.LTouch);
            }
            else
            {
                SetActiveController(OVRInput.Controller.RTouch);
            }
        }

        void SetActiveController(OVRInput.Controller c)
        {
            Transform t;
            if (c == OVRInput.Controller.LTouch)
            {
                t = m_leftHandAnchor;
            }
            else
            {
                t = m_rightHandAnchor;
            }
            m_MRInputModule.rayTransform = t;
        }
    }
}

