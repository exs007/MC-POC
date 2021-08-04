namespace MS.API.Models.Common
{
    /// <summary>
    /// Message info. Contains field and message, field can be omitted
    /// </summary>
    public class MessageInfo
    {
        public MessageInfo(string field, string message)
        {
            Field = field;
            Message = message;
        }

        public string Field { get; }
        public string Message { get; }
    }
}