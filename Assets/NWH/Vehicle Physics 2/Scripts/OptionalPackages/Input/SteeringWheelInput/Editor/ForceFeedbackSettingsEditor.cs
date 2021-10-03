#if UNITY_EDITOR

using NWH.NUI;
using UnityEditor;

namespace NWH.VehiclePhysics2.Input
{
    [CustomEditor(typeof(ForceFeedbackSettings))]
    public class ForceFeedbackSettingsEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.Info("Vehicle-specific FFB settings.");
            drawer.Field("overallCoeff");
            drawer.Field("frictionCoeff");
            drawer.Field("lowSpeedFrictionCoeff");
            drawer.Field("satCoeff", true, null, "Self Aligning Torque Coeff");
            drawer.Field("centeringCoeff");

            drawer.EndEditor(this);
            return true;
        }

        public override bool UseDefaultMargins()
        {
            return false;
        }
    }
}

#endif