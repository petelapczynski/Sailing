using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject _Player;
    public Vector3 _PositionOffset;
    public bool _FollowRotation;

    private void Update()
    {
        Vector3 position = transform.position;
        position.x = _Player.transform.position.x + _PositionOffset.x;
        position.z = _Player.transform.position.z + _PositionOffset.z;
        position.y += _PositionOffset.y;
        transform.position = position;

        if (_FollowRotation) {
            transform.rotation = Quaternion.Euler(0, _Player.transform.rotation.eulerAngles.y, 0);
        }
    }
}