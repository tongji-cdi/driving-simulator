using NWH.Common.SceneManagement;
using NWH.WheelController3D;
using UnityEngine;
using UnityEngine.Serialization;

namespace NWH.VehiclePhysics2.Input
{
    /// <summary>
    /// Handles input from steeling wheels such as Logitech, Thrustmaster, etc.
    /// Also calculates force feedback.
    /// </summary>
    public class SteeringWheelInputProvider : VehicleInputProviderBase
    {
        public bool[] buttonDown = new bool[128];
        public bool[] buttonPressed = new bool[128];
        public bool[] buttonWasPressed = new bool[128];

        public enum Axis
        {
            XPosition, 
            YPosition, 
            ZPosition, 
            XRotatation, 
            YRotation, 
            ZRotation, 
            rglSlider0, 
            rglSlider1, 
            rglSlider2, 
            rglSlider3, 
            rglASlider0, 
            rglASlider1, 
            rglASlider2, 
            rglASlider3, 
            rglFSlider0, 
            rglFSlider1, 
            rglFSlider2, 
            rglFSlider3, 
            rglVSlider0, 
            rglVSlider1, 
            rglVSlider2, 
            rglVSlider3, 
            lArx,
            lAry,
            lArz,
            lAx,
            lAy,
            lAz,
            lFRx,
            lFRy,
            lFRz,
            lFx,
            lFy,
            lFz,
            lVRx,
            lVRy,
            lVRz,
            lVx,
            lVy,
            lVz,
            None
        }

        public enum VehicleSource { VehicleChanger, VehicleController }

        /// <summary>
        /// Should all steering filtering and smoothing be ignored?
        /// </summary>
        [UnityEngine.Tooltip("Should all steering filtering and smoothing be ignored?")]
        public bool useDirectInput = true;
        
        /// <summary>
        /// Determines where from the target VehicleController is retrieved.
        /// </summary>
        [UnityEngine.Tooltip("Determines where from the target VehicleController is retrieved.")]
        public VehicleSource vehicleSource = VehicleSource.VehicleChanger;
        
        /// <summary>
        /// Target VehicleController.
        /// </summary>
        [UnityEngine.Tooltip("Target VehicleController.")]
        public VehicleController vehicleController;

        /// <summary>
        /// Maximum wheel force. This is a hardware settings for the used device.
        /// </summary>
        [Range(0,100)] public int maximumWheelForce = 100;
        
        /// <summary>
        /// Multiplier by which the overall force is multiplied. Set to 0 to disable force feedback.
        /// </summary>
        [UnityEngine.Tooltip("Multiplier by which the overall force is multiplied. Set to 0 to disable force feedback.")]
        public float overallEffectStrength = 1f;
        
        /// <summary>
        /// Smoothing of the forces. Set to 0 for most direct feedback.
        /// </summary>
        [Range(0, 0.1f)] public float smoothing = 0f;
        
        /// <summary>
        /// Range of rotation of the wheel in degrees. Most wheels are capable of 900 degrees
        /// but this will be too slow for most games (except for truck simulators).
        /// 540 is usually used in sim racing.
        /// </summary>
        [FormerlySerializedAs("wheelRange")]
        [UnityEngine.Tooltip("Range of rotation of the wheel in degrees. Most wheels are capable of 900 degrees\r\nbut this will be too slow for most games (except for truck simulators).\r\n540 is usually used in sim racing.")]
        public int wheelRotationRange = 540;

        /// <summary>
        /// Fix non-linear input from LogitechSDK.
        /// </summary>
        [UnityEngine.Tooltip("Fix non-linear input from LogitechSDK.")]
        public bool linearizeSDKInput = true;        
        
        /// <summary>
        /// Curve that shows how the self aligning torque acts in relation to wheel slip.
        /// Vertical axis is force coefficient, horizontal axis is slip.
        /// </summary>
        [UnityEngine.Tooltip(
            "Curve that shows how the self aligning torque acts in relation to wheel slip.\r\nVertical axis is force coefficient, horizontal axis is slip.")]
        public AnimationCurve slipSATCurve = new AnimationCurve(
            new Keyframe(0, 0, -0.9f, 25f),
            new Keyframe(0.07f, 1),
            new Keyframe(0.16f, 0.93f, -1f, -1f),
            new Keyframe(1f, 0.2f)
        );

