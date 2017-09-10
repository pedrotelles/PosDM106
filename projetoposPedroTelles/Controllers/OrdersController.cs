using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using projetoposPedroTelles.Models;
using Microsoft.AspNet.Identity;
using projetoposPedroTelles.CRMClient;

namespace projetoposPedroTelles.Controllers
{
    [Authorize]
    public class OrdersController : ApiController
    {

        private projetoposPedroTellesContext db = new projetoposPedroTellesContext();

        // GET: api/Orders
        [Authorize(Roles = "ADMIN")]
        public List<Order> GetOrders()
        {
            return db.Orders.ToList();
        }

        // GET: api/Orders/5
        
        [ResponseType(typeof(Order))]
        public IHttpActionResult GetOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }
            if(User.IsInRole("USER"))
            {

                string email = order.userEmail;
                if(email == User.Identity.Name)
                {
                    return Ok(order);
                }

            }
            else if (User.IsInRole("ADMIN")) 
            {
                return Ok(order);
            }
            return BadRequest();
        }

        [HttpPut]
        [Route("cep")]
        public IHttpActionResult ObtemFreteData(int id)
        {
            Order order = db.Orders.Find(id);
            CRMRestClient crmClient = new CRMRestClient();
            Customer customer = crmClient.GetCustomerByEmail(User.Identity.Name);
            if (customer != null)
            {
                return Ok(customer.zip);
            }
            else
            {
                return BadRequest("Falha	ao	consultar	o	CRM");
            }
        }


        //GET: api/Orders/byemail=...
        [Authorize(Roles ="ADMIN, USER")]
        [ResponseType(typeof(List<Order>))]
        [HttpGet]
        [Route("byemail")]
        public IHttpActionResult GetOrdersByEmail(String email)
        {
            if (User.IsInRole("USER"))
            {
                if(User.Identity.Name == email)
                {
                    return Ok(db.Orders.Where(Order => Order.userEmail == email).ToList());
                }
                
            }
            else if (User.IsInRole("ADMIN"))
            {
                return Ok(db.Orders.Where(Order => Order.userEmail == email).ToList());
            }
            return BadRequest();
            
            
        }

        // PUT: api/Orders/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutOrder(int id)
        {
            if (ModelState.IsValid)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    var order = db.Orders.Find(id);
                    if (User.IsInRole("ADMIN"))
                    {
                        if(order.precoTotal == 0)
                        {
                            order.status = "fechado";
                            db.SaveChanges();
                        }
                       
                    }
                    else if (User.IsInRole("USER"))
                    {
                        if (User.Identity.Name == order.userEmail)
                        {
                            if(order.precoFrete == 0)
                            {
                                order.status = "fechado";
                                db.SaveChanges();
                            }
                            
                        }
                    }
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }
            
            

            
        }

        // POST: api/Orders
        [ResponseType(typeof(Order))]
        public IHttpActionResult PostOrder(Order order)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            order.dataInicial = DateTime.Today;
            order.status = "novo";
            order.pesoTotal = 0;
            order.precoFrete = 0;
            order.precoTotal = 0;
            db.Orders.Add(order);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = order.Id }, order);
        }

        // DELETE: api/Orders/5
        [ResponseType(typeof(Order))]
        public IHttpActionResult DeleteOrder(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }
            if(User.IsInRole("ADMIN"))
            {
                db.Orders.Remove(order);
                db.SaveChanges();

                return Ok(order);
            }
            else if (User.IsInRole("User"))
            {
                if (User.Identity.Name == order.userEmail)
                {
                    db.Orders.Remove(order);
                    db.SaveChanges();

                    return Ok(order);
                }
            }
            return BadRequest();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool OrderExists(int id)
        {
            return db.Orders.Count(e => e.Id == id) > 0;
        }
    }
}