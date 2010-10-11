// http://ferozedaud.blogspot.com/2010/03/multipart-form-upload-helper.html
// Modified by <strager.nds@gmail.com>

using System;
using System.Collections.Generic;
using System.IO;

namespace NoCap.WebHelpers
{
    /// <summary>
    /// Helper class to aid in uploading multipart
    /// entities to HTTP web endpoints.
    /// </summary>
    public class MultipartHelper
    {
        private static Random random = new Random(Environment.TickCount);

        private readonly ICollection<MimePart> formData = new List<MimePart>();
        private readonly string boundary;

        public String Boundary { get { return boundary; } }

        public static String GetBoundary()
        {
            return Environment.TickCount.ToString("X");
        }

        public MultipartHelper()
        {
            this.boundary = GetBoundary();
        }

        public void Add(MimePart part)
        {
            this.formData.Add(part);
            part.Boundary = boundary;
        }

        public void CopyTo(Stream stream)
        {
            foreach (var part in this.formData)
            {
                part.CopyTo(stream);
            }
        }
    }
}