         /// <summary>
        /// Maximum force that can be achieved as a result of self aligning torque.
        /// </summary>
        [UnityEngine.Tooltip("Maximum force that can be achieved as a result of self aligning torque.")]
        public float maxSatForce = 80;
        
        /// <summary>
        /// Slip is multiplied by this value before it is used to evaluate slipSATCurve.
        /// Use to adjust the point at which the wheel will begin to loosen as a result of wheel skid.
        /// </summary>
        [UnityEngine.Tooltip("Slip is multiplied by this value before it is used to evaluate slipSATCurve.\r\nUse to adjust the point at which the wheel will begin to loosen as a result of wheel skid.")]
        public float slipMultiplier = 4.6f;
        
        /// <summary>
        /// Friction when vehicle is stationary or near stationary.
        /// </summary>
        [UnityEngine.Tooltip("Friction when vehicle is stationary or near stationary.")]
        public float lowSpeedFriction = 70f;
        
        /// <summary>
        /// Friction when vehicle is moving.
        /// </summary>
        [UnityEngine.Tooltip("Friction when vehicle is moving.")]
        public float friction = 16f;

        /// <summary>
        /// Strength of centering force (the tendency of the steering wheel to center itself).
        /// Also affects the feel of bumps as the steering center moves based on suspension compression (e.g. when
        /// right wheel goes over a bump on the road the steering wheel center moves to the left).
        /// </summary>
        [UnityEngine.Tooltip("Strength of centering force (the tendency of the steering wheel to center itself).\r\nAlso affects the feel of bumps as the steering center moves based on suspension compression (e.g. when\r\nright wheel goes over a bump on the road the steering wheel center moves to the left).")]
        public float centeringForceStrength = 60;
        
        /// <summary>
        /// How much the steering center will move based on difference on compression of suspension on the
        /// left and right side. 
        /// </summary>
        [Range(0, 1)] public float centerPositionDrift = 0.4f;
        
        /// <summary>
        /// Axis resolution of the wheel's ADC.
        /// </summary>
        [UnityEngine.Tooltip("Axis resolution of the wheel's ADC.")]
        public int axisResolution = 65536;
        
        /// <summary>
        /// Flips the sign on the steering input.
        /// </summary>
        [UnityEngine.Tooltip("Flips the sign on the steering input.")]
        public bool flipSteeringInput = false;
        
        /// <summary>
        /// Flips the sign on the throttle input.
        /// </summary>
        [UnityEngine.Tooltip("Flips the sign on the throttle input.")]
        public bool flipThrottleInput = true;
        
        /// <summary>
        /// Flips the sign on the brake input.
        /// </summary>
        [UnityEngine.Tooltip("Flips the sign on the brake input.")]
        public bool flipBrakeInput = true;
        
        /// <summary>
        /// Flips the sign on the clutch input.
        /// </summary>
        [UnityEngine.Tooltip("Flips the sign on the clutch input.")]
        public bool flipClutchInput = true;
        
        /// <summary>
        /// Determines which wheel axis will be used for steering.
        /// </summary>
        [UnityEngine.Tooltip("Determines which wheel axis will be used for steering.")]
        public Axis steeringAxis = Axis.XPosition;
        
        /// <summary>
        /// Determines which wheel axis will be used for throttle.
        /// </summary>
        [UnityEngine.Tooltip("Determines which wheel axis will be used for throttle.")]
        public Axis throttleAxis = Axis.YPosition;
        
        /// <summary>
        /// Determines which wheel axis will be used for braking.
        /// </summary>
        [UnityEngine.Tooltip("Determines which wheel axis will be used for braking.")]
        public Axis brakeAxis = Axis.ZRotation;
        
        /// <summary>
        /// Determines which wheel axis will be used for clutch.
        /// </summary>
        [UnityEngine.Tooltip("Determines which wheel axis will be used for clutch.")]
        public Axis clutchAxis = Axis.ZPosition;
        
