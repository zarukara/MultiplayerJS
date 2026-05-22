using Mirror;
using PlayerSystem;
using UnityEngine;

namespace WeaponSystem
{
    public class Bullet : NetworkBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private int damage = 1;

        private Vector3 direction;
        private uint ownerNetId;

        [Server]
        public void Init(Vector3 shootDirection, uint ownerId)
        {
            direction = shootDirection.normalized;
            ownerNetId = ownerId;

            Invoke(nameof(DestroyBullet), lifeTime);
        }

        [ServerCallback]
        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            MirrorPlayer player = other.GetComponent<MirrorPlayer>();

            if (player == null)
            {
                return;
            }

            if (player.netId == ownerNetId)
            {
                return;
            }

            player.TakeDamage(damage);

            DestroyBullet();
        }

        [Server]
        private void DestroyBullet()
        {
            if (gameObject != null)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}