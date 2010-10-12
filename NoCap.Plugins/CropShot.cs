using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using NoCap.Library;
using NoCap.Library.Destinations;

namespace NoCap.Plugins {
    [Export(typeof(IDestination))]
    public class CropShot : IDestination {
        public TypedData Put(TypedData data, IProgressTracker progress) {
            var cropShotForm = new CropShotForm {
                SourceImage = (Image) data.Data,
                DataName = data.Name
            };

            cropShotForm.ShowDialog();

            return cropShotForm.Data;
        }

        public IEnumerable<TypedDataType> GetInputDataTypes() {
            return new[] { TypedDataType.Image };
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes(TypedDataType input) {
            return new[] { TypedDataType.Image };
        }
    }

    public class CropShotForm : Form {
        public Image SourceImage {
            get {
                return BackgroundImage;
            }

            set {
                BackgroundImage = value;
            }
        }

        public string DataName {
            get;
            set;
        }

        public TypedData Data {
            get;
            private set;
        }

        private Point dragStart;
        private bool isDragging;

        public CropShotForm() {
            ShowInTaskbar = false;

            Shown     += (sender, e) => PutOnTop();
            KeyDown   += (sender, e) => KeyPressed(e.KeyCode);
            LostFocus += (sender, e) => Close();

            Closed += (sender, e) => {
                // TODO
            };

            MouseDown += (sender, e) => StartDragging(e.Location);
            MouseUp   += (sender, e) => EndDragging(e.Location);
        }

        private void StartDragging(Point location) {
            this.isDragging = true;
            this.dragStart = location;
        }

        private void EndDragging(Point location) {
            if (!this.isDragging) {
                return;
            }

            var region = Rectangle.FromLTRB(
                Math.Min(this.dragStart.X, location.X),
                Math.Min(this.dragStart.Y, location.Y),
                Math.Max(this.dragStart.X, location.X),
                Math.Max(this.dragStart.Y, location.Y)
            );

            var selectedImage = GetSelectedImage(region);

            Data = TypedData.FromImage(selectedImage, DataName);

            Close();
        }

        private Image GetSelectedImage(Rectangle region) {
            var selectedImage = new Bitmap(region.Width, region.Height);
            
            var g = Graphics.FromImage(selectedImage);
            g.DrawImageUnscaled(SourceImage, -region.X, -region.Y);
            return selectedImage;
        }

        private void StopDragging() {
            this.isDragging = false;
        }

        private void KeyPressed(Keys keyCode) {
            if (keyCode.HasFlag(Keys.Escape)) {
                if (this.isDragging) {
                    StopDragging();
                }

                Close();
            }
        }

        private void PutOnTop() {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
            Focus();
        }
    }
}
