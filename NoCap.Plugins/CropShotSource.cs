using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using NoCap.Library.Sources;

namespace NoCap.Plugins {
    [Export]
    public class CropShotSource : ISource {
        public IOperation<TypedData> Get() {
            var cropShotForm = new CropShotForm {
                SourceImage = ScreenCapturer.CaptureEntireDesktop()
            };

            return cropShotForm.Operation;
        }

        public IEnumerable<TypedDataType> GetOutputDataTypes() {
            return new[] {
                TypedDataType.Image
            };
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

        private EasyOperation<TypedData> operation;

        public IOperation<TypedData> Operation {
            get {
                return this.operation;
            }
        }

        private Point dragStart;
        private bool isDragging;

        public CropShotForm() {
            this.operation = new EasyOperation<TypedData>((op) => {
                Show();

                return null;
            });

            ShowInTaskbar = false;

            Shown     += (sender, e) => PutOnTop();
            KeyDown   += (sender, e) => KeyPressed(e.KeyCode);
            LostFocus += (sender, e) => Close();

            Closed += (sender, e) => {
                if (this.operation.State != OperationState.Completed) {
                    this.operation.Cancel();
                }
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

            var selectedImage = GetSelectedImage(Rectangle.FromLTRB(this.dragStart.X, this.dragStart.Y, location.X, location.Y));
            this.operation.Done(TypedData.FromImage(selectedImage, "crop shot"));

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
