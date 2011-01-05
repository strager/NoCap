using NoCap.Library.Util;
using NoCap.Update;

namespace NoCap.GUI.WPF {
    public class Updater {
        private readonly PatchingEnvironment patchingEnvironment;

        public Updater() {
            this.patchingEnvironment = PatchingEnvironment.Create(PatchingEnvironment.GetCurrent());
        }

        public void CheckForUpdates() {
            // TODO
        }

        public void Commit() {
            if (!this.patchingEnvironment.IsModified) {
                // If the patching environment wasn't modified,
                // don't apply any changes.
                return;
            }

            Process.QueueDOPE(Process.Quote(
                "--approot", PatchingEnvironment.GetCurrent().ApplicationRoot,
                "--replace-with", this.patchingEnvironment.ApplicationRoot,
                "--delete", this.patchingEnvironment.ApplicationRoot
            ));
        }
    }
}