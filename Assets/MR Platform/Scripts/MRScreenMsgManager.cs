using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketServer;
using LitJson;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MRPlatform
{
    public class MRScreenMsgManager : MonoBehaviour
    {
        //[HideInInspector]
        //public MRTouchInput m_MRTouchInput;

        public UnityAction<int, int, string> myAction;
        public MyEvent myEvent = new MyEvent();
        public MRScreenInput m_MRScreenInput;

        //private bool isReleased;

        void Start()
        {
            //isReleased = false;
            // base.Start();

            myAction = new UnityAction<int, int, string>(m_MRScreenInput.UpdatePosition);
            myEvent.AddListener(myAction);
            //myAction += m_MRScreenInput.UpdatePosition;


        }

        public void OnOpen(WebSocketConnection connection)
        {
            Debug.Log("Client connected.");
        }

        public void OnMessage(WebSocketMessage message)
        {
            //Debug.Log(message.connection.id);
            //Debug.Log(message.id);
            Debug.Log(message.data);

            // get position
            try
            {
                var inputEvent = JsonMapper.ToObject<InputEventData>(message.data);
                Debug.Log("Type: " + inputEvent.eventType);
                Debug.Log("X: " + inputEvent.x);
                Debug.Log("y: " + inputEvent.y);

                //if(inputEvent.eventType == "TouchEnd")
                //{
                //    isReleased = true;
                //}

                myEvent.Invoke(inputEvent.x, inputEvent.y, inputEvent.eventType);

                //m_MRTouchInput.touchPose = new Vector2(inputEvent.x, inputEvent.y);
            }
            catch (Exception ex)
            {
                Debug.Log("error" + ex.Message);
            }


        }

        public void OnClose(WebSocketConnection connection)
        {
            Debug.Log("Client disconnected.");
        }


    }



    public class InputEventData
    {
        public string eventType;
        public int x;
        public int y;
    }

    public class MyEvent : UnityEvent<int, int, string>
    {

    }

}
