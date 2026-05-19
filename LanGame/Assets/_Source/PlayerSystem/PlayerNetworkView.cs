using UnityEngine;

namespace PlayerSystem
{
    public class PlayerNetworkView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Init(bool isLocalPlayer)
        {
            spriteRenderer.color = isLocalPlayer ? Color.white : Color.gray;
        }

        public void UpdateView(float x, float y, bool isDead)
        {
            transform.position = new Vector3(x, y, 0f);
            gameObject.SetActive(!isDead);
        }
    }
}