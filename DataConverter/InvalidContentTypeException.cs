namespace DataConverter
{
    public class InvalidContentTypeException : Exception
    {
        public InvalidContentTypeException()
        {
        }

        public InvalidContentTypeException(string message)
            : base(message)
        {
        }

        public InvalidContentTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
