using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CinemachineVirtualCameraUtility : MonoBehaviour
{
    CinemachineVirtualCamera virtualCamera;

    [SerializeField] float tweenDuration;

    private void Start()
    {
        GetVirtualCamera();
    }

    void GetVirtualCamera()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void SetFollow(GameObject target) => virtualCamera.Follow = target.transform;
    public void TweenDistance(float value)
    {
        try
        {
            if (virtualCamera == null) GetVirtualCamera();

            var cinemachineComponent = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            DOTween.To(
                () => cinemachineComponent.CameraDistance,
                x => cinemachineComponent.CameraDistance = x,
                value, tweenDuration);
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
    }

}
