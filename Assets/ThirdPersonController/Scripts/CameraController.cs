using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Yoma.ThirdPerson
{
    public class CameraController : MonoBehaviour
    {
        private CinemachineFreeLook _cameraHolder;

        [Header("REFERENCES")]
        [SerializeField] private float _rotateSpeed;
        [SerializeField] private float _zoomSpeed;
        [SerializeField] private bool _isRotateAllDirection;

        float cameraDistance;

        void Start()
        {
            _cameraHolder = GetComponent<CinemachineFreeLook>();
        }

        void Update()
        {
            // HORIZONTAL ROTATION
            _cameraHolder.m_XAxis.m_MaxSpeed = Input.GetMouseButton(1)? _rotateSpeed : 0f;
            if(_isRotateAllDirection) _cameraHolder.m_YAxis.m_MaxSpeed = Input.GetMouseButton(1) ? _zoomSpeed : 0f;

            // VERTICAL ROTATION (ZOOM)
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                cameraDistance = Input.GetAxis("Mouse ScrollWheel") * _zoomSpeed;
                _cameraHolder.m_YAxis.Value -= cameraDistance;
            }
        }
    }
}
