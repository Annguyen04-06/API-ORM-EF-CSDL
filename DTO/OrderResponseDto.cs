namespace btap_api_orm.DTO
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string OrderDate { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }
}