        /// <summary>
        /// Determines which wheel axis will be used for steering.
        /// If there is no analog axis for handbrake, handbrakeButton mapping can be used instead.
        /// </summary>
        [UnityEngine.Tooltip("Determines which wheel axis will be used for steering.\r\nIf there is no analog axis for handbrake, handbrakeButton mapping can be used instead.")]
        public Axis handbrakeAxis = Axis.None;
        
        /// <summary>
        /// Primary shift up button.
        /// </summary>
        [UnityEngine.Tooltip("Primary shift up button.")]
        public int shiftUpButton = 12;
        
        /// <summary>
        /// Primary shift down button.
        /// </summary>
        [UnityEngine.Tooltip("Primary shift down button.")]
        public int shiftDownButton = 13;
        
        /// <summary>
        /// Alternative shift up button.
        /// To be used when there is both a sequential stick shifter and paddles.
        /// </summary>
        [UnityEngine.Tooltip("Alternative shift up button.\r\nTo be used when there is both a sequential stick shifter and paddles.")]
        public int altShiftUpButton = 4;
        
        /// <summary>
        /// Alternative shift down button.
        /// To be used when there is both a sequential stick shifter and paddles.
        /// </summary>
        [UnityEngine.Tooltip("Alternative shift down button.\r\nTo be used when there is both a sequential stick shifter and paddles.")]
        public int altShiftDownButton = 5;
        
        /// <summary>
        /// Button used to shift into reverse gear.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into reverse gear.")]
        public int shiftIntoReverseButton = -1;
        
        /// <summary>
        /// Button used to shift into neutral gear.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into neutral gear.")]
        public int shiftIntoNeutralButton= -1;
        
        /// <summary>
        /// Button used to shift into 1st gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 1st gear. Set to -1 to disable.")]
        
        public int shiftInto1stButton = -1;
        
        /// <summary>
        /// Button used to shift into 2nd gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 2nd gear. Set to -1 to disable.")]
        public int shiftInto2ndButton = -1;
        
        /// <summary>
        /// Button used to shift into 3rd gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 3rd gear. Set to -1 to disable.")]
        public int shiftInto3rdButton = -1;
        
        /// <summary>
        /// Button used to shift into 4th gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 4th gear. Set to -1 to disable.")]
        public int shiftInto4thButton = -1;
        
        /// <summary>
        /// Button used to shift into 5th gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 5th gear. Set to -1 to disable.")]
        public int shiftInto5thButton = -1;
        
        /// <summary>
        /// Button used to shift into 6th gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 6th gear. Set to -1 to disable.")]
        public int shiftInto6thButton = -1;
        
        /// <summary>
        /// Button used to shift into 7th gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 7th gear. Set to -1 to disable.")]
        public int shiftInto7thButton = -1;
        
        /// <summary>
        /// Button used to shift into 8th gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 8th gear. Set to -1 to disable.")]
        public int shiftInto8thButton = -1;
        
        /// <summary>
        /// Button used to shift into 9th gear.
        /// Set to -1 to disable.
        /// </summary>
        [UnityEngine.Tooltip("Button used to shift into 9th gear. Set to -1 to disable.")]
        public int shiftInto9thButton = -1;
        
        /// <summary>
        /// Button used to trigger handbrake.
        /// For analog input use handbrakeAxis mapping instead.
        /// </summary>
        [UnityEngine.Tooltip("Button used to trigger handbrake.\r\nFor analog input use handbrakeAxis mapping instead.")]
        public int handbrakeButton = -1;
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _steeringInput         = 0;
            _throttleInput         = 0;
            _brakeInput            = 0;
            _clutchInput           = 0;
            _handbrakeInput        = 0;
            _shiftIntoInput        = -999;
            _shiftUpInput          = false;
            _shiftDownInput        = false;
            _lowSpeedFrictionForce = 0;
            _totalForce            = 0;
            _satForce              = 0;
            _frictionForce         = 0;
            _centeringForce        = 0;
            _centerPosition        = 0;
        }
        
        // Inputs
        [SerializeField][Range(-1, 1)] private float _steeringInput;
        [SerializeField][Range(0, 1)] private float _throttleInput;
        [SerializeField][Range(0, 1)] private float _brakeInput;
        [SerializeField][Range(0, 1)] private float _clutchInput;
        [SerializeField][Range(0, 1)] private float _handbrakeInput = 0;
        [SerializeField] private int _shiftIntoInput = -999;
        [SerializeField] private bool _shiftUpInput = false;
        [SerializeField] private bool _shiftDownInput = false;
        
