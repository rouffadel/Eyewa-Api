using System;
using System.Collections.Generic;

namespace Eyewa_new_api.DTOs
{
    public class UserPermissionDto
    {
        public bool Add { get; set; }
        public bool View { get; set; }
        public bool Delete { get; set; }
        public bool Edit { get; set; }
    }

    public class UserLoginResultDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool View { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Delete { get; set; }
        public object Result { get; set; }
    }

    public class LoginUserDetailDto
    {
        public int LoginID { get; set; }
        public int RoleID { get; set; }
        public string UserName { get; set; }
        public int StoreID { get; set; }
    }

    public class StoreDto
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; }
    }

    public class CategoryDto
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }

    public class BrandDto
    {
        public int BrandID { get; set; }
        public string BrandName { get; set; }
    }

    public class ProductDto
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal TaxPer { get; set; }
    }

    public class OrderLenseDto
    {
        public int OrderLenseID { get; set; }
        public string CategoryID { get; set; }
        public string Brand { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

    public class PrescriptionDetailDto
    {
        public string SPH { get; set; }
        public string CYL { get; set; }
        public string AXIS { get; set; }
        public string ADD { get; set; }
    }

    public class PrescriptionIpdDto
    {
        public string SphText { get; set; }
        public string CylText { get; set; }
        public string AxisText { get; set; }
        public string AddText { get; set; }
    }

    public class OrderLensesResponseDto
    {
        public List<OrderLenseDto> OrderLenses { get; set; } = new List<OrderLenseDto>();
        public List<PrescriptionDetailDto> PrescriptionDetails { get; set; } = new List<PrescriptionDetailDto>();
    }

    public class SalesManDto
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
    }

    public class UserDto
    {
        public int LoginID { get; set; }
        public string LoginName { get; set; }
    }

    public class SalesGridItemDto
    {
        public int SaleID { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNo { get; set; }
        public decimal NetTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public string PaymentMode { get; set; }
        public string CreatedDate { get; set; }
    }

    public class InvoiceDetailDto
    {
        public int SaleID { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNo { get; set; }
        public decimal GrossTotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal NetTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public string PaymentMode { get; set; }
        public string StoreName { get; set; }
        public string VatNo { get; set; }
        public List<InvoiceItemDto> Items { get; set; } = new List<InvoiceItemDto>();
    }

    public class InvoiceItemDto
    {
        public int SalesDetailID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public decimal Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxPer { get; set; }
    }
}
