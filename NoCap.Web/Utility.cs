using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace NoCap.Web {
    public static class Utility {
        private static readonly Random Random = new Random(Environment.TickCount);

        public static string GetRandomBoundary() {
            return Random.Next().ToString("X");
        }

        public static string LineSeparator {
            get {
                return "\r\n";
            }
        }

        public static Encoding Encoding {
            get {
                return Encoding.ASCII;
            }
        }

        public static void WriteBoundary(Stream stream, string boundary) {
            if (stream == null) {
                throw new ArgumentNullException("stream");
            }

            if (boundary == null) {
                throw new ArgumentNullException("boundary");
            }

            // Do not dispose (because it closes the stream)
            var writer = new StreamWriter(stream, Encoding);

            writer.Write("--{0}", boundary);
            writer.Write(LineSeparator);

            writer.Flush();
        }

        public static string ToQueryString(IDictionary<string, string> pairs) {
            // http://stackoverflow.com/questions/829080/how-to-build-a-query-string-for-a-url-in-c/829138#829138
            return String.Join("&", pairs.Select(
                (pair) => String.Format("{0}={1}", HttpUtility.UrlEncode(pair.Key), HttpUtility.UrlEncode(pair.Value))
            ));
        }
    }
}