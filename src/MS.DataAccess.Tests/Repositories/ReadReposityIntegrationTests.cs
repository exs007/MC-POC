using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MS.Core.Constants;
using MS.DataAccess.Repositories;
using MS.DataAccess.Tests.Entities;
using MS.DataAccess.Tests.Fixtures;
using MS.DataAccess.Tests.Specifications;
using Xunit;

namespace MS.DataAccess.Tests.Repositories
{
    public class ReadReposityIntegrationTests : IClassFixture<ReadReposityORMInMemoryFixture>
    {
        private readonly IReadRepository<Product> _productReadRepository;
        private readonly ServiceProvider _serviceProvider;
        private readonly ReadReposityORMInMemoryFixture _fixture;

        public ReadReposityIntegrationTests(ReadReposityORMInMemoryFixture fixture)
        {
            _fixture = fixture;
            _productReadRepository = fixture.ServiceProvider.GetService<IReadRepository<Product>>();
            _serviceProvider = fixture.ServiceProvider;
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_ToListAsync_ReturnTheSameProductsAsContext()
        {
            var expectedProducts =
                await _fixture.InMemoryContext.Set<Product>().OrderBy(p => p.ProductId).ToListAsync();

            var products = (await _productReadRepository.ToListAsync()).OrderBy(p => p.ProductId).ToList();

            AssertProductLists(expectedProducts, products);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_ToListAsyncWithFilter_ReturnTheSameFilteredProductsAsContext()
        {
            var firstProduct = await _fixture.InMemoryContext.Set<Product>().OrderBy(p => p.ProductId).FirstAsync();
            var lastProduct = await _fixture.InMemoryContext.Set<Product>().OrderBy(p => p.ProductId).LastAsync();
            Expression<Func<Product, bool>> filter = p =>
                p.Name == firstProduct.Name || p.ProductId == lastProduct.ProductId;
            var expectedProducts = await _fixture.InMemoryContext.Set<Product>().Where(filter)
                .OrderBy(p => p.ProductId).ToListAsync();

            var products = (await _productReadRepository.ToListAsync(filter)).OrderBy(p => p.ProductId).ToList();

            AssertProductLists(expectedProducts, products);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_ToListAsyncWithOrderBy_ReturnTheSameProductsAsContext()
        {
            var expectedProducts =
                await _fixture.InMemoryContext.Set<Product>().OrderBy(p => p.ProductId).ToListAsync();

            var products = await _productReadRepository.ToListAsync(orderBy: p => p.OrderBy(p => p.ProductId));

            AssertProductLists(expectedProducts, products);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_ToListAsyncWithRequestingSpecificPage_ReturnTheSameProductsAsContext()
        {
            var pageSize = 2;
            var pageIndex = 1;
            var expectedProducts = await _fixture.InMemoryContext.Set<Product>().OrderBy(p => p.ProductId)
                .Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();

            var products = await _productReadRepository.ToListAsync(orderBy: p => p.OrderBy(p => p.ProductId),
                pageIndex: pageIndex, pageSize: pageSize);

            AssertProductLists(expectedProducts, products);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_ToListAsyncWithoutIncludeSkus_ReturnTheSameProductsWithoutSkus()
        {
            var products = await _productReadRepository.ToListAsync(tracking: false);

            foreach (var product in products) Assert.Null(product.Skus);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_ToListAsyncWithIncludeSkus_ReturnTheSameProductsAndSkusAsContext()
        {
            var expectedProducts = await _fixture.InMemoryContext.Set<Product>().Include(p => p.Skus).ToListAsync();

            var products =
                await _productReadRepository.ToListAsync(includes: p => p.Include(pp => pp.Skus), tracking: false);

            AssertProductLists(expectedProducts, products, true);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_ToListAsyncWithSpecification_ReturnTheSameListAsFromContext()
        {
            var expectedProducts = await _fixture.InMemoryContext.Set<Product>().Include(p => p.Skus)
                .OrderBy(p => p.ProductId).Take(2).ToListAsync();

            var products =
                await _productReadRepository.ToListAsync(
                    new ProductWithSkusSpecification(expectedProducts.Select(p => p.ProductId)));

            AssertProductLists(expectedProducts, products, true);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_ToListAsyncWithSpecificationWithProjection_ReturnTheSameListAsFromContext()
        {
            var expectedProducts = await _fixture.InMemoryContext.Set<Product>().Select(p => new ProductShortList
            {
                ProductId = p.ProductId
            }).ToListAsync();

            var products = await _productReadRepository.ToListAsync(new ProductShortListSpecification());

            AssertProductLists(expectedProducts, products);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenProductsExist_CountAsync_ReturnTheSameCountAsContext()
        {
            var expected = await _fixture.InMemoryContext.Set<Product>().CountAsync();

            var actual = await _productReadRepository.CountAsync();

            Assert.Equal(expected, actual);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenTheGivenProductExists_FirstOrDefault_ReturnTheSameObjectAsContext()
        {
            var expected = await _fixture.InMemoryContext.Set<Product>().OrderBy(p => p.ProductId)
                .FirstOrDefaultAsync();

            var actual = await _productReadRepository.FirstOrDefaultAsync(orderBy: p => p.OrderBy(pp => pp.ProductId));

            Assert.Equal(expected.ProductId, actual.ProductId);
            Assert.Equal(expected.Name, actual.Name);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenTheGivenProductExists_AnyAsync_ReturnTrue()
        {
            var expected = await _fixture.InMemoryContext.Set<Product>().FirstOrDefaultAsync();

            var actual = await _productReadRepository.AnyAsync(p => p.ProductId == expected.ProductId);

            Assert.True(actual);
        }

        [Fact]
        [Trait(TestConstants.TEST_CATEGORY_LABEL, TestConstants.TEST_CATEGORY_INTEGRATION)]
        public async Task WhenTheGivenProductNotExists_AnyAsync_ReturnFalse()
        {
            var actual = await _productReadRepository.AnyAsync(p => p.ProductId == -1);

            Assert.False(actual);
        }

        private void AssertProductLists(ICollection<Product> expectedProducts, ICollection<Product> actualProducts,
            bool assertSkus = false)
        {
            var expectedProductsList = expectedProducts.ToList();
            var actualProductsList = actualProducts.ToList();
            Assert.Equal(expectedProductsList.Count, actualProductsList.Count);
            for (var i = 0; i < expectedProductsList.Count; i++)
            {
                Assert.Equal(expectedProductsList[i].ProductId, actualProductsList[i].ProductId);
                Assert.Equal(expectedProductsList[i].Name, actualProductsList[i].Name);
                if (assertSkus) AssertSkusLists(expectedProductsList[i].Skus, actualProductsList[i].Skus);
            }
        }

        private void AssertSkusLists(ICollection<Sku> expectedSkus, ICollection<Sku> actualSkus)
        {
            var expectedSkusList = expectedSkus.OrderBy(p => p.SkuId).ToList();
            var actualSkusList = actualSkus.OrderBy(p => p.SkuId).ToList();
            Assert.Equal(expectedSkusList.Count, actualSkusList.Count);
            for (var i = 0; i < expectedSkusList.Count; i++)
            {
                Assert.Equal(expectedSkusList[i].SkuId, actualSkusList[i].SkuId);
                Assert.Equal(expectedSkusList[i].Code, actualSkusList[i].Code);
            }
        }

        private void AssertProductLists(ICollection<ProductShortList> expectedProducts,
            ICollection<ProductShortList> actualProducts)
        {
            var expectedProductsList = expectedProducts.ToList();
            var actualProductsList = actualProducts.ToList();
            Assert.Equal(expectedProductsList.Count, actualProductsList.Count);
            for (var i = 0; i < expectedProductsList.Count; i++)
                Assert.Equal(expectedProductsList[i].ProductId, actualProductsList[i].ProductId);
        }
    }
}