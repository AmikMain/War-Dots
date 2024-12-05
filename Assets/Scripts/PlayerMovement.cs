using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private int defaultSpeed = 10;
    [SerializeField] private int increasedSpeed = 20;

    private int speed;

    private void Start() {
        speed = defaultSpeed;

        InputManager.Instance.onShiftPerformed += IncreaseSpeed;
        InputManager.Instance.onShiftCanceled += DecreaseSpeed;
    }

    private void Update() {
        Move();
    }

    private void Move()
    {
        transform.position += new Vector3(InputManager.Instance.movementVector.x, InputManager.Instance.movementVector.y, 0.0f)* speed * Time.deltaTime;
    }

    private void IncreaseSpeed()
    {
        speed = increasedSpeed;
    }

    private void DecreaseSpeed()
    {
        speed = defaultSpeed;
    }
}
