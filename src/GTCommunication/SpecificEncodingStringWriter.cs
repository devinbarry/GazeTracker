using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GTCommunication
{
    public class SpecificEncodingStringWriter : StringWriter
    {
        private Encoding _encoding;

        public SpecificEncodingStringWriter(Encoding encoding)
        {
            _encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return _encoding; }
        }
    }

}
