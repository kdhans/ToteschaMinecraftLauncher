using System;
using System.Runtime.Serialization;

namespace ToteschaMinecraftLauncher.UpdatedScripts.Exceptions
{
    [Serializable]
    internal class ToteschaServerException : Exception
    {
        public ToteschaServerException()
        {
        }

        public ToteschaServerException(string? message) : base(message)
        {
        }

        public ToteschaServerException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ToteschaServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string StatusTextMessage { get; internal set; }
    }
}