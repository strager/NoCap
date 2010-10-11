// http://ferozedaud.blogspot.com/2010/03/multipart-form-upload-helper.html

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NoCap.Library.WebHelpers
{
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
