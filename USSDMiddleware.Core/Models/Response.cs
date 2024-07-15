namespace USSDMiddleware.Core.Models
{
    public class Response<T>
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public bool Succeeded { get; set; }
        public T Data { get; set; }
    }

    public class Response : Response<object>
    {
        public static Response<T> Create<T>(Func<T> func)
        {
            try
            {
                var result = func();
                return new Response<T>
                {
                    Data = result,
                    Succeeded = true,
                   // Message
                };
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                return new Response<T>
                {
                    Message = ex.Message,
                    Succeeded = false,
                };
            }
        }

        public static Response Create(Action action)
        {
            try
            {
                action();
                return new Response
                {
                    Succeeded = true,
                };
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                return new Response
                {
                    Message = ex.Message,
                    Succeeded = false,
                };
            }
        }
    }
}
