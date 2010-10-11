// http://ferozedaud.blogspot.com/2010/03/multipart-form-upload-helper.html

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace NoCap.WebHelpers
{
    class NameValuePart : MimePart
    {
        private NameValueCollection nameValues;

        public NameValuePart(NameValueCollection nameValues)
        {
            this.nameValues = nameValues;
        }

        public override void CopyTo(Stream stream)
        {
            string boundary = this.Boundary;
            StringBuilder sb = new StringBuilder();

            foreach (object element in this.nameValues.Keys)
            {
                sb.AppendFormat("--{0}", boundary);
                sb.Append("\r\n");
                sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\";", element);
                sb.Append("\r\n");
                sb.Append("\r\n");
                sb.Append(this.nameValues[element.ToString()]);

                sb.Append("\r\n");

            }

            sb.AppendFormat("--{0}", boundary);
            sb.Append("\r\n");

            //Trace.WriteLine(sb.ToString());
            byte [] data = Encoding.ASCII.GetBytes(sb.ToString());
            stream.Write(data, 0, data.Length);
        }

        public override string ContentDisposition
        {
            get { return "form-data"; }
        }

        public override string ContentType
        {
            get { return String.Empty; }
        }
    }
}
