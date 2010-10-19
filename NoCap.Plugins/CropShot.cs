﻿using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using NoCap.Library;
using NoCap.Library.Util;
using NoCap.Plugins.Helpers;

namespace NoCap.Plugins {
    [Export(typeof(IDestination))]
    public class CropShot : IDestination {
        public TypedData Put(TypedData data, IMutableProgressTracker progress) {
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

            progress.Progress = 1;  // TODO ?

            return cropShotWindow.Data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Image };
        }
    }
}