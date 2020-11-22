using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;
    public WorldManager voxelWorld;
    private Controls _controls;
    
    public float walkSpeed = 10;
    public float gravity = 9.81f;
    public float playerWidth = 0.35f;

    public GameObject cursorBlock;
    
    private bool _isGrounded = false;
    private Vector3 _moveDirection;
    private Vector3Int currentChunk;
    
    private void Awake()
    {
        _controls = new Controls();
        Application.targetFrameRate = 60;
        //_controls.Player.Jump.performed += context => Jump(context);
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    /// <summary>
    /// Move the player in the direction of the camera
    /// </summary>
    /// <param name="controllerValue">A Vector2 value of controller</param>
    private void Move(Vector2 controllerValue)
    {
        var worldDirection = transform.rotation * new Vector3(controllerValue.x, 0, controllerValue.y);
        _moveDirection.x = worldDirection.x;
        _moveDirection.z = worldDirection.z;
    }

    private void Jump()
    {
        _moveDirection.y = 2;
        _isGrounded = false;
    }
    
    /// <summary>
    /// Move the player in the direction of the camera
    /// </summary>
    /// <param name="controllerValue">A Vector2 value of controller</param>
    private void Look(Vector2 controllerValue)
    {
        const float horizontalSensitivity = 30.0f;
        const float verticalSensitivity = 30.0f;
 
        var rotationX = horizontalSensitivity * controllerValue.x * Time.deltaTime;
        var rotationY = verticalSensitivity * controllerValue.y * Time.deltaTime;
 
        transform.Rotate(0,rotationX,0);
        playerCamera.transform.Rotate(-rotationY, 0, 0);
    }

    // Update is called once per frame
    private void Update()
    {
        Move(_controls.Player.Move.ReadValue<Vector2>());
        Look(_controls.Player.Look.ReadValue<Vector2>());

        var pos = transform.position;
        Vector3[] positions = new[]
        {
            new Vector3(pos.x - playerWidth, pos.y, pos.z - playerWidth),
            new Vector3(pos.x - playerWidth, pos.y, pos.z + playerWidth),
            new Vector3(pos.x + playerWidth, pos.y, pos.z - playerWidth),
            new Vector3(pos.x - playerWidth, pos.y, pos.z - playerWidth),
        };

        
        _isGrounded = false;
        foreach (var cornerPos in positions)
        {
            var cornerPoint = voxelWorld.GetVoxelPoint(cornerPos);
            var voxelCornerPoint = Vector3Int.FloorToInt(cornerPoint);
            if (voxelWorld.IsSolidPoint(voxelCornerPoint)) _isGrounded = true;
        }
        
        var point = voxelWorld.GetVoxelPoint(pos);
        var voxelPoint = Vector3Int.FloorToInt(point);
        
        
        if (!_isGrounded)
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }
        else if (_moveDirection.y < 0)
        {
            _moveDirection.y = 0;
            var position = transform.position;
            position.y = voxelWorld.GetWorldPoint(voxelPoint).y + 0.99f;
            transform.position = position;
        }

        if (_controls.Player.Jump.triggered && _isGrounded)
        {
            Jump();
        }

        if (_moveDirection.magnitude > 0)
        {
            for (var i = _isGrounded ? 1 : 0; i < 3; i++)
            {
                var positionToCheck = transform.position;
                positionToCheck.y += i;
                foreach (var dimension in new []{0,2})
                {
                    foreach (var direction in new[] {-1, 1})
                    {
                        var nextPos = positionToCheck;
                        nextPos[dimension] += direction * playerWidth * 1.1f;
                        var nextVoxelPoint = Vector3Int.FloorToInt(nextPos);
                        if (voxelWorld.IsSolidPoint(nextVoxelPoint) && _moveDirection[dimension] * direction > 0)
                            _moveDirection[dimension] = 0;
                    }
                }
            }

            //TODO: add a ceiling check after building is finished
            
            transform.position += _moveDirection * (Time.deltaTime * walkSpeed);
        }

        if (_controls.Player.Escape.triggered)
        {
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        var chunk = voxelWorld.GetVoxelPointChunk(voxelPoint);
        if (chunk != currentChunk)
        {
            currentChunk = chunk;
            voxelWorld.AddChunksAround(chunk, 3);
        }
    }
}
