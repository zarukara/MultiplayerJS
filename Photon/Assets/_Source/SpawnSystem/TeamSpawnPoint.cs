using TeamSystem;
using UnityEngine;

namespace SpawnSystem
{
    public class TeamSpawnPoint : MonoBehaviour
    {
        [SerializeField] private TeamType team;

        public TeamType Team => team;
    }
}