using Fusion;
using NetworkSystem;
using UnityEngine;

namespace PlayerSystem
{
    public class NetworkPlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData inputData))
            {
                Vector3 moveDirection = inputData.Direction;

                if (moveDirection.sqrMagnitude > 1f)
                {
                    moveDirection.Normalize();
                }

                transform.position += moveDirection * moveSpeed * Runner.DeltaTime;
            }
        }
    }
}