using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace MRPlatform
{
    public class MRInteractionManager : MonoBehaviour
    {
        public Transform m_leftHandAnchor;
        public Transform m_rightHandAnchor;

        [HideInInspector]
        public OVRInputModule2 m_MRInputModule;

        private UIManagaer m_UIManager;
        public Camera m_OVRCamera;


        private void Start()
        {
            m_MRInputModule = FindObjectOfType<OVRInputModule2>();
            m_MRInputModule.rayTransform = m_rightHandAnchor;
            m_MRInputModule.m_Cursor = transform.Find("LaserPointer").GetComponent<OVRCursor>();

            m_UIManager = FindObjectOfType<UIManagaer>();

            if(m_UIManager)
            {
                foreach(Canvas c in m_UIManager.canvases)
                {
                    c.worldCamera = m_OVRCamera;
                    c.GetComponent<OVRRaycaster>().pointer = m_MRInputModule.m_Cursor.gameObject;
                    c.gameObject.SetActive(true);
                    
                }                
            }else{
                Debug.LogError("can't find avalibale UI Manager");
            }

        }

        private void Update()
        {
            // decide which controller for raycasting
            // if (OVRInput.GetActiveController() == OVRInput.Controller.LTouch)
            // {
            //     SetActiveController(OVRInput.Controller.LTouch);
            // }
            // else
            // {
            //     SetActiveController(OVRInput.Controller.RTouch);
            // }
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

