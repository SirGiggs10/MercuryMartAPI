using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Dtos.HomePage;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Repositories
{
    public class HomePageRepository : IHomePageRepository
    {
        private readonly DataContext _dataContext;

        public HomePageRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<ReturnResponse> GetHomePage()
        {
            var homePageResponse = new HomePageResponse();
            var allProducts = _dataContext.Product;
            var allProductsGrouped = await allProducts.GroupBy(a => new { a.CategoryId, a.ProductName }).Select(b => new ProductCard() { CategoryId = b.Key.CategoryId, ProductName = b.Key.ProductName }).ToListAsync();
            var allProductsNotAssigned = await allProducts.Where(a => !a.AssignedTo.HasValue).GroupBy(a => new { a.CategoryId, a.ProductName }).Select(b => new ProductCard() { CategoryId = b.Key.CategoryId, ProductName = b.Key.ProductName, QuantityInStock = b.Count() }).ToListAsync();
            homePageResponse.ProductCards = allProductsNotAssigned;

            homePageResponse.ProductCards = allProductsGrouped.GroupJoin(homePageResponse.ProductCards, left => new { left.CategoryId, left.ProductName}, right => new { right.CategoryId, right.ProductName }, (a, b) => new
            {
                TableA = a,
                TableB = b
            }).SelectMany(p => p.TableB.DefaultIfEmpty(), (x, y) => new
            {
                TableAA = x.TableA,
                TableBB = y
            }).Select(c => new ProductCard()
            {
                CategoryId = c.TableAA.CategoryId,
                ProductName = c.TableAA.ProductName,
                QuantityInStock = (c.TableBB == null) ? c.TableAA.QuantityInStock : c.TableBB.QuantityInStock
            }).ToList();

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = homePageResponse
            };
        }
    }
}
