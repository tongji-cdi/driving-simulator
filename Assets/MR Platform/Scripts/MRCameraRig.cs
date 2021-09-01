using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Node = UnityEngine.XR.XRNode;


namespace MRPlatform {
    public class MRCameraRig : OVRCameraRig
    {
        override protected void UpdateAnchors(bool updateEyeAnchors, bool updateHandAnchors)
        {
            if (!OVRManager.OVRManagerinitialized)
                return;

            EnsureGameObjectIntegrity();

            if (!Application.isPlaying)
                return;

            if (_skipUpdate)
            {
                centerEyeAnchor.FromOVRPose(OVRPose.identity, true);
                leftEyeAnchor.FromOVRPose(OVRPose.identity, true);
                rightEyeAnchor.FromOVRPose(OVRPose.identity, true);

                return;
            }


            bool monoscopic = OVRManager.instance.monoscopic;
            bool hmdPresent = OVRNodeStateProperties.IsHmdPresent();

            OVRPose tracker = OVRManager.tracker.GetPose();

            trackerAnchor.localRotation = tracker.orientation;

            Quaternion emulatedRotation = Quaternion.Euler(-OVRManager.instance.headPoseRelativeOffsetRotation.x, -OVRManager.instance.headPoseRelativeOffsetRotation.y, OVRManager.instance.headPoseRelativeOffsetRotation.z);

            //Note: in the below code, when using UnityEngine's API, we only update anchor transforms if we have a new, fresh value this frame.
            //If we don't, it could mean that tracking is lost, etc. so the pose should not change in the virtual world.
            //This can be thought of as similar to calling InputTracking GetLocalPosition and Rotation, but only for doing so when the pose is valid.
            //If false is returned for any of these calls, then a new pose is not valid and thus should not be updated.
            if (updateEyeAnchors)
            {
                if (hmdPresent)
                {
                    Vector3 centerEyePosition = Vector3.zero;
                    Quaternion centerEyeRotation = Quaternion.identity;

                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.CenterEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out centerEyePosition))
                        centerEyeAnchor.localPosition = centerEyePosition;
                    if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.CenterEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out centerEyeRotation))
                        centerEyeAnchor.localRotation = centerEyeRotation;
                }
                else
                {
                    centerEyeAnchor.localRotation = emulatedRotation;
                    centerEyeAnchor.localPosition = OVRManager.instance.headPoseRelativeOffsetTranslation;
                }

                if (!hmdPresent || monoscopic)
                {
                    leftEyeAnchor.localPosition = centerEyeAnchor.localPosition;
                    rightEyeAnchor.localPosition = centerEyeAnchor.localPosition;
                    leftEyeAnchor.localRotation = centerEyeAnchor.localRotation;
                    rightEyeAnchor.localRotation = centerEyeAnchor.localRotation;
                }
                else
                {
                    Vector3 leftEyePosition = Vector3.zero;
                    Vector3 rightEyePosition = Vector3.zero;
                    Quaternion leftEyeRotation = Quaternion.identity;
                    Quaternion rightEyeRotation = Quaternion.identity;

                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.LeftEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeLeft, OVRPlugin.Step.Render, out leftEyePosition))
                        leftEyeAnchor.localPosition = leftEyePosition;
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.RightEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeRight, OVRPlugin.Step.Render, out rightEyePosition))
                        rightEyeAnchor.localPosition = rightEyePosition;
                    if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.LeftEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeLeft, OVRPlugin.Step.Render, out leftEyeRotation))
                        leftEyeAnchor.localRotation = leftEyeRotation;
                    if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.RightEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeRight, OVRPlugin.Step.Render, out rightEyeRotation))
                        rightEyeAnchor.localRotation = rightEyeRotation;
                }
            }

            if (updateHandAnchors)
            {
                //Need this for controller offset because if we're on OpenVR, we want to set the local poses as specified by Unity, but if we're not, OVRInput local position is the right anchor
                if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
                {
                    Vector3 leftPos = Vector3.zero;
                    Vector3 rightPos = Vector3.zero;
                    Quaternion leftQuat = Quaternion.identity;
                    Quaternion rightQuat = Quaternion.identity;

                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.LeftHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandLeft, OVRPlugin.Step.Render, out leftPos))
                        leftHandAnchor.localPosition = leftPos;
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.RightHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandRight, OVRPlugin.Step.Render, out rightPos))
                        rightHandAnchor.localPosition = rightPos;
                    if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.LeftHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.HandLeft, OVRPlugin.Step.Render, out leftQuat))
                        leftHandAnchor.localRotation = leftQuat;
                    if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.RightHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.HandRight, OVRPlugin.Step.Render, out rightQuat))
                        rightHandAnchor.localRotation = rightQuat;

                }
                else
                {
                    leftHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
                    rightHandAnchor.localPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
                    leftHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
                    rightHandAnchor.localRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
                }

                trackerAnchor.localPosition = tracker.position;

                OVRPose leftOffsetPose = OVRPose.identity;
                OVRPose rightOffsetPose = OVRPose.identity;
                if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
                {
                    leftOffsetPose = OVRManager.GetOpenVRControllerOffset(Node.LeftHand);
                    rightOffsetPose = OVRManager.GetOpenVRControllerOffset(Node.RightHand);

                    //Sets poses of left and right nodes, local to the tracking space.
                    OVRManager.SetOpenVRLocalPose(trackingSpace.InverseTransformPoint(leftControllerAnchor.position),
                        trackingSpace.InverseTransformPoint(rightControllerAnchor.position),
                        Quaternion.Inverse(trackingSpace.rotation) * leftControllerAnchor.rotation,
                        Quaternion.Inverse(trackingSpace.rotation) * rightControllerAnchor.rotation);
                }
                rightControllerAnchor.localPosition = rightOffsetPose.position;
                rightControllerAnchor.localRotation = rightOffsetPose.orientation;
                leftControllerAnchor.localPosition = leftOffsetPose.position;
                leftControllerAnchor.localRotation = leftOffsetPose.orientation;
            }
            
            // Change everything to be relative to centerEyeAnchor, then set centerEyeAnchor position to zero.
            leftHandAnchor.localPosition -= centerEyeAnchor.localPosition;
            rightHandAnchor.localPosition -= centerEyeAnchor.localPosition;
            leftControllerAnchor.localPosition -= centerEyeAnchor.localPosition;
            rightControllerAnchor.localPosition -= centerEyeAnchor.localPosition;
            centerEyeAnchor.localPosition = Vector3.zero;
            RaiseUpdatedAnchorsEvent();
        }
    }
}