using System.Collections.Generic;
using RefactorThis.Persistence.Models.Enums;

namespace RefactorThis.Persistence.Models
{
    public class Invoice
    {
        // Private Fields
        private readonly InvoiceRepository _repository;

        // Constructor
        public Invoice(InvoiceRepository repository)
        {
            _repository = repository;
        }

        // Public Properties
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }
        public InvoiceType Type { get; set; }

        // Public Methods
        public void Save()
        {
            _repository.SaveInvoice(this);
        }
    }
}
