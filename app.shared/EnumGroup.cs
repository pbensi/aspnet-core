namespace app.shared
{
    public class EnumGroup
    {
        public enum RequestMethod
        {
            None = 0,
            Post = 1,
            Put = 2,
            Delete = 3,
            Get = 4,
            Options = 5,
        }

        public enum MessageType
        {
            None = 0,
            Information = 1,
            Success = 2,
            Warning = 3,
            Error = 4,
        }

        public enum HostingEnvironment
        {
            Development = 1,
            Production = 2
        }
    }
}
