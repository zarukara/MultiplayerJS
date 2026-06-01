using Mirror;
using RoundSystem;
using TeamSystem;
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

        [Header("Visual")]
        [SerializeField] private Renderer bodyRenderer;
        [SerializeField] private Color redTeamColor = Color.red;
        [SerializeField] private Color blueTeamColor = Color.blue;
        [SerializeField] private Color deadColor = Color.gray;

        [SyncVar(hook = nameof(OnHealthChanged))]
        private int currentHealth;

        [SyncVar(hook = nameof(OnTeamChanged))]
        private TeamType team;

        [SyncVar(hook = nameof(OnDeadChanged))]
        private bool isDead;

        private Vector3 lastLookDirection = Vector3.forward;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public TeamType Team => team;
        public bool IsDead => isDead;

        public override void OnStartServer()
        {
            currentHealth = maxHealth;
            isDead = false;
        }

        public override void OnStartClient()
        {
            ApplyTeamColor();
            ApplyDeadVisual();
        }

        [Server]
        public void ServerInitialize(TeamType newTeam)
        {
            team = newTeam;
            currentHealth = maxHealth;
            isDead = false;
        }

        private void Update()
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (isDead)
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
            if (isDead)
            {
                return;
            }

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

            bullet.Init(direction, netId, team);

            NetworkServer.Spawn(bulletObject);
        }

        [Server]
        public void TakeDamage(int damage)
        {
            if (isDead)
            {
                return;
            }

            currentHealth -= damage;

            Debug.Log("Player " + netId + " HP: " + currentHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDead = true;

                Debug.Log("Player " + netId + " died");

                if (RoundManager.Instance != null)
                {
                    RoundManager.Instance.ServerCheckRoundState();
                }
            }
        }

		[Server]
		public void Respawn(Vector3 spawnPosition, Quaternion spawnRotation)
		{
 		   currentHealth = maxHealth;
 		   isDead = false;

   		 transform.position = spawnPosition;
  		  transform.rotation = spawnRotation;

 		   Debug.Log($"Respawn player {netId} team {team} to {spawnPosition}");

		    RpcRespawnForObservers(spawnPosition, spawnRotation);

 		   if (connectionToClient != null)
		    {
 		       TargetRespawnOwner(connectionToClient, spawnPosition, spawnRotation);
 		   }
		}

		[ClientRpc]
		private void RpcRespawnForObservers(Vector3 spawnPosition, Quaternion spawnRotation)
		{
 		   transform.position = spawnPosition;
   		 transform.rotation = spawnRotation;

  		  ApplyDeadVisual();
		}

		[TargetRpc]
		private void TargetRespawnOwner(
 		   NetworkConnectionToClient target,
 		   Vector3 spawnPosition,
 		   Quaternion spawnRotation)
		{
  		  transform.position = spawnPosition;
 		   transform.rotation = spawnRotation;
	
 		   ApplyDeadVisual();
		}

        private void OnHealthChanged(int oldValue, int newValue)
        {
        }

        private void OnTeamChanged(TeamType oldTeam, TeamType newTeam)
        {
            ApplyTeamColor();
        }

        private void OnDeadChanged(bool oldValue, bool newValue)
        {
            ApplyDeadVisual();
        }

        private void ApplyTeamColor()
        {
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyRenderer == null)
            {
                return;
            }

            bodyRenderer.material.color =
                team == TeamType.Red ? redTeamColor : blueTeamColor;
        }

        private void ApplyDeadVisual()
        {
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            if (bodyRenderer == null)
            {
                return;
            }

            if (isDead)
            {
                bodyRenderer.material.color = deadColor;
            }
            else
            {
                ApplyTeamColor();
            }
        }
    }
}