        //Forces
        [SerializeField][Range(0, 100)] private float _lowSpeedFrictionForce;
        [SerializeField][Range(0, 100)] private float _totalForce = 0;
        [SerializeField][Range(0, 100)] private float _satForce;
        [SerializeField] [Range(0, 100)] private float _frictionForce;
        [SerializeField][Range(0, 100)] private float _centeringForce;

        private float _centerPosition;
        private bool _steeringWheelConnected;
        private float _prevSteering;
        private float _steerVelocity;
        private ForceFeedbackSettings _ffbSettings;
        private LogitechGSDK.LogiControllerPropertiesData _properties;
        private WheelController _leftWheel;
        private WheelController _rightWheel;
        private LogitechGSDK.DIJOYSTATE2ENGINES _wheelInput;
        private float _totalForceVelocity;
        
        // Vehicle-specific coefficients
        private float _overallCoeff = 1f;
        private float _frictionCoeff = 1f;
        private float _lowSpeedFrictionCoeff = 1f;
        private float _satCoeff = 1f;
        private float _centeringCoeff = 1f;

        /// <summary>
        /// Is the steering wheel currently connected? Read only.
        /// </summary>
        public bool SteeringWheelConnected
        {
            get { return _steeringWheelConnected; }
        }

        void Start()
        {
            LogitechGSDK.LogiSteeringInitialize(false);
            UpdateWheelSettings();

            buttonDown = new bool[128];
            buttonWasPressed = new bool[128];
            buttonPressed = new bool[128];
        }

        private void Reset()
        {
            slipSATCurve = new AnimationCurve(
                new Keyframe(0, 0, -0.9f, 25f),
                new Keyframe(0.07f, 1),
                new Keyframe(0.16f, 0.93f, -1f, -1f),
                new Keyframe(1f, 0.2f)
            );
        }

        private bool WheelIsConnected
        {
            get { return LogitechGSDK.LogiIsConnected(0); }
        }

        private void Update()
        {
            if (!WheelIsConnected)
            {
                return;
            }
            
            UpdateWheelSettings();
            GetWheelInputs();
            SetVehicleInputs();
        }

