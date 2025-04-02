using RefactorThis.Persistence.Models;

namespace RefactorThis.Persistence.Interfaces
{
    public interface IInvoiceRepository
    {
        /// <summary>
        /// Retrieves an invoice based on the provided reference.
        /// </summary>
        /// <param name="reference">The reference of the invoice to retrieve.</param>
        /// <returns>The matching invoice, or null if not found.</returns>
        Invoice GetInvoice(string reference);

        /// <summary>
        /// Saves the provided invoice to the database.
        /// </summary>
        /// <param name="invoice">The invoice to save.</param>
        void SaveInvoice(Invoice invoice);

        /// <summary>
        /// Adds a new invoice to the repository.
        /// </summary>
        /// <param name="invoice">The invoice to add.</param>
        void Add(Invoice invoice);
    }
}
