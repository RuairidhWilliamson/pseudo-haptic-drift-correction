using UnityEngine;

namespace Controllers
{
    public class RealignWhenOutOfView : PseudoHapticController
    {
        public override string Name => "Realign When Out Of View";
        private Camera _camera;
        protected override void Start()
        {
            base.Start();
            _camera = Camera.main;
        }

        protected override void UpdateVirtual()
        {
            base.UpdateVirtual();
            Vector3 screenPos = _camera.WorldToScreenPoint(VirtualPosition);
            if ((screenPos.x < 0f || screenPos.x > _camera.pixelWidth) && (screenPos.y < 0f || screenPos.y > _camera.pixelHeight))
            {
                ResetDrift();
            }
        }
    }
}
