using RefactorThis.Persistence.Interfaces;
using RefactorThis.Persistence.Models;

namespace RefactorThis.Persistence
{
    public class InvoiceRepository : IInvoiceRepository
    {
        // Private Fields
        private Invoice _invoice;

        // Public Methods

        /// <summary>
        /// Retrieves an invoice based on the provided reference.
        /// </summary>
        /// <param name="reference">The reference of the invoice to retrieve.</param>
        /// <returns>The matching invoice, or null if not found.</returns>
        public Invoice GetInvoice(string reference)
        {
            return _invoice;
        }

        /// <summary>
        /// Saves the provided invoice to the database.
        /// </summary>
        /// <param name="invoice">The invoice to save.</param>
        public void SaveInvoice(Invoice invoice)
        {
            // Logic to save the invoice to the database
        }

        /// <summary>
        /// Adds a new invoice to the repository.
        /// </summary>
        /// <param name="invoice">The invoice to add.</param>
        public void Add(Invoice invoice)
        {
            _invoice = invoice;
        }
    }
}
