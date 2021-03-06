using UnityEngine;

namespace Controllers
{
    public class SlowRealignOnRelease : PseudoHapticController
    {
        public override string Name => "Slow Realign On Release";
        [SerializeField] private float smoothTime = 0.1f;

        protected override void UpdateVirtual()
        {
            base.UpdateVirtual();
            if (!_holding)
            {
                VirtualPosition = Vector3.Lerp(VirtualPosition, RealPosition, smoothTime * Time.deltaTime);
                VirtualRotation = Quaternion.Slerp(VirtualRotation, RealRotation, smoothTime * Time.deltaTime);
            }
        }
    }
}
