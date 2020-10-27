using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rigidBody;
    public int throwByPlayer;
    public Vector3 initialForce;
    public float explosionRadius = 1.5f;
    public float explosionDamage = 75f;
    public float explosionForce = 5000;
    public float explosionMoveRadius = 2000f;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, throwByPlayer);

        rigidBody.AddForce(initialForce);
        StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;
        if (collision.collider == null)
            return;
        if (collision.collider.transform == null)
            return;
        if (collision.collider.transform.parent == null)
            return;
        if (collision.collider.transform.parent.GetComponent<Player>() != null)
        {
            /*
            print("error");
            if (collision.collider.transform.parent.GetComponent<Player>().id == throwByPlayer)
            {
                print("No choques contra ti wn!");
                return;
            }*/
        }

        Explode();
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _thrownByPlayer)
    {
        initialForce = _initialMovementDirection * _initialForceStrength;
        throwByPlayer = _thrownByPlayer;
    }

    private void Explode()
    {
        ServerSend.ProjectileExploded(this);

        Collider[] _colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider _collider in _colliders)
        {
            if(_collider.CompareTag("Player Main Collider"))
            {
                Player _player = _collider.transform.parent.GetComponent<Player>();
                if(_player != null)
                {
                    if (_player.id == throwByPlayer)
                    {
                        _player.TakeDamage(0f, throwByPlayer);
                    }
                    else
                    {
                        _player.TakeDamage(explosionDamage, throwByPlayer);
                    }
                    print("Take Damage");
                }
            }
        }

        Collider[] _collidersToMove = Physics.OverlapSphere(transform.position, explosionMoveRadius);
        foreach (Collider _collider in _collidersToMove)
        {
            if (_collider.CompareTag("Player Main Collider"))
            {
                Player _player = _collider.transform.parent.GetComponent<Player>();
                if (_player != null)
                {//Comentar esto quiz[as. No era na :c
                    print("Muevete");
                    _player.transform.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionMoveRadius);
                }
            }
        }

        projectiles.Remove(id);
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(10f);

        Explode();
    }
}
