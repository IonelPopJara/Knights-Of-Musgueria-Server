using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    /// <summary>
    /// Hasta acá funciona
    /// </summary>
    /// 
    [Header("Networking")]
    public int id;
    public string username;
    public int points;

    [Header("Health")]
    public float maxHealth = 100f;
    public float health;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float maxSpeed = 100f;
    
    public bool grounded;
    
    public Rigidbody rb;
    
    public LayerMask whatIsGround;
    
    public float counterMovement = 0.175f;
    public float maxSlopeAngle = 35;

    [Header("Crouch & Slide")]
    public BoxCollider headCollider;
    public SphereCollider feetCollider;
    public Transform playerBody;
    public float slideCounterMovement = 0.2f;
    public float slideForce;

    [Header("Jump")]
    public float jumpForce;

    [Header("Shoot")]
    public Transform shootOrigin;
    public float throwForce = 600f;
    public int maxItemAmount = 3;
    public int itemAmount = 0;

    [Header("Grappling Gun")]
    public LayerMask whatIsGrappling;
    public float jointSpring = 15f;
    public float jointDamper = 3f;
    public float jointMassScale = 7f;

    public SpringJoint joint;
    public Vector3 currentGrapplePosition;
    private float maxDistance = 100f;

    public LineRenderer lr;
    public Vector3 grapplePoint;

    [Header("Sword")]
    public bool swordAttack;
    public BoxCollider swordHitBox;

    [Header("T Pose")]
    public bool tPose;

    [Header("Conchetumare")]
    public float conchetumareRadius;
    public float conchetumareDamage;
    public float conchetumareForce;

    //Networking
    private bool[] inputs;
    
    //Input
    public bool jumping, crouching, checkCrouch;
    
    //Movement
    private Vector2 _inputDirection;
    private float threshold = 0.01f;
    
    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    
    //Sliding
    private Vector3 normalVector = Vector3.up;
    
    //Jumping
    private float jumpCooldown = 0.25f;
    private bool readyToJump = true;
    
    //Ground Detection
    private bool cancellingGrounded;

    //Shooting
    [HideInInspector]
    public bool shootKeyPressed;

    private void Start()
    {
        //moveSpeed *= Time.fixedDeltaTime;
        //jumpForce *= Time.fixedDeltaTime;

        playerScale = playerBody.transform.localScale;
        
        //lr = GetComponent<LineRenderer>();
    }
    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[6];
    }

    /// <summary>Processes player input and moves the player.</summary>
    public void FixedUpdate()
    {
        if (health <= 0f)
        {
            return;
        }

        GetPlayerInputs();

        //print("Input Direction: " + _inputDirection);

        MoveRigid(_inputDirection);
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    #region PlayerInput
    private void GetPlayerInputs()
    {
        ServerSend.PlayerCrouching(this);
        ServerSend.PlayerSpeed(this);
        ServerSend.PlayerGrounded(this);
        ServerSend.PlayerShooting(this);
        ServerSend.PlayerSwordAttack(this);
        ServerSend.PlayerTPose(this);

        _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        jumping = inputs[4];
        crouching = inputs[5];

        if (inputs[5] != checkCrouch)
        {
            if (!checkCrouch)
            {
                StartCrouch();
            }
            else if (checkCrouch)
            {
                StopCrouch();
            }

            checkCrouch = inputs[5];
        }
    }

    /// <summary>
    /// Calculates the player's desired movement direction and moves him
    /// </summary>
    /// <param name="_inputDirection"></param>
    private void MoveRigid(Vector2 _inputDirection)
    {
        //Adds extra gravity to the player
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        Vector2 mag = FindRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        CounterMovement(_inputDirection.x, _inputDirection.y, mag);

        if (readyToJump && jumping)
        {
            Jump();
        }

        if(crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this);
            return;
        }

        //If speed is larger than maxspeed
        if (_inputDirection.x > 0 && xMag > maxSpeed) _inputDirection.x = 0;
        if (_inputDirection.x < 0 && xMag < -maxSpeed) _inputDirection.x = 0;
        if (_inputDirection.y > 0 && yMag > maxSpeed) _inputDirection.y = 0;
        if (_inputDirection.y < 0 && yMag < -maxSpeed) _inputDirection.y = 0;

        float multiplier = 1f, multiplierV = 1f;

        if(!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }
        
        if(grounded && crouching)
        {
            multiplierV = 0f;
        }

        //_inputDirection me da un vector (x,y) dependiendo de hacia donde me quiero mover. Me voy al frente, será (0,1), atrás (0,-1) y así
        rb.AddForce(transform.forward * _inputDirection.y * moveSpeed * multiplier * multiplierV * Time.deltaTime);
        rb.AddForce(transform.right * _inputDirection.x * moveSpeed * multiplier * Time.deltaTime);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    /// <summary>Updates the player input with newly received input.</summary>
    /// <param name="_inputs">The new key inputs.</param>
    /// <param name="_rotation">The new rotation.</param>
    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    #endregion

    #region Grappling
    public void StartGrappling(Vector3 _viewDirection)
    {
        if (health <= 0f)
        {
            return;
        }

        RaycastHit hit;
        if(Physics.Raycast(shootOrigin.position, _viewDirection, out hit, maxDistance, whatIsGrappling))
        {
            grapplePoint = hit.point;

            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(transform.position, grapplePoint);

            joint.maxDistance = distanceFromPoint * 0.3f;
            joint.minDistance = distanceFromPoint * 0.1f;

            joint.spring = jointSpring;
            joint.damper = jointDamper;
            joint.massScale = jointMassScale;

            lr.positionCount = 2;
            currentGrapplePosition = shootOrigin.position;
        }
        /*
        if (Physics.Raycast(shootOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.collider.CompareTag("Player"))
            {
                _hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
        }*/
    }

    public void StopGrappling()
    {
        lr.positionCount = 0;
        Destroy(joint);
    }

    public void DrawRope()
    {
        ServerSend.Joint(this);
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        lr.SetPosition(0, shootOrigin.position);
        lr.SetPosition(1, currentGrapplePosition);

        ServerSend.GrapplePoint(this);
    }
    #endregion

    #region Shooting
    public void Shoot(Vector3 _viewDirection)
    {
        shootKeyPressed = true;

        if (health <= 0f)
        {
            return;
        }

        if (itemAmount > 0)
        {
            itemAmount--;
            NetworkManager.instance.InstantiateProjectile(shootOrigin).Initialize(_viewDirection, throwForce + rb.velocity.magnitude, id);
        }
    }

    public void StopShooting()
    {
        shootKeyPressed = false;
    }
    #endregion

    #region Sword
    public void SwordAttack()
    {
        //Activate Sword HitBox
        swordAttack = true;
    }

    public void StopSwordAttack()
    {
        //Deactivate Sword HitBox
        //Maybe instead of using the keyUp, I could use an animation event
        swordAttack = false;
    }

    public void ActivateSwordCollider()
    {
        swordHitBox.enabled = true;
    }

    public void DeactivateSwordCollider()
    {
        swordHitBox.enabled = false;
    }
    #endregion

    #region T Pose
    public void StartTPose()
    {
        tPose = true;
    }

    public void StopTPose()
    {
        tPose = false;
    }
    #endregion

    #region Health and Interactions
    public void TakeDamage(float _damage, int idPlayerDoingDamage)
    {
        string namePlayerDoingDamage;

        if(idPlayerDoingDamage != -1)
        {
            namePlayerDoingDamage = Server.clients[idPlayerDoingDamage].player.username;
        }
        else
        {
            namePlayerDoingDamage = "";
        }

        ServerSend.PlayerDamaged(this);

        //print($"You were damaged by {Server.clients[idPlayerDoingDamage].player.username}");

        if (health <= 0f)
        {
            return;
        }

        health -= _damage;
        if (health <= 0f)
        {
            //ServerSend.PlayerKilledYou();
            health = 0f;
            //Deshabilitar movimiento
            //controller.enabled = false;
            rb.velocity = new Vector3(0f, 0f, 0f);
            transform.position = new Vector3(0f, 500f, 0f);
            rb.isKinematic = true;
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
            itemAmount = 0;

            if(idPlayerDoingDamage != -1)
            {
                Server.clients[idPlayerDoingDamage].player.points += 10;
            }
        }
        //print("El error está después de esto, " + namePlayerDoingDamage);
        ServerSend.PlayerHealth(this, namePlayerDoingDamage);
        //print("El error está después de esto");
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        rb.isKinematic = false;
        //controller.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

    public void RespawnAtSpawnPoint(Vector3 spawnPoint)
    {
        transform.position = spawnPoint;

        health = maxHealth;
        rb.isKinematic = false;
        ServerSend.PlayerPosition(this);
    }

    public bool AttemptPickupItem()
    {
        if (itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount += 5;
        return true;
    }
    #endregion

    #region CounterMovement
    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        if(crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        if ((Math.Abs(mag.x) > threshold && Math.Abs(_inputDirection.x) < 0.05f) || (mag.x < -threshold && _inputDirection.x > 0) || (mag.x > threshold && _inputDirection.x < 0))
        {
            rb.AddForce(moveSpeed * transform.right * -mag.x * counterMovement * Time.deltaTime);
        }
        if ((Math.Abs(mag.y) > threshold && Math.Abs(_inputDirection.y) < 0.05f) || (mag.y < -threshold && _inputDirection.y > 0) || (mag.y > threshold && _inputDirection.y < 0))
        {
            rb.AddForce(moveSpeed * transform.forward * -mag.y * counterMovement * Time.deltaTime);
        }
    }

    public Vector2 FindRelativeToLook()
    {
        float lookAngle = transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitude = rb.velocity.magnitude;
        float yMag = magnitude * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitude * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }
    #endregion

    #region Crouch
    private void StartCrouch()
    {
        headCollider.enabled = false;
        feetCollider.enabled = false;
        //playerBody.transform.localScale = crouchScale;
        //playerBody.transform.position = new Vector3(playerBody.transform.position.x, playerBody.transform.position.y - 0.5f, playerBody.transform.position.z);
        if(rb.velocity.magnitude > 0f)
        {
            if(grounded)
            {
                rb.AddForce(transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        headCollider.enabled = true;
        feetCollider.enabled = true;
        rb.AddForce(transform.up * 20,ForceMode.Impulse);
        //playerBody.transform.localScale = playerScale;
        //playerBody.transform.position = new Vector3(playerBody.transform.position.x, playerBody.transform.position.y + 0.5f, playerBody.transform.position.z);
    }
    #endregion

    #region Jump
    private void Jump()
    {
        if(grounded && readyToJump)
        {
            readyToJump = false;

            rb.AddForce(Vector2.up * jumpForce * 1.5f, ForceMode.Impulse);
            rb.AddForce(normalVector * jumpForce * 0.5f, ForceMode.Impulse);

            Vector3 vel = rb.velocity;

            if(rb.velocity.y < 0.5f)
            {
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            }
            else if(rb.velocity.y > 0.5f)
            {
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            }

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }
    #endregion

    #region GroundDetection
    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private void OnCollisionStay(Collision other)
    {
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer)))
            return;

        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if(IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground cancel
        float delay = 3f;
        if(!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }
    #endregion

    #region Conchetumare
    public void Conchetumare()
    {
        //OverlapSphere

        Collider[] _colliders = Physics.OverlapSphere(transform.position, conchetumareRadius);
        foreach (Collider _collider in _colliders)
        {
            if (_collider.CompareTag("Player Main Collider"))
            {
                Player _player = _collider.transform.parent.GetComponent<Player>();
                if (_player != null)
                {
                    if (_player.id == id)
                    {
                        _player.TakeDamage(0f, id);
                    }
                    else
                    {
                        _player.TakeDamage(conchetumareDamage, id);
                    }
                    print("Take Damage");
                }
            }
        }

        Collider[] _collidersToMove = Physics.OverlapSphere(transform.position, conchetumareRadius);
        foreach (Collider _collider in _collidersToMove)
        {
            if (_collider.CompareTag("Player Main Collider"))
            {
                Player _player = _collider.transform.parent.GetComponent<Player>();
                if (_player != null)
                {
                    if(_player.id != id)
                    {
                        _player.transform.GetComponent<Rigidbody>().AddExplosionForce(conchetumareForce, transform.position, conchetumareRadius);
                    }
                }
            }
        }

        ServerSend.PlayerConchetumareReceived(this);
    }
    #endregion
}