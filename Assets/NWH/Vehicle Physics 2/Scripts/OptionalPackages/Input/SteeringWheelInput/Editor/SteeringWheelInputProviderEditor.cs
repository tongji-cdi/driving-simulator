#if UNITY_EDITOR

using NWH.NUI;
using UnityEditor;

namespace NWH.VehiclePhysics2.Input
{
    [CustomEditor(typeof(SteeringWheelInputProvider))]
    public class SteeringWheelInputProviderEditor : NUIEditor
    {
        public override bool OnInspectorNUI()
        {
            if (!base.OnInspectorNUI())
            {
                return false;
            }

            drawer.BeginSubsection("Target Vehicle");
            if (drawer.Field("vehicleSource").enumValueIndex == 1)
            {
                drawer.Field("vehicleController");
            }
            drawer.EndSubsection();
            
            drawer.BeginSubsection("Forces");
            drawer.Field("overallEffectStrength", true, "%");
            drawer.Field("maximumWheelForce", true, "%");
            drawer.Field("smoothing");
            drawer.Field("useDirectInput");

            drawer.BeginSubsection("Low Speed Friction");
            drawer.Field("lowSpeedFriction");
            drawer.EndSubsection();

            drawer.BeginSubsection("Self Aligning Torque");
            drawer.Field("maxSatForce", true, "%", "Max. Sat Force");
            drawer.Field("slipSATCurve");
            drawer.Field("slipMultiplier");
            drawer.EndSubsection();

            drawer.BeginSubsection("Friction");
            drawer.Field("friction");
            drawer.EndSubsection();

            drawer.BeginSubsection("Centering Force");
            drawer.Field("centeringForceStrength");
            drawer.Field("centerPositionDrift");
            drawer.EndSubsection();

            drawer.BeginSubsection("Debug Values");
            drawer.Field("_lowSpeedFrictionForce", false);
            drawer.Field("_satForce", false);
            drawer.Field("_frictionForce", false);
            drawer.Field("_centeringForce", false);
            drawer.Field("_totalForce", false);
            drawer.Info("Total Force should never exceed 100. This will result in force clipping as wheels can not reproduce forces above 100%.");
            drawer.EndSubsection();
            
            drawer.EndSubsection();
            
            drawer.BeginSubsection("Input");

            drawer.BeginSubsection("Axes");
            drawer.Field("axisResolution");
            drawer.Field("wheelRotationRange");
            drawer.Space();
            drawer.Field("steeringAxis");
            drawer.Field("flipSteeringInput");
            drawer.Space();
            drawer.Field("throttleAxis");
            drawer.Field("flipThrottleInput");
            drawer.Space();
            drawer.Field("brakeAxis");
            drawer.Field("flipBrakeInput");
            drawer.Space();
            drawer.Field("clutchAxis");
            drawer.Field("flipClutchInput");
            drawer.EndSubsection();
            
            drawer.BeginSubsection("Buttons");
            drawer.BeginSubsection("Sequential Shifter");
            drawer.Field("shiftUpButton");
            drawer.Field("altShiftUpButton");
            drawer.Field("shiftDownButton");
            drawer.Field("altShiftDownButton");
            drawer.EndSubsection();
            drawer.BeginSubsection("H-shifter");
            drawer.Field("shiftIntoReverseButton");
            drawer.Field("shiftIntoNeutralButton");
            drawer.Field("shiftInto1stButton");
            drawer.Field("shiftInto2ndButton");
            drawer.Field("shiftInto3rdButton");
            drawer.Field("shiftInto4thButton");
            drawer.Field("shiftInto5thButton");
            drawer.Field("shiftInto6thButton");
            drawer.Field("shiftInto7thButton");
            drawer.Field("shiftInto8thButton");
            drawer.Field("shiftInto9thButton");
            drawer.EndSubsection();
            drawer.EndSubsection();

            drawer.BeginSubsection("Input Debug Values");
            drawer.Field("_steeringInput", false);
            drawer.Field("_throttleInput", false);
            drawer.Field("_brakeInput", false);
            drawer.Field("_clutchInput", false);
            drawer.Field("_shiftUpInput", false);
            drawer.Field("_shiftDownInput", false);
            drawer.Field("_shiftIntoInput", false);
            drawer.EndSubsection();
            drawer.EndSubsection();
            
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