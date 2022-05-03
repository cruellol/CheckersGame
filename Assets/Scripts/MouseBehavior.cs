using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    public class MouseBehavior : MonoBehaviour
    {
        public event EventHandler<MouseEvents> MouseEvent;

        private void OnMouseEnter()
        {
            MouseEvent?.Invoke(this, MouseEvents.Enter);
        }

        private void OnMouseExit()
        {
            MouseEvent?.Invoke(this, MouseEvents.Leave);
        }

        private void OnMouseDown()
        {
            MouseEvent?.Invoke(this, MouseEvents.Click);
        }
    }

    public enum MouseEvents { Click, Enter, Leave } 
}