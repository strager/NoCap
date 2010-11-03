﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Factories;
using NoCap.Plugins.Helpers;

namespace NoCap.Plugins.Commands {
    [Serializable]
    public class CropShot : ICommand {
        public string Name {
            get { return "Crop shot"; }
        }

        public TypedData Process(TypedData data, IMutableProgressTracker progress) {
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
                throw new CommandCancelledException(this);
            }

            progress.Progress = 1;  // TODO Crop shot progress (?)

            return cropShotWindow.Data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Image };
        }

        public ICommandFactory GetFactory() {
            return new CropShotFactory();
        }

        public ITimeEstimate ProcessTimeEstimate {
            get {
                return TimeEstimates.UserInteractive;
            }
        }
    }
}