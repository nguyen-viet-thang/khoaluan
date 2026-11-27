using System.ComponentModel.DataAnnotations;
using PopPop.Data.Entities.Enums;

namespace PopPop.Models;

public sealed class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
    [StringLength(256, MinimumLength = 2, ErrorMessage = "Tên người nhận phải từ 2 đến 256 ký tự.")]
    [Display(Name = "Tên người nhận")]
    public string RecipientName { get; set; } = string.Empty;

    /// <summary>
    /// Vietnamese mobile number: 0 or +84 followed by a 3/5/7/8/9 prefix and 8 digits.
    /// Spaces or dots between digits are allowed and stripped when the order is saved.
    /// </summary>
    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [RegularExpression(@"^\s*(0|\+84[\s.]?)[35789](?:[\s.]?\d){8}\s*$",
        ErrorMessage = "Số điện thoại phải là số di động Việt Nam hợp lệ (VD: 0901234567).")]
    [StringLength(20)]
    [Display(Name = "Số điện thoại")]
    public string RecipientPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
    [StringLength(500, MinimumLength = 10,
        ErrorMessage = "Vui lòng ghi rõ địa chỉ: số nhà, đường, phường/xã, quận/huyện, tỉnh/thành phố.")]
    [Display(Name = "Địa chỉ giao hàng")]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
    [Display(Name = "Phương thức thanh toán")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;

    /// <summary>False when the VNPAY gateway is not configured: only COD is offered.</summary>
    public bool VnPayAvailable { get; set; }
    public int VnPayTimeoutMinutes { get; set; }

    public IReadOnlyList<CartLineViewModel> Items { get; set; } = [];
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public decimal ShippingFee { get; set; }
    public decimal FreeShippingThreshold { get; set; }
    public decimal TotalAmount => Subtotal + ShippingFee;

    /// <summary>How much more the customer needs to buy to get free shipping (0 when already free).</summary>
    public decimal AmountToFreeShipping =>
        ShippingFee > 0 && FreeShippingThreshold > 0 ? FreeShippingThreshold - Subtotal : 0m;
}
