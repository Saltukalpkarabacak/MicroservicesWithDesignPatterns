using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrdersController(AppDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreateDto)
        {
            var Order = new Models.Order {

                BuyerId = orderCreateDto.BuyerId,
                Status = OrderStatus.Suspend,
                Adress = new Adress { 
                    Line = orderCreateDto.Address.Line,
                    Province = orderCreateDto.Address.Province,
                    District = orderCreateDto.Address.District,                    
                },
                CreatedDate = DateTime.Now
             };

            orderCreateDto.orderItems.ForEach(x => {

                Order.Items.Add( new OrderItem()
                {
                    Price = x.Price,
                    ProductId = x.ProductId, 
                    Count = x.Count,
                });
            
            });

            await _context.AddAsync(Order);

            await _context.SaveChangesAsync();

            var OrderCreatedEvent = new OrderCreatedEvent()
            {
                BuyerId = orderCreateDto.BuyerId,
                OrderId = Order.Id,
                Payment = new PaymentMessage{ 
                    CardName = orderCreateDto.payment.CardName,
                    CardNumber = orderCreateDto.payment.CardNumber,
                    CVV = orderCreateDto.payment.CVV,
                    Expiration = orderCreateDto.payment.Expiration,
                    TotalPrice = orderCreateDto.orderItems.Sum(x => x.Price * x.Count),
                }
            };

            orderCreateDto.orderItems.ForEach(x => {
                OrderCreatedEvent.orderItems.Add(new OrderItemMessage() {
                    Count = x.Count, 
                    ProductId=x.ProductId

                });
            });

            await _publishEndpoint.Publish(OrderCreatedEvent);

            return Ok();  
        }
    }
}
