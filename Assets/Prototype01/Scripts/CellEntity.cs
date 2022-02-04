using Prototype01.Scripts.Others;
using UnityEngine;

namespace Prototype01.Scripts
{
    [DisallowMultipleComponent]
    public sealed class CellEntity : MonoBehaviour
    {
        [SerializeField] private int cellId = -1;
        [SerializeField] private int groupId = -1;
        [SerializeField] private int generation = 0;

        public int CellId
        {
            get => cellId;
            private set => cellId = value;
        }

        public int GroupId
        {
            get => groupId;
            set => groupId = value;
        }

        public int Generation
        {
            get => generation;
            set => generation = value;
        }

        private void Awake()
        {
            CellId = GetInstanceID();
        }
    }

    [RequireComponent(typeof(CellEntity))]
    [RequireComponent(typeof(CircleBody))]
    public abstract class CellBehaviour : MonoBehaviour
    {
        private CellEntity _entity;
        public CellEntity Entity => _entity != null ? _entity : (_entity = GetComponent<CellEntity>());

        private CircleBody _body;
        public CircleBody Body => _body != null ? _body : (_body = GetComponent<CircleBody>());

        public Vector2 Position => transform.position;
    }
}