namespace USSDMiddleware.Core.Models.Bills
{
    public class CategoriesResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public Category[] Data { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public object Description { get; set; }
    }

}
