namespace NoCap.Update {
    public class Patch {
        private readonly PatchInfo patchInfo;

        public Patch(PatchInfo patchInfo) {
            this.patchInfo = patchInfo;
        }

        public void Apply(string applicationRoot) {
            foreach (var instruction in this.patchInfo.Instructions) {
                instruction.Apply(this.patchInfo.patchDataRoot, applicationRoot);
            }
        }
    }
}