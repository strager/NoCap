// http://ferozedaud.blogspot.com/2010/03/multipart-form-upload-helper.html
// Modified by <strager.nds@gmail.com>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace NoCap.WebHelpers
{
    /// <summary>
    /// Helper class to aid in uploading multipart
    /// entities to HTTP web endpoints.
    /// </summary>
    class MultipartHelper
    {
        private static Random random = new Random(Environment.TickCount);

        private List<NameValuePart> formData = new List<NameValuePart>();
        private FilesCollection files = null;
        private MemoryStream bufferStream = new MemoryStream();
        private string boundary;

        public String Boundary { get { return boundary; } }

        public static String GetBoundary()
        {
            return Environment.TickCount.ToString("X");
        }

        public MultipartHelper()
        {
            this.boundary = GetBoundary();
        }

        public void Add(NameValuePart part)
        {
            this.formData.Add(part);
            part.Boundary = boundary;
        }

        public void Add(FilePart part)
        {
            if (files == null)
            {
                files = new FilesCollection();
            }

            this.files.Add(part);
        }

        public void LoadInto(Stream stream)
        {
            // first, serialize the form data
            foreach (NameValuePart part in this.formData)
            {
                part.CopyTo(stream);
            }

            // serialize the files.
            if (this.files != null)
            {
                this.files.CopyTo(stream);

                if (this.files.Count > 0)
                {
                    // add the terminating boundary.
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("--{0}", this.Boundary).Append("\r\n");
                    byte [] buffer = Encoding.ASCII.GetBytes(sb.ToString());
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// Helper class that encapsulates all file uploads
        /// in a mime part.
        /// </summary>
        class FilesCollection : MimePart
        {
            private List<FilePart> files;

            public FilesCollection()
            {
                this.files = new List<FilePart>();
                this.Boundary = MultipartHelper.GetBoundary();
            }

            public int Count
            {
                get { return this.files.Count; }
            }

            public override string ContentDisposition
            {
                get
                {
                    return String.Format("form-data; name=\"{0}\"", this.Name);
                }
            }

            public override string ContentType
            {
                get { return String.Format("multipart/mixed; boundary={0}", this.Boundary); }
            }

            public override void CopyTo(Stream stream)
            {
                // serialize the headers
                StringBuilder sb = new StringBuilder(128);
                sb.Append("Content-Disposition: ").Append(this.ContentDisposition).Append("\r\n");
                sb.Append("Content-Type: ").Append(this.ContentType).Append("\r\n");
                sb.Append("\r\n");
                sb.AppendFormat("--{0}", this.Boundary).Append("\r\n");

                byte[] headerBytes = Encoding.ASCII.GetBytes(sb.ToString());
                stream.Write(headerBytes, 0, headerBytes.Length);
                foreach (FilePart part in files)
                {
                    part.Boundary = this.Boundary;
                    part.CopyTo(stream);
                }
            }

            public void Add(FilePart part)
            {
                this.files.Add(part);
            }
        }
    }
}
