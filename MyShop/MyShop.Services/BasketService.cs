using MyShop.Core.Contracts;
using MyShop.Core.Models;
using MyShop.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Services
{
   public class BasketService : IBasketService
    {
        public IRepository<Product> ProductsContext;
        public IRepository<Basket> BasketContext;

        public const string BasketSessionName = "eCommerceBasket";

        public BasketService(IRepository<Product> productsContext,IRepository<Basket> basketContext)
        {
            this.BasketContext = basketContext;
            this.ProductsContext = productsContext;


        }


        private Basket GetBasket (HttpContextBase httpContext,bool createIfNull)
        {
            HttpCookie cookie = httpContext.Request.Cookies.Get(BasketSessionName);

            Basket basket = new Basket();

            if (cookie != null)
            {
                string basketId = cookie.Value;
                if (!string.IsNullOrEmpty(basketId))
                {
                    basket = BasketContext.Find(basketId);
                }
                else
                {
                    if (createIfNull)
                    {
                        basket = CreateNewBasket(httpContext);
                    }
                }

            }

            else
            {
                if (createIfNull)
                {
                    basket = CreateNewBasket(httpContext);
                }

            }

            return basket;

        }

        private Basket CreateNewBasket(HttpContextBase httpContext)
        {
            Basket basket = new Basket();
            BasketContext.Insert(basket);
            BasketContext.Commit();

            HttpCookie cookie = new HttpCookie(BasketSessionName);
            cookie.Value = basket.Id;
            cookie.Expires = DateTime.Now.AddDays(1);
            httpContext.Response.Cookies.Add(cookie);

            return basket;
        }

        public void AddToBasket(HttpContextBase httpContext, string productId)
        {
            Basket basket = GetBasket(httpContext,true);
            BasketItem item = basket.BasketItems.FirstOrDefault(i=>i.ProductId == productId);

            if (item == null)
            {
                item = new BasketItem() { BasketId = basket.Id, ProductId = productId, Quantity = 1 };
                basket.BasketItems.Add(item);


            }

         
            else
            {
                item.Quantity += 1;

            }


            BasketContext.Commit();



        }


        public void RemoveFromBasket(HttpContextBase httpContext , string itemId )
        {
            Basket basket = GetBasket(httpContext,false);
            BasketItem itemToRemove = basket.BasketItems.FirstOrDefault(i=>i.Id==itemId);
            if (itemToRemove!=null)
            {
                basket.BasketItems.Remove(itemToRemove);
                BasketContext.Commit();
            }

        }




        public List<BasketItemViewModel> GetBasketItems(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext,false);

            if (basket != null)
            {
                var result = (from b in basket.BasketItems
                              join p in ProductsContext.Collection()
                              on b.ProductId equals p.Id
                              select new BasketItemViewModel()
                              {
                                  Id = b.Id,
                                  Quantity = b.Quantity,
                                  ProductName = p.Name,
                                  Price = p.Price,
                                  Image = p.Image
                              }


                              ).ToList();
                return result;


            }
            else
            {
                return new List<BasketItemViewModel>();



            }        

        }

        public BasketSummaryViewModel GetBasketSummary(HttpContextBase httpContext)
        {
            Basket basket = GetBasket(httpContext,false);
            BasketSummaryViewModel model = new BasketSummaryViewModel(0,0);

            if (basket != null)
            {
                int? basketCount = (from item in basket.BasketItems
                                    select item.Quantity).Sum();
                decimal? basketTotal = (from item in basket.BasketItems
                                        join p in ProductsContext.Collection()
                                        on item.ProductId equals p.Id
                                        select item.Quantity * p.Price).Sum();

                model.BasketCount = basketCount ?? 0;
                model.BasketTotal = basketTotal ?? decimal.Zero;
                return model;


            }
            else { return model; }


        }



    }
}
