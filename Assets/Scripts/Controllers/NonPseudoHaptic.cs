namespace Controllers
{
    public class NonPseudoHaptic : PseudoHapticController
    {
        public override string Name => "Non Pseudo Haptic";

        protected override void UpdateVirtual()
        {
            base.UpdateVirtual();
            ResetDrift();
        }
    }
}
