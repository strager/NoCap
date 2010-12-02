using System;
using System.IO;
using NUnit.Framework;

namespace NoCap.Library.Tests {
    [TestFixture]
    public class TypedDataTests {
        [Test]
        public void ConstructorThrowsOnNullArgs() {
            Assert.Throws<ArgumentNullException>(() => new TypedData(TypedDataType.Text, null, "test"));
            Assert.Throws<ArgumentNullException>(() => new TypedData(TypedDataType.Text, "test", null));
        }

        [Test]
        public void ConstructorDoesNotThrowOnNullNullTypeData() {
            Assert.DoesNotThrow(() => new TypedData(TypedDataType.None, null, "null data"));
        }

        [Test]
        public void FromImage() {
            var image = new System.Drawing.Bitmap(128, 128);

            var data = TypedData.FromImage(image, "name");

            Assert.AreEqual(TypedDataType.Image, data.DataType);
            Assert.AreEqual(image, data.Data);
            Assert.AreEqual("name", data.Name);
        }

        [Test]
        public void FromUriString() {
            string uri = "http://google.com/";

            var data = TypedData.FromUri(uri, "my uri");

            Assert.AreEqual(TypedDataType.Uri, data.DataType);
            Assert.IsInstanceOf<Uri>(data.Data);
            Assert.AreEqual(new Uri(uri), data.Data);
            Assert.AreEqual("my uri", data.Name);
        }

        [Test]
        public void FromUriObject() {
            var uri = new Uri("http://google.com/");

            var data = TypedData.FromUri(uri, "my uri");

            Assert.AreEqual(TypedDataType.Uri, data.DataType);
            Assert.AreEqual(uri, data.Data);
            Assert.AreEqual("my uri", data.Name);
        }

        [Test]
        public void FromStream() {
            Stream stream = new MemoryStream();

            var data = TypedData.FromStream(stream, "raw data");
            
            Assert.AreEqual(TypedDataType.Stream, data.DataType);
            Assert.AreEqual(stream, data.Data);
            Assert.AreEqual("raw data", data.Name);
        }

        // TODO Test Dispose
    }
}
