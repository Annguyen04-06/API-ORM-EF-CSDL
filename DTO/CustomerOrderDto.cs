namespace btap_api_orm.DTO
{
    public class CustomerOrderDto
    {
        public int OrderId { get; set; }
        public string OrderDate { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
