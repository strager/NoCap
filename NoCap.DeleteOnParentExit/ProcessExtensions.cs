using System.Diagnostics;

namespace NoCap.DeleteOnParentExit {
    // Source: http://stackoverflow.com/questions/394816/how-to-get-parent-process-in-net-in-managed-way/2336322#2336322
    public static class ProcessExtensions {
        private static string FindIndexedProcessName(int pid) {
            var processName = Process.GetProcessById(pid).ProcessName;
            var processesByName = Process.GetProcessesByName(processName);
            string processIndexedName = null;

            for (int index = 0; index < processesByName.Length; index++) {
                processIndexedName = index == 0 ? processName : processName + "#" + index;

                var processId = new PerformanceCounter("Process", "ID Process", processIndexedName);

                if ((int) processId.NextValue() == pid) {
                    return processIndexedName;
                }
            }

            return processIndexedName;
        }

        private static Process FindPidFromIndexedProcessName(string indexedProcessName) {
            var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);

            return Process.GetProcessById((int) parentId.NextValue());
        }

        public static Process GetParent(this Process process) {
            return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
        }
    }
}