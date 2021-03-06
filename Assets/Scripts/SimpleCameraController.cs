#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using UnityEngine;

namespace UnityTemplateProjects
{
    public class SimpleCameraController : MonoBehaviour
    {
        bool is_zoomed_on_planet = false;
        bool is_zooming = false;

        public class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                x = t.position.x;
                y = t.position.y;
                z = t.position.z;
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;
            }

            public void ModifyTargetState(Vector3 newposition, Vector3 eulerangles)
            {

                pitch = eulerangles.x;
                yaw = eulerangles.y;
                roll = eulerangles.z;
                x = newposition.x;
                y = newposition.y;
                z = newposition.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
                
                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }

            public bool Equals(CameraState target)
            {
                if (Mathf.Abs(target.x-x)<0.001 &&
                    Mathf.Abs(target.y-y)<0.001 &&
                    Mathf.Abs(target.z-z)<0.001 &&
                    Mathf.Abs(target.pitch-pitch)<0.001 &&
                    Mathf.Abs(target.yaw-yaw)<0.001 &&
                    Mathf.Abs(target.roll-roll)<0.001)
                {
                    return true;
                }
                    return false;
                }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = new Vector3(x, y, z);
            }

            public void CopyCameraState(CameraState cameraState)
            {
                pitch = cameraState.pitch;
                yaw = cameraState.yaw;
                roll = cameraState.roll;
                x = cameraState.x;
                y = cameraState.y;
                z = cameraState.z;

            }
        }

        const float k_MouseSensitivityMultiplier = 0.01f;

        public CameraState m_TargetCameraState = new CameraState();
        CameraState m_StartOfUpdateCameraState = new CameraState();
        CameraState m_BeforeZoomCameraState = new CameraState();
        CameraState m_InterpolatingCameraState = new CameraState();

        [Header("Movement Settings")]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 3.5f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("Multiplier for the sensitivity of the rotation.")]
        public float mouseSensitivity = 60.0f;

        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY = false;

#if ENABLE_INPUT_SYSTEM
        InputAction movementAction;
        InputAction verticalMovementAction;
        InputAction lookAction;
        InputAction boostFactorAction;
        bool        mouseRightButtonPressed;

        void Start()
        {
            var map = new InputActionMap("Simple Camera Controller");

            lookAction = map.AddAction("look", binding: "<Mouse>/delta");
            movementAction = map.AddAction("move", binding: "<Gamepad>/leftStick");
            verticalMovementAction = map.AddAction("Vertical Movement");
            boostFactorAction = map.AddAction("Boost Factor", binding: "<Mouse>/scroll");

            lookAction.AddBinding("<Gamepad>/rightStick").WithProcessor("scaleVector2(x=15, y=15)");
            movementAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d")
                .With("Right", "<Keyboard>/rightArrow");
            verticalMovementAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/pageUp")
                .With("Down", "<Keyboard>/pageDown")
                .With("Up", "<Keyboard>/e")
                .With("Down", "<Keyboard>/q")
                .With("Up", "<Gamepad>/rightshoulder")
                .With("Down", "<Gamepad>/leftshoulder");
            boostFactorAction.AddBinding("<Gamepad>/Dpad").WithProcessor("scaleVector2(x=1, y=4)");

            movementAction.Enable();
            lookAction.Enable();
            verticalMovementAction.Enable();
            boostFactorAction.Enable();
        }
#endif

        void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
        }

        Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = Vector3.zero;
#if ENABLE_INPUT_SYSTEM
            var moveDelta = movementAction.ReadValue<Vector2>();
            direction.x = moveDelta.x;
            direction.z = moveDelta.y;
            direction.y = verticalMovementAction.ReadValue<Vector2>().y;
#else
            if (Input.GetKey(KeyCode.W))
            {
                direction += Vector3.forward;
            }
            if (Input.GetKey(KeyCode.S))
            {
                direction += Vector3.back;
            }
            if (Input.GetKey(KeyCode.A))
            {
                direction += Vector3.left;
            }
            if (Input.GetKey(KeyCode.D))
            {
                direction += Vector3.right;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                direction += Vector3.down;
            }
            if (Input.GetKey(KeyCode.E))
            {
                direction += Vector3.up;
            }
