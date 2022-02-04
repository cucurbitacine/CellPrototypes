using UnityEngine;
using Random = UnityEngine.Random;

namespace Prototype01.Scripts.Others
{
    public class FoodSpawner : MonoBehaviour
    {
        public float Period = 1f;
        public float Timer = 1f;
        public float Force = 1000f;

        public Vector2 Center => transform.position;
        
        private void Spawn()
        {
            var prefab = PrefabList.GetPrefab<FoodController>(PrefabList.Food);
            var food = Instantiate(prefab, Center, Quaternion.identity, transform);

            Vector2 dir = Random.onUnitSphere;
            dir.Normalize();
            food.Body.Rigidbody.AddForce(dir * Force);
        }
        
        private void Update()
        {
            if (Timer <= 0f)
            {
                Timer = Period;

                Spawn();
            }
            else
            {
                Timer -= Time.deltaTime;
            }
        }
    }
}
