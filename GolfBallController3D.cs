using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(LineRenderer))]
public class GolfBallController3D : MonoBehaviour
{
    [Header("Aim & Launch")]
    [SerializeField] private float maxDragDistance = 4.0f;
    [SerializeField] private float launchForceMultiplier = 8.5f;
    [SerializeField] private float stopSpeedThreshold = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Hole & Water Hazards")]
    [SerializeField] private Transform holeCenter;
    [SerializeField] private float holeCaptureRadius = 0.45f;
    [SerializeField] private float maxCaptureSpeed = 6.0f;
    [SerializeField] private string waterTag = "Water";

    public event Action<int> OnStrokeChanged;
    public event Action OnHoleSunk;

    private Rigidbody _rb;
    private LineRenderer _aimLine;
    private Camera _mainCam;

    private Vector3 _startTeePosition;
    private Vector3 _lastSafePosition;
    private bool _isDragging;
    private bool _isBallSunk;
    private int _strokes;

    public int Strokes => _strokes;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _aimLine = GetComponent<LineRenderer>();
        _mainCam = Camera.main;

        _startTeePosition = transform.position;
        _lastSafePosition = transform.position;

        _aimLine.positionCount = 2;
        _aimLine.enabled = false;
    }

    private void Update()
    {
        if (_isBallSunk) return;

        bool isMoving = _rb.linearVelocity.magnitude > stopSpeedThreshold;

        if (!isMoving)
        {
            if (_rb.linearVelocity.magnitude > 0f)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _lastSafePosition = transform.position;
            }

            HandleAimInput();
        }
        else if (_isDragging)
        {
            CancelAim();
        }

        CheckHoleCapture();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(waterTag) && !_isBallSunk)
        {
            RespawnAtLastSafePosition();
        }
    }

    public void ResetBall(Vector3 newTeePosition)
    {
        _startTeePosition = newTeePosition;
        _lastSafePosition = newTeePosition;
        _strokes = 0;
        _isBallSunk = false;
        _isDragging = false;
        _aimLine.enabled = false;

        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = newTeePosition;

        OnStrokeChanged?.Invoke(_strokes);
    }

    public void SetHoleCenter(Transform newHoleCenter)
    {
        holeCenter = newHoleCenter;
    }

    private void RespawnAtLastSafePosition()
    {
        CancelAim();
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position = _lastSafePosition;
        _strokes++;
        OnStrokeChanged?.Invoke(_strokes);
    }

    private void HandleAimInput()
    {
        if (Input.GetMouseButtonDown(0) && TryGetGroundPoint(out Vector3 downHit))
        {
            float distToBall = Vector3.Distance(
                new Vector3(downHit.x, transform.position.y, downHit.z),
                transform.position
            );

            if (distToBall <= 1.25f)
            {
                _isDragging = true;
                _aimLine.enabled = true;
            }
        }
        else if (_isDragging && Input.GetMouseButton(0) && TryGetGroundPoint(out Vector3 dragHit))
        {
            Vector3 flatBall = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Vector3 flatCursor = new Vector3(dragHit.x, transform.position.y, dragHit.z);
            Vector3 pullVector = flatCursor - flatBall;

            if (pullVector.magnitude > maxDragDistance)
            {
                pullVector = pullVector.normalized * maxDragDistance;
            }

            Vector3 shotDirection = -pullVector.normalized;
            float previewLength = pullVector.magnitude * 1.25f;

            _aimLine.SetPosition(0, flatBall);
            _aimLine.SetPosition(1, flatBall + shotDirection * previewLength);
        }
        else if (_isDragging && Input.GetMouseButtonUp(0) && TryGetGroundPoint(out Vector3 upHit))
        {
            Vector3 flatBall = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            Vector3 flatCursor = new Vector3(upHit.x, transform.position.y, upHit.z);
            Vector3 pullVector = flatCursor - flatBall;

            if (pullVector.magnitude > maxDragDistance)
            {
                pullVector = pullVector.normalized * maxDragDistance;
            }

            CancelAim();

            if (pullVector.magnitude > 0.2f)
            {
                _lastSafePosition = transform.position;
                Vector3 launchDirection = -pullVector.normalized;
                float strength = Mathf.Clamp(pullVector.magnitude / maxDragDistance, 0f, 1f);

                _rb.AddForce(launchDirection * (strength * launchForceMultiplier), ForceMode.Impulse);
                _strokes++;
                OnStrokeChanged?.Invoke(_strokes);
            }
        }
    }

    private void CancelAim()
    {
        _isDragging = false;
        _aimLine.enabled = false;
    }

    private void CheckHoleCapture()
    {
        if (holeCenter == null) return;

        Vector2 ballXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 holeXZ = new Vector2(holeCenter.position.x, holeCenter.z);
        float horizontalDistance = Vector2.Distance(ballXZ, holeXZ);

        if (horizontalDistance <= holeCaptureRadius && _rb.linearVelocity.magnitude <= maxCaptureSpeed)
        {
            _isBallSunk = true;
            _aimLine.enabled = false;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
            transform.position = new Vector3(holeCenter.position.x, transform.position.y - 0.15f, holeCenter.position.z);

            OnHoleSunk?.Invoke();
        }
    }

    private bool TryGetGroundPoint(out Vector3 point)
    {
        if (_mainCam == null)
        {
            _mainCam = Camera.main;
        }

        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            point = hit.point;
            return true;
        }

        point = Vector3.zero;
        return false;
    }
}
