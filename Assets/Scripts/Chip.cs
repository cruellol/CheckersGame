using System;
using UnityEngine;

namespace Checkers
{
    public class Chip : BaseMapObject
    {
        private GameObject _chipObject;
        public GameObject ChipObject
        {
            get
            {
                return _chipObject;
            }
            private set
            {
                _chipObject = value;
            }
        }
        private GameObject _bindRectObject;
        public GameObject BindRectObject
        {
            get
            {
                return _bindRectObject;
            }
            private set
            {
                _bindRectObject = value;
            }
        }
        private bool _isWhite;
        public bool IsWhite
        {
            private set { _isWhite = value; }
            get
            {
                return _isWhite;
            }
        }

        public string IsItWhiteString()
        {
            return IsWhite ? "White" : "Black";
        }

        private MouseBehavior Mouse;

        private bool _isQueen = false;
        public bool IsQueen
        {
            protected set
            {
                _isQueen = value;
            }
            get
            {
                return _isQueen;
            }
        }

        public void SetQueen()
        {
            IsQueen = true;
        }

        public Chip(int i, int j, GameObject newChip, GameObject bindRectObject, Material material, bool isWhite) : base(i, j, material)
        {
            ChipObject = newChip;
            IsWhite = isWhite;
            BindRectObject = bindRectObject;
            Mouse = (MouseBehavior)newChip.AddComponent(typeof(MouseBehavior));
            Mouse.MouseEvent += Mouse_MouseEvent;
        }

        public event EventHandler<MouseEvents> MouseEvent;
        private void Mouse_MouseEvent(object sender, MouseEvents mouseEvent)
        {
            MouseEvent?.Invoke(this, mouseEvent);
        }

        internal void SetNewPos(BaseCoord newPosition)
        {
            Position = newPosition;
        }

        internal void Destroy()
        {
            Mouse.MouseEvent -= Mouse_MouseEvent;
            GameObject.Destroy(BindRectObject);
        }
    }

}