using UnityEngine;

namespace Checkers
{
    public enum ToEmit { None, Blue, Green, Red }
    public class BaseMapObject
    {
        public BaseCoord Position;

        protected Material _currentMaterial;

        public BaseMapObject(int i, int j, Material material)
        {
            Position = new BaseCoord(i, j);
            _currentMaterial = material;
        }
        private ToEmit Emitting;
        public bool IsEmitting
        {
            get
            {
                return Emitting != ToEmit.None;
            }
        }
        internal void Emit(ToEmit emit)
        {
            Emitting = emit;
            switch (emit)
            {
                case ToEmit.Blue:
                    {
                        _currentMaterial.EnableKeyword("_EMISSION");
                        _currentMaterial.SetColor("_EmissionColor", Color.blue);
                        break;
                    }
                case ToEmit.Green:
                    {
                        _currentMaterial.EnableKeyword("_EMISSION");
                        _currentMaterial.SetColor("_EmissionColor", Color.green);
                        break;
                    }
                case ToEmit.Red:
                    {
                        _currentMaterial.EnableKeyword("_EMISSION");
                        _currentMaterial.SetColor("_EmissionColor", Color.red);
                        break;
                    }
                case ToEmit.None:
                default:
                    {
                        _currentMaterial.DisableKeyword("_EMISSION");
                        break;
                    }
            }

        }
    }

}
