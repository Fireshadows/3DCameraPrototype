using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform m_focus = default;

    [SerializeField, Range(2f, 20f)]
    float m_distance = 10f;

    [SerializeField, Min(0f)]
    float m_focusRadius = 1f;
    Vector3 m_focusPoint, m_previousFocusPoint;
    [SerializeField, Range(0f, 1f)]
    float m_focusCentering = 0.5f;

    [SerializeField, Range(0f, 360f)]
    float m_rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)]
    float m_minVerticalAngle = -30f, m_maxVerticalAngle = 60f;
    Vector2 m_orbitAngles = new Vector2(45f, 0f);

    [SerializeField, Min(0f)]
    float m_alignDelay = 5f;
    float m_lastManualRotationTime;
    [SerializeField, Range(0f, 90f)]
    float m_alignSmoothRange = 45f;

    Camera m_regularCamera;
    Vector3 m_cameraHalfExtends;

    [SerializeField]
    LayerMask m_obstructionMask = -1;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        m_focusPoint = m_focus.position;

        transform.LookAt(m_focusPoint);
        m_orbitAngles = transform.rotation.eulerAngles;

        m_regularCamera = GetComponent<Camera>();

        Vector3 m_halfExtends;
        m_halfExtends.y = m_regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Rad2Deg * m_regularCamera.fieldOfView) * 1.3f;
        m_halfExtends.x = m_halfExtends.y * m_regularCamera.aspect ;
        m_halfExtends.z = 0f;
        m_cameraHalfExtends = m_halfExtends;
    }

    private void OnValidate()
    {
        if (m_maxVerticalAngle < m_minVerticalAngle)
            m_maxVerticalAngle = m_minVerticalAngle;
    }

    private void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion m_lookRotation;

        m_distance += -10 * Input.GetAxis("Scrollwheel");
        if (m_distance < 2)
            m_distance = 2;
        else if (m_distance > 20)
            m_distance = 20;


        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            m_lookRotation = Quaternion.Euler(m_orbitAngles);
        }
        else
            m_lookRotation = transform.localRotation;

        Vector3 m_lookDirection = m_lookRotation * Vector3.forward;
        Vector3 m_lookPosition = m_focusPoint - m_lookDirection * m_distance;

        Vector3 m_rectOffset = m_lookDirection * m_regularCamera.nearClipPlane;
        Vector3 m_rectPosition = m_lookPosition + m_rectOffset;
        Vector3 m_castFrom = m_focus.position;
        Vector3 m_castLine = m_rectPosition - m_castFrom;
        float m_castDistance = m_castLine.magnitude;
        Vector3 m_castDirection = m_castLine / m_castDistance;

        if (Physics.BoxCast(m_castFrom, m_cameraHalfExtends, m_castDirection, out RaycastHit m_hit, m_lookRotation, m_castDistance - m_regularCamera.nearClipPlane, m_obstructionMask))
        {
            m_rectPosition = m_castFrom + m_castDirection * m_hit.distance;
            m_lookPosition = m_rectPosition - m_rectOffset;
        }

        transform.SetPositionAndRotation(m_lookPosition, m_lookRotation);
    }

    bool ManualRotation()
    {
        Vector2 m_input = new Vector2(Input.GetAxis("Vertical Camera"), Input.GetAxis("Horizontal Camera"));
        const float e = 0.001f;
        if (m_input.x < -e || m_input.x > e || m_input.y < -e || m_input.y > e)
        {
            m_orbitAngles += m_rotationSpeed * Time.unscaledDeltaTime * m_input;
            m_lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    bool AutomaticRotation()
    {
        if (Time.unscaledTime - m_lastManualRotationTime < m_alignDelay)
            return false;

        Vector2 m_movement = new Vector2(m_focusPoint.x - m_previousFocusPoint.x, m_focusPoint.z - m_previousFocusPoint.z);
        float m_movementDeltaSqr = m_movement.sqrMagnitude;

        if (m_movementDeltaSqr < 0.000001f)
            return false;

        float m_headingAngle = GetAngle(m_movement / Mathf.Sqrt(m_movementDeltaSqr));
        float m_deltaAbs = Mathf.Abs(Mathf.DeltaAngle(m_orbitAngles.y, m_headingAngle));
        float m_rotationChange = m_rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, m_movementDeltaSqr);
        if (m_deltaAbs < m_alignSmoothRange)
            m_rotationChange *= m_deltaAbs / m_alignSmoothRange;
        else if (180f - m_deltaAbs < m_alignSmoothRange)
            m_rotationChange *= (180f - m_deltaAbs) / m_alignSmoothRange;
        m_orbitAngles.y = Mathf.MoveTowardsAngle(m_orbitAngles.y, m_headingAngle, m_rotationChange);

        return true;
    }

    static float GetAngle(Vector2 p_direction)
    {
        float m_angle = Mathf.Acos(p_direction.y) * Mathf.Rad2Deg;
        return p_direction.x < 0f ? 360f - m_angle : m_angle;
    }

    void ConstrainAngles()
    {
        m_orbitAngles.x = Mathf.Clamp(m_orbitAngles.x, m_minVerticalAngle, m_maxVerticalAngle);
        if (m_orbitAngles.y < 0f)
            m_orbitAngles.y += 360f;
        else if (m_orbitAngles.y >= 360f)
            m_orbitAngles.y -= 360f;
    }

    void UpdateFocusPoint()
    {
        m_previousFocusPoint = m_focusPoint;
        Vector3 m_targetPoint = m_focus.position;
        if (m_focusRadius > 0f)
        {
            float m_distance = Vector3.Distance(m_targetPoint, m_focusPoint);
            if (m_distance > m_focusRadius)
                m_focusPoint = Vector3.Lerp(m_targetPoint, m_focusPoint, m_focusRadius/ m_distance);
            if (m_distance > .01f && m_focusCentering > 0f)
                m_focusPoint = Vector3.Lerp(m_targetPoint, m_focusPoint, Mathf.Pow(1f- m_focusCentering, Time.unscaledDeltaTime));
        }
        else
            m_focusPoint = m_targetPoint;
    }
}
