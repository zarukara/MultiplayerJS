using Mirror;
using UnityEngine;

namespace PlayerSystem
{
    public class MirrorPlayer : NetworkBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Combat")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private GameObject bulletPrefab;

        [SyncVar]
        private int currentHealth;

        private Vector3 lastLookDirection = Vector3.forward;

        public override void OnStartServer()
        {
            currentHealth = maxHealth;
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            HandleMovement();
            HandleRotation();
            HandleShoot();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 direction = new Vector3(horizontal, 0f, vertical);

            if (direction.sqrMagnitude > 1f)
            {
                direction.Normalize();
            }

            transform.position += direction * moveSpeed * Time.deltaTime;
        }

        private void HandleRotation()
        {
            Vector3 lookDirection = GetMouseDirection();

            if (lookDirection.sqrMagnitude <= 0.01f)
            {
                return;
            }

            lastLookDirection = lookDirection;

            transform.rotation = Quaternion.LookRotation(
                lookDirection,
                Vector3.up);
        }

        private void HandleShoot()
        {
            if (Input.GetMouseButtonDown(0))
            {
                CmdShoot(lastLookDirection);
            }
        }

        private Vector3 GetMouseDirection()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return lastLookDirection;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 direction = hitPoint - transform.position;

                direction.y = 0f;

                if (direction.sqrMagnitude > 0.01f)
                {
                    return direction.normalized;
                }
            }

            return lastLookDirection;
        }

        [Command]
        private void CmdShoot(Vector3 direction)
        {
            if (bulletPrefab == null)
            {
                Debug.LogError("Bullet prefab is not assigned");
                return;
            }

            if (direction.sqrMagnitude <= 0.01f)
            {
                return;
            }

            direction.Normalize();

            Vector3 spawnPosition;

            if (shootPoint != null)
            {
                spawnPosition = shootPoint.position;
            }
            else
            {
                spawnPosition = transform.position + direction * 1.2f + Vector3.up * 0.5f;
            }

            GameObject bulletObject = Instantiate(
                bulletPrefab,
                spawnPosition,
                Quaternion.LookRotation(direction));

            WeaponSystem.Bullet bullet =
                bulletObject.GetComponent<WeaponSystem.Bullet>();

            bullet.Init(direction, netId);

            NetworkServer.Spawn(bulletObject);
        }

        [Server]
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            Debug.Log("Player " + netId + " HP: " + currentHealth);

            if (currentHealth <= 0)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }
}