﻿using System;
using System.IO;
using System.Text;

namespace NoCap.Web.Multipart {
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

        public static long GetBoundaryByteCount(string boundary) {
            return Encoding.GetByteCount(string.Format("--{0}" + LineSeparator, boundary));
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
    }
}