        void FixedUpdate()
        {
            if (vehicleSource == VehicleSource.VehicleChanger)
            {
                if (VehicleChanger.Instance == null)
                {
                    Debug.LogError("Vehicle source is set to VehicleChanger but VehicleChanger is not present in the scene.");
                    return;
                }

                vehicleController = (VehicleController)VehicleChanger.ActiveVehicle;
                if (vehicleController == null)
                {
                    return;
                }
            }
            else
            {
                if (vehicleController == null)
                {
                    Debug.LogError("SteeringWheelInput source set to VehicleController, yet no VehicleController has been assigned.");
                    return;
                }
            }

            _ffbSettings = vehicleController.GetComponent<ForceFeedbackSettings>();
            if (_ffbSettings == null)
            {
                _overallCoeff = 1f;
                _frictionCoeff = 1f;
                _lowSpeedFrictionCoeff = 1f;
                _satCoeff = 1f;
                _centeringCoeff = 1f;
            }
            else
            {
                _overallCoeff = _ffbSettings.overallCoeff;
                _frictionCoeff = _ffbSettings.frictionCoeff;
                _lowSpeedFrictionCoeff = _ffbSettings.lowSpeedFrictionCoeff;
                _satCoeff = _ffbSettings.satCoeff;
                _centeringCoeff = _ffbSettings.centeringCoeff;
            }
            
            if (!vehicleController.IsAwake)
            {
                ResetForce();
                return;
            }
            
            float newTotalForce = 0;
            
            if (WheelIsConnected && LogitechGSDK.LogiUpdate())
            {
                _steeringWheelConnected = true;

                vehicleController.steering.useDirectInput = useDirectInput;

                _leftWheel = vehicleController.Wheels[0].wheelController;
                _rightWheel = vehicleController.Wheels[1].wheelController;

                // Self Aligning Torque
                float leftFactor = _leftWheel.wheel.load / 12000f * _leftWheel.activeFrictionPreset.BCDE.z;
                float rightFactor = _rightWheel.wheel.load / 12000f * _rightWheel.activeFrictionPreset.BCDE.z;
                float combinedFactor = leftFactor + rightFactor;
                float totalSlip = _leftWheel.sideFriction.slip * leftFactor + _rightWheel.sideFriction.slip * rightFactor;
                float absSlip = totalSlip < 0 ? -totalSlip : totalSlip;
                float slipSign = totalSlip < 0 ? -1f : 1f;
                _satForce = slipSATCurve.Evaluate(absSlip * slipMultiplier) * -slipSign * maxSatForce * combinedFactor * _satCoeff;
                newTotalForce += Mathf.Lerp(0f, _satForce, vehicleController.Speed - 0.4f);
                
                // Determine target center  position (changes with spring compression)
                _centerPosition = (_rightWheel.springCompression - _leftWheel.springCompression) * centerPositionDrift;
                
                // Calculate centering force
                _centeringForce = (_steeringInput - _centerPosition) * centeringForceStrength * _centeringCoeff;
                newTotalForce += _centeringForce;

                // Low speed friction
                _lowSpeedFrictionForce = Mathf.Lerp(lowSpeedFriction, 0, vehicleController.Speed - 0.2f) * _lowSpeedFrictionCoeff;
                
                // Friction 
                _frictionForce = friction * _frictionCoeff;

                // Apply friction
                LogitechGSDK.LogiPlayDamperForce(0, (int)(_lowSpeedFrictionForce + _frictionForce));

                newTotalForce *= overallEffectStrength * _overallCoeff;
                if (smoothing < 0.001f)
                {
                    _totalForce = newTotalForce;
                }
                else
                {
                    _totalForce = Mathf.SmoothDamp(_totalForce, newTotalForce, ref _totalForceVelocity, smoothing);
                }

                AddForce(_totalForce);

                _prevSteering = _steeringInput;

                if (vehicleController.damageHandler.lastCollisionTime + 0.3f > Time.realtimeSinceStartup)
                {
                    int strength = (int)(vehicleController.damageHandler.lastCollision.impulse.magnitude / (Time.fixedDeltaTime * vehicleController.mass * 5f));
                    LogitechGSDK.LogiPlayFrontalCollisionForce(0, strength);
                }
            }
            else
            {
                vehicleController.steering.useDirectInput = false;
                _steeringWheelConnected = false;
            }
        }

        void SetVehicleInputs()
        {
            // Shift Up
            _shiftUpInput = GetButtonValue(shiftUpButton, _wheelInput) ||
                                                     GetButtonValue(altShiftUpButton, _wheelInput);
            
            // Shift Down
            _shiftDownInput = GetButtonValue(shiftDownButton, _wheelInput) ||
                                                     GetButtonValue(altShiftDownButton, _wheelInput);
            
            // H-shifter
            _shiftIntoInput = -999;
            if (GetButtonValue(shiftIntoReverseButton, _wheelInput))
            {
                _shiftIntoInput = -1;
            }
            else if (GetButtonValue(shiftIntoNeutralButton, _wheelInput))
            {
                _shiftIntoInput = 0;
            }
            else if (GetButtonValue(shiftInto1stButton, _wheelInput))
            {
                _shiftIntoInput = 1;
            }
            else if (GetButtonValue(shiftInto2ndButton, _wheelInput))
            {
                _shiftIntoInput = 2;
            }
            else if (GetButtonValue(shiftInto3rdButton, _wheelInput))
            {
                _shiftIntoInput = 3;
            }
            else if (GetButtonValue(shiftInto4thButton, _wheelInput))
            {
                _shiftIntoInput = 4;
            }
            else if (GetButtonValue(shiftInto5thButton, _wheelInput))
            {
                _shiftIntoInput = 5;
            }
            else if (GetButtonValue(shiftInto6thButton, _wheelInput))
            {
                _shiftIntoInput = 6;
            }
            else if (GetButtonValue(shiftInto7thButton, _wheelInput))
            {
                _shiftIntoInput = 7;
            }
            else if (GetButtonValue(shiftInto8thButton, _wheelInput))
            {
                _shiftIntoInput = 8;
            }
            else if (GetButtonValue(shiftInto9thButton, _wheelInput))
            {
                _shiftIntoInput = 9;
            }

            // Handbrake
            if (handbrakeAxis != Axis.None)
            {
                _handbrakeInput = GetAxisValue(handbrakeAxis, _wheelInput, true);
            }
            else
            {
                _handbrakeInput = GetButtonValue(handbrakeButton, _wheelInput) ? 1f : 0f;
            }
        }

