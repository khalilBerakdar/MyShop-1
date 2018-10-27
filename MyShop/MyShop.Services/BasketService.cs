using MyShop.Core.Contracts;
using MyShop.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyShop.Services
{
   public class BasketService
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










    }
}
