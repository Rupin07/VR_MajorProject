using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(Rigidbody))]
public class VRFlightController : MonoBehaviour
{
    [Header("Input Actions (assign InputActionReference from Default XRI actions)")]
    public InputActionReference rightHandRotationAction; // XRI RightHand/Rotation
    public InputActionReference leftHandRotationAction;  // XRI LeftHand/Rotation (optional)
    public InputActionReference rightTriggerAction;      // XRI RightHand/Trigger (throttle)
    public InputActionReference leftTriggerAction;       // XRI LeftHand/Trigger  (brake)
    public InputActionReference primary2DAxisAction;     // XRI LeftHand/Primary2DAxis (optional yaw)

    [Header("Flight tuning")]
    public float maxThrust = 20000f;
    public float maxSpeed = 200f;            // clamp speed (m/s)
    public float pitchTorque = 5000f;        // pitch control strength
    public float rollTorque = 4000f;         // roll control strength
    public float yawTorque = 1500f;          // yaw control strength (from joystick)
    public float liftCoefficient = 0.5f;     // lift = coef * forwardSpeed^2
    public float dragCoefficient = 0.02f;    // simple aerodynamic drag
    public float angularDamping = 0.5f;      // extra angular damping
    public bool useHandRotationForControls = true; // reads controller rotation for pitch/roll

    Rigidbody rb;

    // internal state
    float throttle = 0f; // 0..1
    float brake = 0f;    // 0..1

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        // tighten interpolation for smoother motion in VR
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnEnable()
    {
        rightTriggerAction?.action?.Enable();
        leftTriggerAction?.action?.Enable();
        rightHandRotationAction?.action?.Enable();
        leftHandRotationAction?.action?.Enable();
        primary2DAxisAction?.action?.Enable();
    }

    void OnDisable()
    {
        rightTriggerAction?.action?.Disable();
        leftTriggerAction?.action?.Disable();
        rightHandRotationAction?.action?.Disable();
        leftHandRotationAction?.action?.Disable();
        primary2DAxisAction?.action?.Disable();
    }

    void FixedUpdate()
    {
        ReadInputs();
        ApplyPhysics();
    }

    void ReadInputs()
    {
        // Throttle from right trigger (0..1)
        if (rightTriggerAction != null && rightTriggerAction.action != null)
        {
            throttle = rightTriggerAction.action.ReadValue<float>();
        }

        // Brake from left trigger (0..1)
        if (leftTriggerAction != null && leftTriggerAction.action != null)
        {
            brake = leftTriggerAction.action.ReadValue<float>();
        }

        // Optionally clamp
        throttle = Mathf.Clamp01(throttle);
        brake = Mathf.Clamp01(brake);
    }

    void ApplyPhysics()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
        float forwardSpeed = Mathf.Max(0f, localVel.z); // forward component in local space

        // 1) Thrust forward
        float currentThrust = Mathf.Lerp(0f, maxThrust, throttle);
        Vector3 thrustForce = transform.forward * currentThrust * Time.fixedDeltaTime;
        rb.AddForce(thrustForce, ForceMode.Force);

        // Brake reduces forward velocity directly
        if (brake > 0.01f)
        {
            Vector3 brakeForce = -rb.velocity.normalized * brake * maxThrust * 0.5f * Time.fixedDeltaTime;
            rb.AddForce(brakeForce, ForceMode.Force);
        }

        // 2) Simple lift (lift ~ v^2)
        float lift = liftCoefficient * forwardSpeed * forwardSpeed;
        Vector3 liftForce = transform.up * lift * Time.fixedDeltaTime;
        rb.AddForce(liftForce, ForceMode.Force);

        // 3) Aerodynamic drag proportional to v^2
        Vector3 drag = -rb.velocity * rb.velocity.magnitude * dragCoefficient * Time.fixedDeltaTime;
        rb.AddForce(drag, ForceMode.Force);

        // 4) Rotation: pitch & roll from hand/controller rotations (or joystick fallback)
        Vector3 pitchRollInput = Vector3.zero; // x = pitch, z = roll

        if (useHandRotationForControls && rightHandRotationAction != null && rightHandRotationAction.action != null)
        {
            // Read right hand rotation; interpret local Euler angles for control
            Quaternion q = rightHandRotationAction.action.ReadValue<Quaternion>();
            Vector3 euler = q.eulerAngles;

            // Convert euler to -180..180
            float rx = NormalizeAngle(euler.x);
            float rz = NormalizeAngle(euler.z);

            // Use small deadzone and scale
            float pitchInput = Mathf.Clamp(rx / 45f, -1f, 1f); // tilt forward/backwards
            float rollInput = Mathf.Clamp(rz / 45f, -1f, 1f);  // tilt left/right

            pitchRollInput = new Vector3(pitchInput, 0f, rollInput);
        }
        else
        {
            // fallback: if left hand rotation present, use that
            if (leftHandRotationAction != null && leftHandRotationAction.action != null)
            {
                Quaternion q = leftHandRotationAction.action.ReadValue<Quaternion>();
                Vector3 euler = q.eulerAngles;
                float rx = NormalizeAngle(euler.x);
                float rz = NormalizeAngle(euler.z);
                pitchRollInput = new Vector3(Mathf.Clamp(rx / 45f, -1f, 1f), 0f, Mathf.Clamp(rz / 45f, -1f, 1f));
            }
        }

        // 5) Yaw from joystick/primary2D axis if available
        float yawInput = 0f;
        if (primary2DAxisAction != null && primary2DAxisAction.action != null)
        {
            Vector2 axis = primary2DAxisAction.action.ReadValue<Vector2>();
            yawInput = axis.x; // left/right on thumbstick
        }

        // Apply torques based on inputs, scaled by forwardSpeed to make control more effective at speed
        float speedFactor = Mathf.Clamp01(forwardSpeed / (maxSpeed * 0.5f));
        Vector3 torque = Vector3.zero;

        // Pitch: local X axis
        torque += transform.right * (pitchRollInput.x * pitchTorque * speedFactor * Time.fixedDeltaTime);
        // Roll: local Z axis (negative because of coordinate differences)
        torque += transform.forward * (-pitchRollInput.z * rollTorque * speedFactor * Time.fixedDeltaTime);
        // Yaw: local Y axis
        torque += transform.up * (yawInput * yawTorque * Time.fixedDeltaTime);

        rb.AddTorque(torque, ForceMode.Force);

        // Optional: small angular damping for stability
        rb.angularVelocity *= (1f - angularDamping * Time.fixedDeltaTime);

        // Clamp max speed
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    static float NormalizeAngle(float a)
    {
        // convert 0..360 to -180..180
        if (a > 180f) a -= 360f;
        return a;
    }

    // Debug helper for UI
    public float GetThrottle() => throttle;
    public float GetForwardSpeed() => transform.InverseTransformDirection(rb.velocity).z;
}
