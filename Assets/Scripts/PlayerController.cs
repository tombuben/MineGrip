using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Camera playerCamera;
    public WorldManager voxelWorld;
    private Controls _controls;
    
    public float walkSpeed = 10;
    public float gravity = 9.81f;
    public float playerWidth = 0.35f;

    public sbyte blockTypeSelected = 1;
    public Image blockTypeSelectedIcon;
    private Sprite blockTypeSprite;
    public GameObject cursorPrefab;

    public float maxMineBuildDistance = 5.0f;
    private GameObject _cursor;
    private Transform _cursorScale;
    
    private bool _isGrounded = false;
    private Vector3 _moveDirection;
    private Vector3 _voxelSpacePosition;
    private Vector3Int _voxelGridPosition;
    private Vector3Int _currentChunk;

    private bool _blockSelected;
    private Vector3Int _blockPosToBreak;
    private Vector3Int _blockPosToBuild;

    private Vector3Int _currentlyBreaking;
    private float _currentlyBreakingDurability;
    private float _currentlyBreakingDamage;
    

    private void Awake()
    {
        _controls = new Controls();

        _cursor = Instantiate(cursorPrefab);
        _cursorScale = _cursor.transform.GetChild(0);
        _cursor.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        
        ChangeBuildIcon();
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
    /// Checks if the current chunks the player is in is loaded
    /// </summary>
    /// <returns>Whether or not the chunk was loaded</returns>
    private bool CurrentChunkLoaded()
    {
        return voxelWorld.chunks.ContainsKey(_currentChunk);
    }
    
    // Update is called once per frame
    private void Update()
    {
        
        CheckPosition();
        
        if (!CurrentChunkLoaded()) return;
        
        HandleMovement();

        UpdateCursor();

        HandleBuildBreak();
        
        if (_controls.Player.Escape.triggered)
        {
#if UNITY_STANDALONE
            Application.Quit();
#endif
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = true;
            //UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        
        var chunk = voxelWorld.GetVoxelPointChunk(_voxelGridPosition);
        if (chunk != _currentChunk)
        {
            _currentChunk = chunk;
            voxelWorld.AddChunksAround(chunk);
            voxelWorld.RemoveChunksAround(chunk);
        }
    }

    /// <summary>
    /// Update the positions that are checked
    /// </summary>
    private void CheckPosition()
    {
        _isGrounded = CheckGivenPosition(transform.position);
        
        _voxelSpacePosition = voxelWorld.GetVoxelPoint(transform.position);
        _voxelGridPosition = Vector3Int.FloorToInt(_voxelSpacePosition);
    }

    private bool CheckGivenPosition(Vector3 pos, Vector3 forward = default)
    {
        Vector3 forwardpoint = new Vector3();
        if (forward != Vector3.zero)
            forwardpoint = pos + forward.normalized * playerWidth;
        var positions = new Vector3[]
        {
            new Vector3(pos.x - playerWidth, pos.y, pos.z - playerWidth),
            new Vector3(pos.x - playerWidth, pos.y, pos.z + playerWidth),
            new Vector3(pos.x + playerWidth, pos.y, pos.z - playerWidth),
            new Vector3(pos.x + playerWidth, pos.y, pos.z + playerWidth),
        };

        foreach (var cornerPos in positions)
        {
            if (forward != Vector3.zero && Vector3.Distance(forwardpoint, cornerPos) > playerWidth * 1.5f) continue;
            var cornerPoint = voxelWorld.GetVoxelPoint(cornerPos);
            var voxelCornerPoint = Vector3Int.FloorToInt(cornerPoint);
            if (voxelWorld.IsSolidPoint(voxelCornerPoint)) return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the player movement direction
    /// </summary>
    /// <param name="controllerValue">A Vector2 value of controller</param>
    private void Move(Vector2 controllerValue)
    {
        var worldDirection = transform.rotation * new Vector3(controllerValue.x, 0, controllerValue.y);
        _moveDirection.x = worldDirection.x;
        _moveDirection.z = worldDirection.z;
    }

    /// <summary>
    /// Sets the player's jump direction
    /// </summary>
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
        
        var transform1 = playerCamera.transform;
        var yaw = transform1.eulerAngles.x;
        yaw -= rotationY;
        if ((yaw + 180)%360 < 180-85)
        {
            yaw = -85;
        }
        if ((yaw + 180)%360 > 180+85)
        {
            yaw = 85;
        }
        transform1.eulerAngles = new Vector3(yaw, transform1.eulerAngles.y, 0);
    }


    /// <summary>
    /// Hanndle the different inputs
    /// </summary>
    private void HandleMovement()
    {
        Move(_controls.Player.Move.ReadValue<Vector2>());
        Look(_controls.Player.Look.ReadValue<Vector2>());
        
        if (!_isGrounded)
        {
            _moveDirection.y -= gravity * Time.deltaTime;
        }
        else if (_moveDirection.y < 0)
        {
            _moveDirection.y = 0;
            var position = transform.position;
            position.y = voxelWorld.GetWorldPoint(_voxelGridPosition).y + 0.99f;
            transform.position = position;
        }
        
        if (_controls.Player.Jump.triggered && _isGrounded)
        {
            Jump();
        }

        if (_moveDirection.magnitude > 0)
        {
            // Check for wall movement
            for (var i = _isGrounded ? 1 : 0; i < 3; i++)
            {
                var positionToCheck = transform.position;
                positionToCheck.y += i - 0.1f;
                foreach (var dimension in new []{0,2})
                {
                    var nextPos = positionToCheck;
                    nextPos[dimension] += _moveDirection[dimension] * Time.deltaTime * walkSpeed;
                    if (CheckGivenPosition(nextPos, _moveDirection))
                        _moveDirection[dimension] = 0;
                    
                }
            }
            
            /// Check if head doesn't bump into an object
            if (_moveDirection.y > 0)
            {
                var headPos = transform.position + Vector3.up * 0.5f + _moveDirection;
                if (CheckGivenPosition(headPos))
                    _moveDirection.y = 0;
            }

            transform.position += _moveDirection * (Time.deltaTime * walkSpeed);
        }
    }
    
    /// <summary>
    /// Updates the position of the cursor in scene - sends a ray through the scene using the 3D-DDA algorithm
    /// </summary>
    private void UpdateCursor()
    {
        
        var cameraTransform = playerCamera.transform;
        var rayStart = cameraTransform.position;
        var rayDirection = cameraTransform.forward;
        var p0 = rayStart;
        var dir = rayDirection;
        
        // t_0 - when using the parametric ray definition,
        // the value of t for intersecting the nearest axis plane
        var t_0 = new Vector3(float.NaN, float.NaN, float.NaN);
        
        // t_0 - when using the parametric ray definition,
        // the value of t for intersecting the second nearest axis plane
        var t_1 = new Vector3(float.NaN, float.NaN, float.NaN);
        
        // dt - the difference betewen t_1 and t_0 - a smallest step to
        // move between two integer ray intersections of axis planes
        var dt = new Vector3(float.NaN, float.NaN, float.NaN);
        
        //calculating dt for each dimension
        for (var dim = 0; dim < 3; dim++)
        {
            if (rayDirection[dim] == 0.0f) continue;
            
            // We're looking for first two integer intersections with the x plane t_x in the direction of the ray
            var p_first = dir[dim] > 0 ? Mathf.Ceil(p0[dim]) : Mathf.Floor(p0[dim]);
            var p_next = dir[dim] > 0 ? p_first + 1 : p_first - 1;

            // Parametric equation of line:
            // x = x0 + dir_x * t   ==>   t = (x - x0) / dir_x
            t_0[dim] = (p_first - p0[dim]) / dir[dim];
            t_1[dim] = (p_next - p0[dim]) / dir[dim];

            // how much must T move to get to the next integer value
            dt[dim] = t_1[dim] - t_0[dim];
        }

        //First position we check is the first integer positions
        var nextT = t_0;
        var t = 0.0f;
        
        
        var counter = 0;
        while (t < maxMineBuildDistance)
        {
            if (counter++ > maxMineBuildDistance * 3 + 1)
            {
                Debug.Log("Too many steps when raymarching inside 3D-DDA for cursor position!");
                break;
            }

            var minDim = 0;
            t = nextT[0];
            
            //Find the smallest step to make and make it
            for (var dim = 1; dim < 3; dim++)
            {
                if (float.IsNaN(nextT[dim])) continue;
                if (nextT[dim] < t)
                {
                    minDim = dim;
                    t = nextT[minDim];
                }
            }
            nextT[minDim] += dt[minDim];
            
            // Find the voxel point in the world
            var t_point = p0 + dir * (t+0.001f);
            var voxelPoint = Vector3Int.FloorToInt(voxelWorld.GetVoxelPoint(t_point));
            if (voxelWorld.IsSolidPoint(voxelPoint))
            {
                _blockSelected = true;
                _blockPosToBreak = voxelPoint;
                
                var t_build = p0 + dir * (t-0.001f);
                _blockPosToBuild = Vector3Int.FloorToInt(voxelWorld.GetVoxelPoint(t_build));

                _cursor.SetActive(true);
                _cursor.transform.position = _blockPosToBreak;
                break;
            }
        }

        // Hide cursor when not looking at anything
        if (t >= maxMineBuildDistance)
        {
            _blockSelected = false;
            _cursor.SetActive(false);
        }
    }

    /// <summary>
    /// Changes the sprite icon to show the current building material
    /// </summary>
    void ChangeBuildIcon()
    {
        var types = voxelWorld.voxelTypes;
        var rect = types.GetAtlasFaceUvs(blockTypeSelected, 0);
        rect.min *= types.atlas.width;
        rect.max *= types.atlas.width;
        Destroy(blockTypeSelectedIcon.sprite);
        blockTypeSelectedIcon.sprite = Sprite.Create(types.atlas, 
            rect, new Vector2(0.5f, 0.5f));
    }
    
    /// <summary>
    /// Build or break a block
    /// </summary>
    private void HandleBuildBreak()
    {
        //Handle type selection
        var scroll = _controls.Player.SwitchBlock.ReadValue<Vector2>();
        if (scroll.y != 0)
        {
            if (scroll.y > 0)
            {
                blockTypeSelected += 1;
                if (blockTypeSelected == voxelWorld.voxelTypes.typeDurability.Count) blockTypeSelected = 1;
            }
            else if (scroll.y < 0)
            {
                blockTypeSelected -= 1;
                if (blockTypeSelected == 0)
                    blockTypeSelected = (sbyte) (voxelWorld.voxelTypes.typeDurability.Count - 1);
            }

            ChangeBuildIcon();
        }

        //Handle build
        if (_blockSelected && _controls.Player.Build.triggered)
        {
            if (_blockPosToBuild != _voxelGridPosition + Vector3Int.up &&
                _blockPosToBuild != _voxelGridPosition + Vector3Int.up * 2)
                voxelWorld.SetVoxel(_blockPosToBuild, blockTypeSelected);
        }
        //Handle break
        else if (_blockSelected && _controls.Player.Break.ReadValue<float>() > 0)
        {
            
            if (_currentlyBreaking != _blockPosToBreak)
            {
                _currentlyBreaking = _blockPosToBreak;
                _currentlyBreakingDamage = 0;

                var currentlyBreakingType = voxelWorld.GetVoxelType(_currentlyBreaking);
                _currentlyBreakingDurability = voxelWorld.voxelTypes.typeDurability[currentlyBreakingType];
            }
            else if (_currentlyBreakingDurability >= 0)
            {
                _currentlyBreakingDamage += Time.deltaTime;
                var scale = 1.1f - (_currentlyBreakingDamage / _currentlyBreakingDurability) * 0.1f;
                _cursorScale.localScale = new Vector3(scale,scale,scale);
                if (_currentlyBreakingDamage >= _currentlyBreakingDurability)
                {
                    _currentlyBreakingDamage = 0;
                    scale = 1.1f;
                    _cursorScale.localScale = new Vector3(scale,scale,scale);
                    voxelWorld.SetVoxel(_blockPosToBreak, 0);
                }
            }
        }
        else
        {
            _currentlyBreakingDamage = 0;
            var scale = 1.1f;
            _cursorScale.localScale = new Vector3(scale,scale,scale);
        }
    }
}
