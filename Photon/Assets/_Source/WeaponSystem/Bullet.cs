using Mirror;
using PlayerSystem;
using TeamSystem;
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
        private TeamType ownerTeam;

        [Server]
        public void Init(
            Vector3 shootDirection,
            uint ownerId,
            TeamType team)
        {
            direction = shootDirection.normalized;
            ownerNetId = ownerId;
            ownerTeam = team;

            Invoke(nameof(DestroyBullet), lifeTime);
        }

        [ServerCallback]
        private void Update()
        {
            transform.position += direction * (speed * Time.deltaTime);
        }

        [ServerCallback]
        private void OnTriggerEnter(Collider other)
        {
            MirrorPlayer player =
                other.GetComponentInParent<MirrorPlayer>();

            if (player == null)
            {
                DestroyBullet();
                return;
            }

            if (player.netId == ownerNetId)
            {
                return;
            }

            if (player.Team == ownerTeam)
            {
                DestroyBullet();
                return;
            }

            if (player.IsDead)
            {
                DestroyBullet();
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