using UnityEngine;

namespace Controllers
{
    public class SlowRealignContinuous : PseudoHapticController
    {
        public override string Name => "Slow Realign Continuous";
        [SerializeField] private float smoothTime = 0.1f;

        protected override void UpdateVirtual()
        {
            base.UpdateVirtual();
            VirtualPosition = Vector3.Lerp(VirtualPosition, RealPosition, smoothTime * Time.deltaTime);
            VirtualRotation = Quaternion.Slerp(VirtualRotation, RealRotation, smoothTime * Time.deltaTime);
        }
    }
}
