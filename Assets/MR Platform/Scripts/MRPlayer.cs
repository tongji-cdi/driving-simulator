using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

using System;
using System.Runtime.InteropServices;

namespace MRPlatform {
    public class MRPlayer : MonoBehaviour
    {
        // public CharacterController characterController;
        public OptitrackStreamingClient StreamingClient;

        public Int32 RigidBodyId;
        // public Transform oculusParentTransform;
        // public Transform oculusCameraRig;
        // public Transform oculusTrackingCenter;
        private GameObject m_hmdCameraObject;
        private IntPtr m_driftCorrHandle;

        // public float speed = 12f;

        OptitrackRigidBody optitrack;

        
        
        void Start()
        {
            if ( this.StreamingClient == null )
            {
                this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();
                //Debug.Log(this.StreamingClient.GetComponent<OptitrackStreamingClient>().isActiveAndEnabled);

                // If we still couldn't find one, disable this component.
                if ( this.StreamingClient == null || this.StreamingClient.GetComponent<OptitrackStreamingClient>().isActiveAndEnabled == false)
                {
                    Debug.Log( GetType().FullName + ": Streaming client not set or not enable, and no " + typeof( OptitrackStreamingClient ).FullName + " components found in scene; disabling this component.", this );
                    Debug.Log("Start Debug Mode");

                    //gameObject.SetActive( false );
                    StartDebugMode();


                    return;
                }
            }

            // Get HMD id from Network Manager
            MRConnectionManager m_MRConnectionManager = GameObject.FindObjectOfType<MRConnectionManager>();
            if(m_MRConnectionManager == null)
            {
                Debug.LogError( "Couldn't find Network Manager");
            }else{
                RigidBodyId = m_MRConnectionManager.rigidBodyId;
            }


            // Cache a reference to the gameobject containing the HMD Camera.
            Camera hmdCamera = this.GetComponentInChildren<Camera>();
            if ( hmdCamera == null )
            {
                Debug.LogError( GetType().FullName + ": Couldn't locate HMD-driven Camera component in children.", this );
            }
            else
            {
                m_hmdCameraObject = hmdCamera.gameObject;
            }
        }

        void StartDebugMode()
        {
            var ovrCameraRig = this.transform.Find("OVRCameraRig");
            ovrCameraRig.GetComponent<MRCameraRig>().enabled = false;
            ovrCameraRig.GetComponent<OVRCameraRig>().enabled = true;

            this.enabled = false;

        }

        void OnEnable()
        {
            NpHmdResult result = NativeMethods.NpHmd_Create( out m_driftCorrHandle );
            if ( result != NpHmdResult.OK || m_driftCorrHandle == IntPtr.Zero )
            {
                Debug.LogError( GetType().FullName + ": NpHmd_GetOrientationCorrection failed.", this );
                m_driftCorrHandle = IntPtr.Zero;
                this.enabled = false;
                return;
            }
        }


        void OnDisable()
        {
            if ( m_driftCorrHandle != IntPtr.Zero )
            {
                NativeMethods.NpHmd_Destroy( m_driftCorrHandle );
                m_driftCorrHandle = IntPtr.Zero;
            }
        }

        void Update()
        {
            OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState( RigidBodyId );
            if ( rbState != null && rbState.DeliveryTimestamp.AgeSeconds < 1.0f )
            {
                // Update position.
                this.transform.localPosition = rbState.Pose.Position;
                
                // Calculate orientation correction based on both OptiTrack and Oculus
                NpHmdQuaternion opticalOri = new NpHmdQuaternion( rbState.Pose.Orientation );
                NpHmdQuaternion inertialOri = new NpHmdQuaternion( m_hmdCameraObject.transform.localRotation );

                NpHmdResult result = NativeMethods.NpHmd_MeasurementUpdate(
                    m_driftCorrHandle,
                    ref opticalOri, // const
                    ref inertialOri, // const
                    Time.deltaTime
                );

                if ( result == NpHmdResult.OK )
                {
                    NpHmdQuaternion newCorrection;
                    result = NativeMethods.NpHmd_GetOrientationCorrection( m_driftCorrHandle, out newCorrection );

                    if ( result == NpHmdResult.OK )
                    {
                        this.transform.localRotation = newCorrection;
                    }
                    else
                    {
                        Debug.LogError( GetType().FullName + ": NpHmd_GetOrientationCorrection failed.", this );
                        this.enabled = false;
                        return;
                    }
                }
                else
                {
                    Debug.LogError( GetType().FullName + ": NpHmd_MeasurementUpdate failed.", this );
                    this.enabled = false;
                    return;
                }

            }
        }
    }


    enum NpHmdResult
    {
        OK = 0,
        InvalidArgument
    }


    struct NpHmdQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public NpHmdQuaternion( UnityEngine.Quaternion other )
        {
            this.x = other.x;
            this.y = other.y;
            this.z = other.z;
            this.w = other.w;
        }

        public static implicit operator UnityEngine.Quaternion( NpHmdQuaternion nphmdQuat )
        {
            return new UnityEngine.Quaternion
            {
                w = nphmdQuat.w,
                x = nphmdQuat.x,
                y = nphmdQuat.y,
                z = nphmdQuat.z
            };
        }
    }

    static class NativeMethods
    {
        public const string NpHmdDllBaseName = "HmdDriftCorrection";
        public const CallingConvention NpHmdDllCallingConvention = CallingConvention.Cdecl;

        [DllImport( NpHmdDllBaseName, CallingConvention = NpHmdDllCallingConvention )]
        public static extern NpHmdResult NpHmd_UnityInit();

        [DllImport( NpHmdDllBaseName, CallingConvention = NpHmdDllCallingConvention )]
        public static extern NpHmdResult NpHmd_Create( out IntPtr hmdHandle );

        [DllImport( NpHmdDllBaseName, CallingConvention = NpHmdDllCallingConvention )]
        public static extern NpHmdResult NpHmd_Destroy( IntPtr hmdHandle );

        [DllImport( NpHmdDllBaseName, CallingConvention = NpHmdDllCallingConvention )]
        public static extern NpHmdResult NpHmd_MeasurementUpdate( IntPtr hmdHandle, ref NpHmdQuaternion opticalOrientation, ref NpHmdQuaternion inertialOrientation, float deltaTimeSec );

        [DllImport( NpHmdDllBaseName, CallingConvention = NpHmdDllCallingConvention )]
        public static extern NpHmdResult NpHmd_GetOrientationCorrection( IntPtr hmdHandle, out NpHmdQuaternion correction );


        public const string OvrPluginDllBaseName = "OVRPlugin";
        public const CallingConvention OvrPluginDllCallingConvention = CallingConvention.Cdecl;

        [DllImport( OvrPluginDllBaseName, CallingConvention = OvrPluginDllCallingConvention )]
        public static extern Int32 ovrp_GetCaps();

        [DllImport( OvrPluginDllBaseName, CallingConvention = OvrPluginDllCallingConvention )]
        public static extern Int32 ovrp_SetCaps( Int32 caps );

        [DllImport( OvrPluginDllBaseName, CallingConvention = OvrPluginDllCallingConvention )]
        public static extern Int32 ovrp_SetTrackingIPDEnabled( Int32 value );
    }

}