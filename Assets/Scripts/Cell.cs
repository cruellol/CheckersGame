using System;
using UnityEngine;

namespace Checkers
{
    public class Cell : BaseMapObject
    {
        private GameObject _cellObject;
        public Cell(int i, int j, GameObject newCell, Material material) : base(i, j, material)
        {
            _cellObject = newCell;
            var mouse = (MouseBehavior)newCell.AddComponent(typeof(MouseBehavior));
            if ((i + j) % 2 == 0)
            {
                mouse.MouseEvent += Mouse_MouseEvent;
            }
        }


        public event EventHandler<MouseEvents> MouseEvent;
        private void Mouse_MouseEvent(object sender, MouseEvents mouseEvent)
        {
            MouseEvent?.Invoke(this, mouseEvent);
        }

    }

}
