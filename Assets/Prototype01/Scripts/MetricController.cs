using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype01.Scripts
{
    public class MetricController : MonoBehaviour
    {
        private static MetricController _instance;

        public static MetricController Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<MetricController>();
                return _instance;
            }
        }

        public readonly static Dictionary<int, HashSet<CellController>> Cells = new Dictionary<int, HashSet<CellController>>();

        public static void Add(CellController cell)
        {
            Instance.AddCell(cell);
        }
        
        public static void Remove(CellController cell)
        {
            Instance.RemoveCell(cell);
        }
        
        public void AddCell(CellController cell)
        {
            if (!Cells.TryGetValue(cell.Entity.GroupId, out var cells))
            {
                cells = new HashSet<CellController>();
                cells.Add(cell);
                Cells.Add(cell.Entity.GroupId, cells);
            }
            else
            {
                cells.Add(cell);
            }
        }

        public void RemoveCell(CellController cell)
        {
            if (Cells.TryGetValue(cell.Entity.GroupId, out var cells))
            {
                cells.Remove(cell);
            }
        }

        public int[] GetGroups()
        {
            return Cells.Keys.ToArray();
        }
        
        public float GetTotalHealth(int groupId)
        {
            if (Cells.TryGetValue(groupId, out var cells))
            {
                return cells.Aggregate(0f, (res, cur) => res += cur?.Health?.Value ?? 0f);
            }
            
            return 0f;
        }

        public float totalHealth;

        public float[] healths = new float[2];
        public int[] counts = new int[2];
        
        public Image left;
        public Image right;

        private void Update()
        {
            counts[0] = Cells[0].Count;
            counts[1] = Cells[1].Count;
            
            healths[0] = GetTotalHealth(0);
            healths[1] = GetTotalHealth(1);

            totalHealth = healths[0] + healths[1];
            
            left.fillAmount = totalHealth > 0f ?  healths[0]  / totalHealth : 0f;
            right.fillAmount = totalHealth > 0f ? healths[1] / totalHealth : 0f;
        }
    }
}