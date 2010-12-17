using System.Drawing;
using System.Runtime.Serialization;
using System.Threading;
using NoCap.Extensions.Default.Factories;
using NoCap.Extensions.Default.Helpers;
using NoCap.Library;
using NoCap.Library.Progress;

namespace NoCap.Extensions.Default.Commands {
    [DataContract(Name = "CropShot")]
    public sealed class CropShot : ICommand, IExtensibleDataObject {
        public string Name {
            get { return "Crop shot"; }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress, CancellationToken cancelToken) {
            CropShotWindow cropShotWindow = null;

            var thread = new Thread(() => {
                cropShotWindow = new CropShotWindow {
                    SourceImage = (Image) data.Data,
                    DataName = data.Name
                };

                cropShotWindow.ShowDialog();
            });

            // UI objects want to be in STA; make sure we're in STA
            thread.SetApartmentState(ApartmentState.STA);

            thread.Start();
            thread.Join();

            if (cropShotWindow.Data == null) {
                throw new CommandCanceledException(this);
            }

            progress.Progress = 1;

            return cropShotWindow.Data;
        }

        public ICommandFactory GetFactory() {
            return new CropShotFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.UserInteractive;
            }
        }

        public bool IsValid() {
            return true;
        }

        ExtensionDataObject IExtensibleDataObject.ExtensionData {
            get;
            set;
        }
    }
}