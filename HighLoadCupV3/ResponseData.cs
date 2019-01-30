namespace HighLoadCupV3
{
    public partial class CustomRequestHandler
    {
        public class ResponseData
        {
            public ResponseData(int statusCode, string content)
            {
                StatusCode = statusCode;
                Content = content;
            }

            public int StatusCode { get; set; }
            public string Content { get; set; }
        }
    }
}