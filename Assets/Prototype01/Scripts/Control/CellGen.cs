using System;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Prototype01.Scripts.Control
{
    [Serializable]
    public class CellGen
    {
        [Header("Mutation")]
        [Range(0f, 1f)]
        public float mutationProbability;
        [Range(0f, 1f)]
        public float mutationPower;
        
        [Header("General")]
        [Min(0.1f)]
        public float mass;
        [Min(0.1f)]
        public float size;
        [Min(0.1f)]
        public float speed;
        [Min(1f)]
        public float healthMax;
        public float healthRestoreSpeed;
        [Min(1f)]
        public float staminaMax;
        public float staminaRestoreSpeed;
        
        [Header("Feeding")]

        [Min(1f)]
        public float radiusScaleSearchFood;
        [Min(1f)]
        public float radiusFeedingScale;

        [Header("Behaviour")]
        [Range(0f, 1f)]
        public float staminaLevelRestore;
        [Range(0f, 1f)]
        public float staminaLevelFreeWalk;
        [Range(0f, 1f)]
        public float healthLevelSearchFood;
        [Range(0f, 1f)]
        public float healthLevelDivide;
        
        public static CellGen Default => new CellGen()
        {
            mutationProbability = 0.1f,
            mutationPower = 0.1f,
            
            mass = 1f,
            size = 1f,
            speed = 2f,
            healthMax = 100f,
            healthRestoreSpeed = 10f,
            staminaMax = 10f,
            staminaRestoreSpeed = 1f,
            

            radiusScaleSearchFood = 10f,
            radiusFeedingScale = 2f,
            
            staminaLevelRestore = 0.1f,
            staminaLevelFreeWalk = 0.9f,
            healthLevelSearchFood = 0.5f,
            healthLevelDivide = 0.9f,
        };

        public void Mutation()
        {
            mass = Mutation(mass, 0.1f);
            size = Mutation(size, 0.1f);
            speed = Mutation(speed, 0.1f);

            healthMax = Mutation(healthMax, 10f);
            healthRestoreSpeed = Mutation(healthRestoreSpeed, 1f);
            
            staminaMax = Mutation(staminaMax, 1f);
            staminaRestoreSpeed = Mutation(staminaRestoreSpeed, 0.1f);
            
            radiusScaleSearchFood = Mutation(radiusScaleSearchFood, 1.5f);
            radiusFeedingScale = Mutation(radiusFeedingScale, 1.5f);
            
            staminaLevelRestore = Mutation(staminaLevelRestore, 0.1f, 0.9f);
            staminaLevelFreeWalk = Mutation(staminaLevelFreeWalk, 0.1f, 0.9f);
            healthLevelSearchFood = Mutation(healthLevelSearchFood, 0.1f, 0.9f);
            healthLevelDivide = Mutation(healthLevelDivide, 0.1f, 0.9f);
        }

        private float Mutation(float value, float min = float.MinValue, float max = float.MaxValue)
        {
            value += value * mutationPower * (2 * Random.value - 1);
            return Mathf.Clamp(value, min, max);
        }

        public CellGen Copy()
        {
            var copy = new CellGen();

            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                field.SetValue(copy, field.GetValue(this));
            }

            return copy;
        }
    }
}