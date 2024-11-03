using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Vector3 offset = new Vector3(0f,0f,-10f);
    private float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private Transform target;
    private float currentPosX;

    void Update()
    {
        if(target==null) return;
        Vector3 targetPosition = target.position+offset;
        transform.position = Vector3.SmoothDamp(transform.position,targetPosition,ref velocity, smoothTime);
    }

    public void MoveToNewRoom(Transform _newRoom)
    {
        print("here");
        currentPosX = _newRoom.position.x;
    }
}
