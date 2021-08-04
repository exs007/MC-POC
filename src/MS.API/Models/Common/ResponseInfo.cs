using System.Collections.Generic;

namespace MS.API.Models.Common
{
    /// <summary>
    /// Common response structure
    /// </summary>
    /// <typeparam name="T">Result data type</typeparam>
    public class ResponseInfo<T>
    {
        public ResponseInfo(T data)
        {
            Success = true;
            Data = data;
            Messages = new List<MessageInfo>();
        }

        public ResponseInfo(string message) : this(new List<MessageInfo> {new(null, message)})
        {
        }

        public ResponseInfo(IEnumerable<MessageInfo> messages)
        {
            Success = false;
            Data = default;
            Messages = messages;
        }

        public bool Success { get; set; }
        public T Data { get; set; }
        public IEnumerable<MessageInfo> Messages { get; set; }

        public static implicit operator ResponseInfo<T>(T value)
        {
            return new(value);
        }
    }
}