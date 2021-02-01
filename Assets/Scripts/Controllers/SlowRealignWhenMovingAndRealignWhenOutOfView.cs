using UnityEngine;

namespace Controllers
{
    public class SlowRealignWhenMovingAndRealignWhenOutOfView : PseudoHapticController
    {
        public override string Name => "Slow Realign When Moving and Realign When Out Of View";
        private Camera _camera;
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private float speedThreshold = 0.5f;
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
            if (deltaPosition.magnitude + Quaternion.Angle(Quaternion.identity, deltaRotation) > speedThreshold)
            {
                VirtualPosition = Vector3.Lerp(VirtualPosition, RealPosition, smoothTime * Time.deltaTime);
                VirtualRotation = Quaternion.Slerp(VirtualRotation, RealRotation, smoothTime * Time.deltaTime);
            }
        }
    }
}