        void GetWheelInputs()
        {
            _wheelInput = LogitechGSDK.LogiGetStateUnity(0);
                
            // Steer angle
            _steeringInput = GetAxisValue(steeringAxis, _wheelInput, false);
            if (flipSteeringInput) _steeringInput = -_steeringInput;
            float steerDelta = _steeringInput - _prevSteering;
            _steerVelocity = steerDelta / Time.deltaTime;
                
            // Throttle
            _throttleInput = GetAxisValue(throttleAxis, _wheelInput, true);
            if (flipThrottleInput) _throttleInput = -_throttleInput;

            // Brake
            _brakeInput = GetAxisValue(brakeAxis, _wheelInput, true);
            if (flipBrakeInput) _brakeInput = -_brakeInput;
            
            // Clutch
            _clutchInput = GetAxisValue(clutchAxis, _wheelInput, true);
            if (flipClutchInput) _clutchInput = -_clutchInput;
            
            // Buttons
            for (int i = 0; i < 128; i++)
            {
                buttonWasPressed[i] = buttonPressed[i];
                buttonPressed[i] = _wheelInput.rgbButtons[i] == 128;
                buttonDown[i] = !buttonWasPressed[i] && buttonPressed[i];
            }
        }
        
        bool GetButtonValue(int buttonIndex, LogitechGSDK.DIJOYSTATE2ENGINES wheelState)
        {
            if (buttonIndex < 0)
            {
                return false;
            }

            return buttonDown[buttonIndex];
        }

        float GetAxisValue(Axis axis, LogitechGSDK.DIJOYSTATE2ENGINES wheelState, bool zeroToOne)
        {
            float rawValue = 0;
            switch (axis)
            {
                case Axis.XPosition: rawValue = wheelState.lX;
                    break;
                case Axis.YPosition: rawValue = wheelState.lY;
                    break;
                case Axis.ZPosition: rawValue = wheelState.lZ;
                    break;
                case Axis.XRotatation: rawValue = wheelState.lRx;
                    break;
                case Axis.YRotation: rawValue = wheelState.lRy;
                    break;
                case Axis.ZRotation: rawValue = wheelState.lRz;
                    break;
                case Axis.rglSlider0: rawValue = wheelState.rglSlider[0];
                    break;
                case Axis.rglSlider1: rawValue = wheelState.rglSlider[1];
                    break;
                case Axis.rglSlider2: rawValue = wheelState.rglSlider[2];
                    break;
                case Axis.rglSlider3: rawValue = wheelState.rglSlider[3];
                    break;
                case Axis.rglASlider0: rawValue = wheelState.rglASlider[0];
                    break;
                case Axis.rglASlider1: rawValue = wheelState.rglASlider[1];
                    break;
                case Axis.rglASlider2: rawValue = wheelState.rglASlider[2];
                    break;
                case Axis.rglASlider3: rawValue = wheelState.rglASlider[3];
                    break;
                case Axis.rglFSlider0: rawValue = wheelState.rglFSlider[0];
                    break;
                case Axis.rglFSlider1: rawValue = wheelState.rglFSlider[1];
                    break;
                case Axis.rglFSlider2: rawValue = wheelState.rglFSlider[2];
                    break;
                case Axis.rglFSlider3: rawValue = wheelState.rglFSlider[3];
                    break;
                case Axis.rglVSlider0: rawValue = wheelState.rglVSlider[0];
                    break;
                case Axis.rglVSlider1: rawValue = wheelState.rglVSlider[1];
                    break;
                case Axis.rglVSlider2: rawValue = wheelState.rglVSlider[2];
                    break;
                case Axis.rglVSlider3: rawValue = wheelState.rglVSlider[3];
                    break;
                case Axis.lArx: rawValue = wheelState.lARx;
                    break;
                case Axis.lAry: rawValue = wheelState.lARy;
                    break;
                case Axis.lArz: rawValue = wheelState.lARz;
                    break;
                case Axis.lAx: rawValue = wheelState.lAX;
                    break;
                case Axis.lAy: rawValue = wheelState.lAY;
                    break;
                case Axis.lAz: rawValue = wheelState.lAZ;
                    break;
                case Axis.lFRx: rawValue = wheelState.lFRx;
                    break;
                case Axis.lFRy: rawValue = wheelState.lFRy;
                    break;
                case Axis.lFRz: rawValue = wheelState.lFRz;
                    break;
                case Axis.lFx: rawValue = wheelState.lFX;
                    break;
                case Axis.lFy: rawValue = wheelState.lFY;
                    break;
                case Axis.lFz: rawValue = wheelState.lFZ;
                    break;
                case Axis.lVRx: rawValue = wheelState.lVRx;
                    break;
                case Axis.lVRy: rawValue = wheelState.lVRy;
                    break;
                case Axis.lVRz: rawValue = wheelState.lVRz;
                    break;
                case Axis.lVx: rawValue = wheelState.lVX;
                    break;
                case Axis.lVy: rawValue = wheelState.lVY;
                    break;
                case Axis.lVz: rawValue = wheelState.lVZ;
                    break;
                default: rawValue = 0;
                    break;
            }

            float halfResolution = axisResolution / 2f;
            if (zeroToOne)
            {
                return (rawValue - halfResolution) / axisResolution;
            }
            else
            {
                return rawValue / halfResolution;
            }
        }
        