#endif
            return direction;
        }
        
        void Update()
        {
            // Exit Sample  
            m_StartOfUpdateCameraState.CopyCameraState(m_InterpolatingCameraState);

            if (IsEscapePressed())
            {
                Application.Quit();
				#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false; 
				#endif
            }

            // Hide and lock cursor when right mouse button pressed
            if (IsRightMouseButtonDown())
            {
                is_zoomed_on_planet = false;
                Cursor.lockState = CursorLockMode.Locked;
            }


            // Unlock and show cursor when right mouse button released
            if (IsRightMouseButtonUp())
            {
                is_zoomed_on_planet = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }



            // Rotation
            if (IsCameraRotationAllowed())
            {
                var mouseMovement = GetInputLookRotation() * k_MouseSensitivityMultiplier * mouseSensitivity;
                if (invertY)
                    mouseMovement.y = -mouseMovement.y;
                
                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }
            
            // Translation
            var translation = GetInputTranslationDirection() * Time.deltaTime;

            // Speed up movement when shift key held
            if (IsBoostPressed())
            {
                translation *= 10.0f;
            }
            
            // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
            boost += GetBoostFactor();
            translation *= Mathf.Pow(2.0f, boost);

            m_TargetCameraState.Translate(translation);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);

            var positionLerpPctZoom = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * 0.8f*Time.deltaTime);
            var rotationLerpPctZoom = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * 0.014f*Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.LeftShift) && (IsLeftMouseButtonDown()))
            {
                RaycastHit hit;
                Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(cameraRay, out hit) && hit.collider.name == "Sphere")
                {
                    if (!is_zoomed_on_planet) // If not currently zoomed in on planet
                    {
                        m_BeforeZoomCameraState.CopyCameraState(m_InterpolatingCameraState); //Store previous camera state before zooming
                        Vector3 newPosition = GetPositionAlongRay(cameraRay, hit, 6f);  
                        Vector3 newAngles = GetEulerAngles(cameraRay, hit);
                        m_TargetCameraState.ModifyTargetState(newPosition, newAngles);
                        is_zoomed_on_planet = true;
                        is_zooming = true;
                    }
                    else // If already zoomed in on planet
                    {
                        m_TargetCameraState.CopyCameraState(m_BeforeZoomCameraState);
                        is_zoomed_on_planet = false;
                        is_zooming = true;
                    }
                }
            }
            // Debug.Log("Is Zooming = " + is_zooming);
            // Debug.Log("Target camera state: x,y,z = " + m_TargetCameraState.x + m_TargetCameraState.y + m_TargetCameraState.z);
            if (is_zooming)
            {
                m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPctZoom, rotationLerpPctZoom);
            }
            else
            {
                m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);
            }

                m_InterpolatingCameraState.UpdateTransform(transform);

            if (translation != Vector3.zero) //Moving with arrow keys will break zoom
            {
                is_zooming= false;
                is_zoomed_on_planet = false;
            }
            if (m_StartOfUpdateCameraState.Equals(m_InterpolatingCameraState))
            {
                is_zooming= false;
            }
        }

        Vector3 GetPositionAlongRay(Ray cameraray, RaycastHit hit, float offsetFactor) // Generate position right before planet (given by offset)
        {
            Vector3 centerOfSphere = hit.transform.gameObject.GetComponent<Renderer>().bounds.center; // Find center of sphere
            Vector3 translation = new Vector3(centerOfSphere.x - cameraray.origin.x,
                                              centerOfSphere.y - cameraray.origin.y,
                                              centerOfSphere.z - cameraray.origin.z);
            Vector3 offset = offsetFactor*cameraray.direction;
            return translation+cameraray.origin-offset;
        }

        Vector3 GetEulerAngles(Ray cameraray, RaycastHit hit)
        {
            Vector3 centerOfSphere = hit.transform.gameObject.GetComponent<Renderer>().bounds.center;                
            Quaternion q = Quaternion.FromToRotation(Vector3.forward, centerOfSphere - cameraray.origin);
            return q.eulerAngles;
        }

        float GetBoostFactor()
        {
#if ENABLE_INPUT_SYSTEM
            return boostFactorAction.ReadValue<Vector2>().y * 0.01f;
#else
            return Input.mouseScrollDelta.y * 0.01f;
#endif
        }

        Vector2 GetInputLookRotation()
        {
            // try to compensate the diff between the two input systems by multiplying with empirical values
#if ENABLE_INPUT_SYSTEM
            var delta = lookAction.ReadValue<Vector2>();
            delta *= 0.5f; // Account for scaling applied directly in Windows code by old input system.
            delta *= 0.1f; // Account for sensitivity setting on old Mouse X and Y axes.
            return delta;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
        }

        bool IsBoostPressed()
        {
#if ENABLE_INPUT_SYSTEM
            bool boost = Keyboard.current != null ? Keyboard.current.leftShiftKey.isPressed : false; 
            boost |= Gamepad.current != null ? Gamepad.current.xButton.isPressed : false;
            return boost;
#else
            return Input.GetKey(KeyCode.LeftShift);
#endif

        }

        bool IsEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null ? Keyboard.current.escapeKey.isPressed : false; 
#else
            return Input.GetKey(KeyCode.Escape);
#endif
        }

        bool IsCameraRotationAllowed()
        {
#if ENABLE_INPUT_SYSTEM
            bool canRotate = Mouse.current != null ? Mouse.current.rightButton.isPressed : false;
            canRotate |= Gamepad.current != null ? Gamepad.current.rightStick.ReadValue().magnitude > 0 : false;
            return canRotate;
#else
            return Input.GetMouseButton(1);
#endif
        }

        bool IsRightMouseButtonDown()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.rightButton.isPressed : false;
#else
            return Input.GetMouseButtonDown(1);
#endif
        }

        bool IsRightMouseButtonUp()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? !Mouse.current.rightButton.isPressed : false;
#else
            return Input.GetMouseButtonUp(1);
#endif
        }

        bool IsLeftMouseButtonDown()
        {
#if ENABLE_INPUT_SYSTEM
                return Mouse.current != null ? Mouse.current.leftButton.isPressed : false;
#else
                return Input.GetMouseButtonDown(0);
#endif
        }
    
        bool IsLeftMouseButtonUp()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.leftButton.isPressed : false;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

    }

}
