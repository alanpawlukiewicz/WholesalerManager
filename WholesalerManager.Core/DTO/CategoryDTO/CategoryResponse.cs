using WholesalerManager.Core.Domain.Entities;

namespace WholesalerManager.Core.DTO.CategoryDTO
{
    public class CategoryResponse
    {
        public Guid CategoryID { get; set; }
        public string? CategoryName { get; set; }
    }

    public static class CategoryExtensions
    {
        /// <summary>
        /// Extension method converting a Category entity to a CategoryResponse DTO.
        /// </summary>
        /// <param name="category">The Category entity to convert.</param>
        /// <returns>A CategoryResponse DTO.</returns>
        public static CategoryResponse ToCategoryResponse(this Category category)
        {
            return new CategoryResponse()
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName
            };
        }
    }
}