        void UpdateWheelSettings()
        {
            LogitechGSDK.LogiControllerPropertiesData currentProperties =
                new LogitechGSDK.LogiControllerPropertiesData();
            LogitechGSDK.LogiGetCurrentControllerProperties(0, ref currentProperties);
            currentProperties.forceEnable = true;
            currentProperties.combinePedals = false;
            currentProperties.gameSettingsEnabled = true;
            currentProperties.defaultSpringEnabled = false;
            currentProperties.defaultSpringGain = 100;
            currentProperties.springGain = 100;
            currentProperties.damperGain = 100;
            currentProperties.overallGain = (int)(maximumWheelForce * 100);
            currentProperties.wheelRange = wheelRotationRange;
            LogitechGSDK.LogiSetPreferredControllerProperties(currentProperties);
        }
        
        void AddForce(float force)
        {
            LogitechGSDK.LogiPlayConstantForce(0, (int)force);
        }

        void ResetForce()
        {
            LogitechGSDK.LogiStopConstantForce(0);
        }
        
        void OnApplicationQuit()
        {
            LogitechGSDK.LogiSteeringShutdown();
        }

        public override bool EngineStartStop()
        {
            return false;
        }

        public override float Clutch()
        {
            return _clutchInput;
        }

        public override bool ExtraLights()
        {
            return false;
        }

        public override bool HighBeamLights()
        {
            return false;
        }

        public override float Handbrake()
        {
            return _handbrakeInput;
        }

        public override bool HazardLights()
        {
            return false;
        }

        public override float Brakes()
        {
            return _brakeInput;
        }

        public override float Steering()
        {
            return _steeringInput;
        }

        public override bool Horn()
        {
            return false;
        }

        public override bool LeftBlinker()
        {
            return false;
        }

        public override bool LowBeamLights()
        {
            return false;
        }

        public override bool RightBlinker()
        {
            return false;
        }

        public override bool ShiftDown()
        {
            return _shiftDownInput;
        }

        public override int ShiftInto()
        {
            return _shiftIntoInput;
        }

        public override bool ShiftUp()
        {
            return _shiftUpInput;
        }

        public override bool TrailerAttachDetach()
        {
            return false;
        }

        public override float Throttle()
        {
            return _throttleInput;
        }

        public override bool FlipOver()
        {
            return false;
        }

        public override bool Boost()
        {
            return false;
        }

        public override bool CruiseControl()
        {
            return false;
        }
    }  
}

