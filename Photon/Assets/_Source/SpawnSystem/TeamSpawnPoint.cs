using TeamSystem;
using UnityEngine;

namespace SpawnSystem
{
    public class TeamSpawnPoint : MonoBehaviour
    {
        [SerializeField] private TeamType team;

        [Header("Spawn Area")]
        [SerializeField] private Vector2 areaSize = new Vector2(3f, 3f);
        [SerializeField] private float spawnHeight = 1f;

        public TeamType Team => team;

        public Vector3 GetRandomSpawnPosition()
        {
            float randomX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
            float randomZ = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);

            Vector3 localOffset = new Vector3(randomX, 0f, randomZ);
            Vector3 worldPosition = transform.position + localOffset;

            worldPosition.y = spawnHeight;

            return worldPosition;
        }

        public Quaternion GetSpawnRotation()
        {
            return transform.rotation;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = team == TeamType.Purple
                ? new Color(0.45f, 0.15f, 0.9f, 0.35f)
                : new Color(1f, 0.85f, 0.1f, 0.35f);

            Vector3 center = transform.position;
            center.y = spawnHeight;

            Vector3 size = new Vector3(areaSize.x, 0.1f, areaSize.y);

            Gizmos.DrawCube(center, size);

            Gizmos.color = team == TeamType.Purple
                ? new Color(0.45f, 0.15f, 0.9f, 1f)
                : new Color(1f, 0.85f, 0.1f, 1f);

            Gizmos.DrawWireCube(center, size);
        }
    }
}