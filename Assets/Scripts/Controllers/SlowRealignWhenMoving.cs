using UnityEngine;

namespace Controllers
{
    public class SlowRealignWhenMoving : PseudoHapticController
    {
        public override string Name => "Slow Realign When Moving";
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private float speedThreshold = 0.5f;

        protected override void UpdateVirtual()
        {
            base.UpdateVirtual();
            if (deltaPosition.magnitude + Quaternion.Angle(Quaternion.identity, deltaRotation) > speedThreshold)
            {
                VirtualPosition = Vector3.Lerp(VirtualPosition, RealPosition, smoothTime * Time.deltaTime);
                VirtualRotation = Quaternion.Slerp(VirtualRotation, RealRotation, smoothTime * Time.deltaTime);
            }
        }
    }
}
