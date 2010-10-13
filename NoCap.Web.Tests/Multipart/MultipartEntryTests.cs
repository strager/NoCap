using System.Collections.Generic;
using System.IO;
using System.Text;
using NoCap.Web.Multipart;
using NUnit.Framework;

namespace NoCap.Web.Tests.Multipart {
    [TestFixture]
    public class MultipartEntryTests {
        [Test]
        public void WriteHeadersWithoutBoundary() {
            var mpe = new MultipartEntry();
            mpe.Headers.Add(new MultipartHeader("Content-Type", "x-my/mime-type", null));

            using (var stream = new MemoryStream()) {
                mpe.WriteHeaders(stream);
                
                Assert.AreEqual(Encoding.ASCII.GetBytes("Content-Type: x-my/mime-type\r\n"), stream.ToArray());
            }
        }

        [Test]
        public void WriteHeadersWithBoundary() {
            var mpe = new MultipartEntry();
            mpe.Headers.Add(new MultipartHeader("Content-Type", "x-my/mime-type", new Dictionary<string, string> {
                { "boundary", "foobar" }
            }));

            using (var stream = new MemoryStream()) {
                mpe.WriteHeaders(stream);
                
                Assert.AreEqual(Encoding.ASCII.GetBytes("Content-Type: x-my/mime-type; boundary=foobar\r\n"), stream.ToArray());
            }
        }

        [Test]
        public void WriteContents() {
            var mpe = new MultipartEntry();

            using (var stream = new MemoryStream()) {
                mpe.WriteContents(stream);

                Assert.AreEqual(new byte[] { }, stream.ToArray());
            }
        }
    }
}
