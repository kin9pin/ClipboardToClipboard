using System;

namespace ClipboardToClipboard
{
    public class Buffer : IEquatable<Buffer>
    {
        private readonly string shortValue;
        private readonly string longValue;

        public Buffer(string longValue)
        {
            this.longValue = longValue;

            if (longValue.Length > 150)
                this.shortValue = longValue.Replace("\n", "\t").Replace("\r", "\t").Substring(0, 150);
            else
                this.shortValue = longValue.Replace("\n", "\t").Replace("\r", "\t");

            this.Comment = String.Empty;
        }

        public Buffer(string longValue, string comment)
            : this(longValue)
        {
            this.Comment = comment;
        }

        public string ShortValue
        {
            get
            {
                return shortValue;
            }
        }

        public string LongValue
        {
            get
            {
                return longValue;
            }
        }

        public string Comment
        {
            get;
            set;
        }

        public bool Equals(Buffer other)
        {
            return this.longValue.Equals(other.longValue);
        }

        public override string ToString()
        {
            return this.shortValue;
        }
    }
}
