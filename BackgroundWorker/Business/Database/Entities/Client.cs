using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pro4Soft.BackgroundWorker.Business.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Business.Database.Entities;

public class Client : P4WEntity
{
    [Required]
    public string Name { get; set; }

    public string SsccCompanyId { get; set; }

    public string Description { get; set; }

    [ForeignKey(nameof(Customer.ClientId))]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    [ForeignKey(nameof(Vendor.ClientId))]
    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();

    [ForeignKey(nameof(Product.ClientId))]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Warehouse : P4WEntity
{
    [Required]
    public string Code { get; set; }
}