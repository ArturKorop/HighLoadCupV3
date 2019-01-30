namespace HighLoadCupV3
{
    public class ResponseData
    {
        public ResponseData(int statusCode, object content)
        {
            StatusCode = statusCode;
            Content = content;
        }

        public int StatusCode { get; set; }
        public object Content { get; set; }
    }
}