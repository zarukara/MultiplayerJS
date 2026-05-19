using UnityEngine;

namespace ViewSystem
{
    public class BulletNetworkView : MonoBehaviour
    {
        public void UpdateView(float x, float y)
        {
            transform.position = new Vector3(x, y, 0f);
        }
    }
}