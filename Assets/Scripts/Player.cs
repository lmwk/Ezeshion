using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController CharacterController;
    public Transform attackOrigin;
    public float gravity = -9.81f;
    public float movespeed = 5f;
    public float jumpspeed = 5f;
    public float health;
    public float maxHealth = 100f;
    public int itemAmount = 0;
    public int maxItemAmount = 3;
    
    private bool[] inputs;
    private float yvelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        movespeed *= Time.fixedDeltaTime;
        jumpspeed *= Time.fixedDeltaTime;
    }

    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[5];

    }

    public void FixedUpdate()
    {
        if (health <= 0)
        {
            return;
        }
        Vector2 inputdirection = Vector2.zero;

        if (inputs[0])
        {
            inputdirection.y += 1;
        }

        if (inputs[1])
        {
            inputdirection.y -= 1;
        }

        if (inputs[2])
        {
            inputdirection.x += 1;
        }

        if (inputs[3])
        {
            inputdirection.x -= 1;
        }

        Move(inputdirection);

    }

    private void Move(Vector2 inputdirection)
    {

        Vector3 _moveDirection = transform.right * inputdirection.x + transform.forward * inputdirection.y;
        _moveDirection *= movespeed;

        if (CharacterController.isGrounded)
        {
            yvelocity = 0f;
            if (inputs[4])
            {
                yvelocity = jumpspeed;
            }
        }

        yvelocity += gravity;

        _moveDirection.y = yvelocity;
        CharacterController.Move(_moveDirection);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Attack(Vector3 _viewDirection)
    {
        if (Physics.Raycast(attackOrigin.position, _viewDirection, out RaycastHit _hit, 25f))
        {
            if (_hit.transform.root.CompareTag("Player"))
            {
                _hit.transform.root.GetComponent<Player>().TakeDamage(50f);
            }
            else if (_hit.transform.root.CompareTag("Enemy"))
            {
                _hit.transform.root.GetComponent<EnemyMob>().TakeDamage(50f);
            }
        }
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0)
        {
            return;
        }

        health -= _damage;
        if (health <= 0f)
        {
            health = 0f;
            CharacterController.enabled = false;
            transform.position = new Vector3(0f, 0.7f, 0);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }
        
        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        health = maxHealth;
        CharacterController.enabled = true;
        ServerSend.PlayerRespawned(this);
    }

    public bool AttemptPickupItem()
    {
        if (itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount++;
        return true;
    }
    
}
