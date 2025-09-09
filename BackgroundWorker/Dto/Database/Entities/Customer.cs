using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pro4Soft.BackgroundWorker.Dto.Database.Entities.Base;

namespace Pro4Soft.BackgroundWorker.Dto.Database.Entities;

public class Customer : P4WStateEntity
{
    public Guid ClientId { get; set; }

    [Required]
    public string Code { get; set; }

    public string CompanyName { get; set; }

    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Description { get; set; }

    public string Info1 { get; set; }
    public string Info2 { get; set; }
    public string Info3 { get; set; }
    public string Info4 { get; set; }
    public string Info5 { get; set; }
    public string Info6 { get; set; }
    public string Info7 { get; set; }
    public string Info8 { get; set; }
    public string Info9 { get; set; }
    public string Info10 { get; set; }

    [ForeignKey(nameof(Pickticket.CustomerId))]
    public virtual ICollection<Pickticket> PickTickets { get; set; } = new List<Pickticket>();
}