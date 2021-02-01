using UnityEngine;

namespace Controllers
{
    public class SlowRealignWhenOutOfView : PseudoHapticController
    {
        public override string Name => "Slow Realign When Out Of View";
        [SerializeField] private float smoothTime = 0.1f;
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
                VirtualPosition = Vector3.Lerp(VirtualPosition, RealPosition, smoothTime * Time.deltaTime);
                VirtualRotation = Quaternion.Slerp(VirtualRotation, RealRotation, smoothTime * Time.deltaTime);
            }
        }
    }
}
