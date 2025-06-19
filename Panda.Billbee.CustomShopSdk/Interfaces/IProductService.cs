using Panda.Billbee.CustomShopSdk.Models.Products;

namespace Panda.Billbee.CustomShopSdk.Interfaces;

/// <summary>
/// Defines operations for retrieving products in a Billbee Custom Shop integration.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a single product by its internal ID.
    /// Corresponds to the HTTP GET Action=GetProduct endpoint.
    /// </summary>
    /// <param name="productId">Internal ID of the product to retrieve.</param>
    /// <returns>The <see cref="Product"/> or null if not found.</returns>
    Task<Product?> GetProductAsync(string productId);
    /// <summary>
    /// Retrieves a paged list of products.
    /// Corresponds to the HTTP GET Action=GetProducts endpoint.
    /// </summary>
    /// <param name="page">Page number of the data retrieval; default is 1.</param>
    /// <param name="pageSize">Maximum number of records per page; default is 100.</param>
    /// <returns>A <see cref="ProductResponse"/> containing paging info and products.</returns>
    Task<ProductResponse> GetProductsAsync(int page, int pageSize);
}