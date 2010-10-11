// http://ferozedaud.blogspot.com/2010/03/multipart-form-upload-helper.html

using System;
using System.IO;
using System.Text;

namespace NoCap.WebHelpers
{
    class FilePart : MimePart
    {
        private Stream input;
        private String contentType;

        public FilePart(Stream input, String name, String contentType)
        {
            this.input = input;
            this.contentType = contentType;
            this.Name = name;
        }

        public override void CopyTo(Stream stream)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Content-Disposition: {0}", this.ContentDisposition);
            if (this.Name != null)
                sb.Append("; ").AppendFormat("name=\"{0}\"", this.Name);
            if (this.FileName != null)
                sb.Append("; ").AppendFormat("filename=\"{0}\"", this.FileName);
            sb.Append("\r\n");
            sb.AppendFormat(this.ContentType);
            sb.Append("\r\n");
            sb.Append("\r\n");

            // serialize the header data.
            byte[] buffer = Encoding.ASCII.GetBytes(sb.ToString());
            stream.Write(buffer, 0, buffer.Length);

            // send the stream.
            byte[] readBuffer = new byte[1024];
            int read = input.Read(readBuffer, 0, readBuffer.Length);
            while (read > 0)
            {
                stream.Write(readBuffer, 0, read);
                read = input.Read(readBuffer, 0, readBuffer.Length);
            }

            // write the terminating boundary
            sb.Length = 0;
            sb.Append("\r\n");
            sb.AppendFormat("--{0}", this.Boundary);
            sb.Append("\r\n");
            buffer = Encoding.ASCII.GetBytes(sb.ToString());
            stream.Write(buffer, 0, buffer.Length);
        }

        public override string ContentDisposition
        {
            get { return "file"; }
        }

        public override string ContentType
        {
            get { return String.Format("content-type: {0}", this.contentType); }
        }

        public String FileName { get; set; }
    }
}
