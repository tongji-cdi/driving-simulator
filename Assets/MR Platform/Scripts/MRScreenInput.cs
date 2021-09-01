using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;


namespace MRPlatform { 

    [RequireComponent(typeof(Canvas))]
    public class MRScreenInput : MonoBehaviour
    {

        private Canvas m_canvas;
        private float canvasWidth;
        private float canvasHeight;

        public enum State
        {
            Start,
            Move,
            End,
            None
        }

        [HideInInspector]
        public State state = State.None;

        private bool isReleased;

        [HideInInspector]
        public float x;
        [HideInInspector]
        public float y;

        private void Awake()
        {
            x = 10000f;
            y = 10000f;

            isReleased = false;

            m_canvas = GetComponent<Canvas>();

            canvasWidth = GetComponent<RectTransform>().rect.width;
            canvasHeight = GetComponent<RectTransform>().rect.height;
        }

        //private void Start()
        //{
        //    m_canvas = GetComponent<Canvas>();

        //    canvasWidth = GetComponent<RectTransform>().rect.width;
        //    canvasHeight = GetComponent<RectTransform>().rect.height;

        //}

        private void Update()
        {
            if (isReleased)
            {
                UpdatePosition(0, 0, "None");
                isReleased = false;
            }

            if(state == State.End)
            {
                isReleased = true;
            }
        }

        public bool PressedThisFrame()
        {
            bool m_PressedThisFrame;
            if (state == State.Start|| state == State.Move)
            {
                m_PressedThisFrame = true;
            }else
            {
                m_PressedThisFrame = false;
            }

            return m_PressedThisFrame;

        }

        public bool ReleasedThisFrame()
        {
            bool m_ReleasedThisFrame;
            if (state == State.End)
            {
                m_ReleasedThisFrame = true;
            }
            else
            {
                m_ReleasedThisFrame = false;
            }

            return m_ReleasedThisFrame;
        }

        public Vector2 GetPosition()
        {
            return new Vector2(x, y);
        }

        public void UpdatePosition(int _x,int _y,string eventType)
        {
            x = _x - canvasWidth / 2;
            y = - _y + canvasHeight / 2;

            switch (eventType)
            {
                case "TouchStart":
                    state = State.Start;
                    break;
                case "TouchMove":
                    state = State.Move;
                    break;
                case "TouchEnd":
                    state = State.End;
                    break;
                case "None":
                    state = State.None;
                    break;
            }
        }
    }

}