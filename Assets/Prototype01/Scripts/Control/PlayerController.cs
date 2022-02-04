using System;
using System.Collections.Generic;
using Prototype01.Scripts.Others;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Prototype01.Scripts.Control
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly Collider2D[] PointOverlaps = new Collider2D[8];
        
        [Header("Input")]
        public PlayerInput PlayerInput;
        
        [Space]
        public float SelectDamp = 16f;
        
        [Space]
        public Camera Camera = default;
        public float CameraDamp = 32f;
        public float zoom;
        public float zoomSpeed = 32f;
        public float zoomDamp = 8f;
        
        [Space]
        public SpriteRenderer SpriteZone = default;
        public PathController PathController = default;
        
        private List<CellController> cells = default;
        
        private Vector2 _startMove;
        private Vector2 _endMove;
        
        private Rect _rectZone;
        private Vector2 _startZone;
        private Vector2 _endZone;
        private Collider2D[] _selectOverlap;
        
        public List<CellController> Cells => cells != null ? cells : (cells = new List<CellController>());
        
        public void AddCells(params CellController[] cells)
        {
            foreach (var cell in cells)
            {
                if (!Cells.Contains(cell))
                {
                    if (!cell.Autopilot.isOn)
                    {
                        Cells.Add(cell);
                        cell.SelectedBy(this);
                    }
                }
            }
        }

        public void RemoveCells(params CellController[] cells)
        {
            foreach (var cell in cells)
            {
                if (Cells.Remove(cell)) cell.UnselectedBy(this);
            }
        }
        
        public void SetCells(params CellController[] cells)
        {
            ClearCells();

            AddCells(cells);
        }

        public void ClearCells()
        {
            RemoveCells(Cells.ToArray());
        }

        private void SelectCells(bool clearCells = true)
        {
            var count = Physics2D.OverlapBoxNonAlloc(_rectZone.center, _rectZone.size, 0f, _selectOverlap);

            if (clearCells) ClearCells();

            for (var i = 0; i < count; i++)
            {
                var cell = _selectOverlap[i].GetComponent<CellController>();
                if (cell != null)
                {
                    AddCells(cell);
                }
            }
        }

        private void UpdateInput()
        {
            PlayerInput.position = Camera.ScreenToWorldPoint(Input.mousePosition);

            var view = Input.GetKey(KeyCode.Mouse2);
            var select = Input.GetKey(KeyCode.Mouse0);
            var move = Input.GetKey(KeyCode.Mouse1);
            
            PlayerInput.viewStart = !PlayerInput.viewUpdate && view;
            PlayerInput.viewUpdate = view;
            
            PlayerInput.selectStart = !PlayerInput.selectUpdate && select;
            PlayerInput.selectEnd = PlayerInput.selectUpdate && !select;
            PlayerInput.selectUpdate = select;
            
            PlayerInput.move = move;

            PlayerInput.zoom = Input.mouseScrollDelta.y;

            PlayerInput.divide = Input.GetKeyDown(KeyCode.Space);
        }

        public Button restart;
        
        private void Awake()
        {
            _selectOverlap = new Collider2D[256];

            if (Camera == null) Camera = Camera.main;

            restart?.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        }

        private void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            zoom = Camera.orthographicSize;
        }

        private void Update()
        {
            /*
             * Input
             */

            UpdateInput();

            /*
             * View
             */

            var deltaTime = Time.unscaledDeltaTime;
            
            zoom -= zoomSpeed * PlayerInput.zoom * deltaTime;
            Camera.orthographicSize = Mathf.Lerp(Camera.orthographicSize, zoom, zoomDamp * deltaTime);   
            
            if (PlayerInput.viewStart)
            {
                _startMove = PlayerInput.position;;
            }
            
            if (PlayerInput.viewUpdate)
            {
                _endMove = PlayerInput.position;
                var position = Camera.transform.position + (Vector3) (_startMove - _endMove);
                Camera.transform.position =  Vector3.Lerp(Camera.transform.position, position, CameraDamp * deltaTime);
            }

            /*
             * Select
             */
            
            if (PlayerInput.selectStart)
            {
                _startZone = PlayerInput.position;
                
                SpriteZone.transform.position = PlayerInput.position;
                SpriteZone.transform.localScale = Vector3.zero;
            }
            
            if (PlayerInput.selectUpdate)
            {
                SpriteZone.enabled = true;
                
                _endZone = PlayerInput.position;
                
                var size = _endZone - _startZone;

                for (var i = 0; i < 2; i++)
                {
                    size[i] = Mathf.Abs(size[i]);
                }
                _rectZone.size = size;
                _rectZone.center = (_startZone + _endZone) * 0.5f;

                SpriteZone.transform.position = Vector3.Lerp(SpriteZone.transform.position, _rectZone.center,
                    SelectDamp * deltaTime);
                SpriteZone.transform.localScale = Vector3.Lerp(SpriteZone.transform.localScale, _rectZone.size,
                    SelectDamp * deltaTime);
            }

            if (PlayerInput.selectEnd)
            {
                SpriteZone.enabled = false;
                
                SelectCells();
            }
            
            /*
             * Move
             */

            if (PlayerInput.move)
            {
                var position = PlayerInput.position;

                var count = Physics2D.OverlapPointNonAlloc(position, PointOverlaps);

                FoodBehaviour food = null;
                for (var i = 0; i < count; i++)
                {
                    food = PointOverlaps[i].gameObject.GetComponent<FoodBehaviour>();
                    if (food != null) break;
                }
                
                if (food != null)
                {
                    Cells.ForEach(c => c.StartEat(food));

                    foreach (var cell in Cells)
                    {
                        position = food.transform.position;
                        position = (cell.Position - position).normalized *
                                   (cell.Body.Radius + food.Body.Radius) + position;

                        cell.Move(PathController.Move(cell, position));
                    }
                }
                else
                {
                    Cells.ForEach(c => c.StopEat());    
                    Cells.ForEach(c => c.Move(PathController.Move(c, position)));
                }
            }

            if (PlayerInput.divide)
            {
                Cells.ForEach(c => c.Divide());
            }
        }
    }
    
    [Serializable]
    public struct PlayerInput
    {
        public Vector2 position;

        [Space]
        public bool viewStart;
        public bool viewUpdate;
        
        [Space]
        public bool selectStart;
        public bool selectUpdate;
        public bool selectEnd;
        
        [Space]
        public bool move;

        [Space]
        public bool divide;
        
        [Space]
        public float zoom;
    }
}