// http://ferozedaud.blogspot.com/2010/03/multipart-form-upload-helper.html

using System;
using System.IO;

namespace NoCap.WebHelpers
{
    /// <summary>
    /// MimePart
    /// Abstract class for all MimeParts
    /// </summary>
    abstract class MimePart
    {
        public string Name { get; set; }

        public abstract string ContentDisposition { get; }

        public abstract string ContentType { get; }

        public abstract void CopyTo(Stream stream);

        public String Boundary
        {
            get;
            set;
        }
    }
}
