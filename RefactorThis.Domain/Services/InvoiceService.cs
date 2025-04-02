using System;
using System.Linq;
using RefactorThis.Domain.DTOs;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Models;
using RefactorThis.Persistence.Models.Enums;

namespace RefactorThis.Domain
{
    /// <summary>
    /// Service class for handling invoice-related operations.
    /// </summary>
    public class InvoiceService
    {
        private readonly InvoiceRepository _invoiceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoiceService"/> class.
        /// </summary>
        /// <param name="invoiceRepository">The repository for accessing invoices.</param>
        public InvoiceService(InvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        /// <summary>
        /// Processes a payment for an invoice.
        /// </summary>
        /// <param name="payment">The payment to process.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        public ProcessInvoiceResultDTO ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);

            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            if (invoice.Amount == 0)
            {
                return HandleZeroAmountInvoice(invoice);
            }

            if (invoice.Payments != null && invoice.Payments.Any())
            {
                return HandleExistingPayments(invoice, payment);
            }

            return HandleNewPayment(invoice, payment);
        }

        /// <summary>
        /// Handles invoices with an amount of 0.
        /// </summary>
        /// <param name="invoice">The invoice to handle.</param>
        /// <returns>A result indicating no payment is needed or throws an exception if the state is invalid.</returns>
        private ProcessInvoiceResultDTO HandleZeroAmountInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
            {
                return new ProcessInvoiceResultDTO
                {
                    Message = "no payment needed",
                    IsSuccess = true,
                };
            }

            throw new InvalidOperationException(
                "The invoice is in an invalid state, it has an amount of 0 and it has payments."
            );
        }

        /// <summary>
        /// Handles invoices with existing payments.
        /// </summary>
        /// <param name="invoice">The invoice to handle.</param>
        /// <param name="payment">The payment to process.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        private ProcessInvoiceResultDTO HandleExistingPayments(Invoice invoice, Payment payment)
        {
            var totalPaid = invoice.Payments.Sum(x => x.Amount);

            if (totalPaid != 0 && invoice.Amount == totalPaid)
            {
                return new ProcessInvoiceResultDTO
                {
                    Message = "invoice was already fully paid",
                    IsSuccess = true,
                };
            }

            // If the payment exceeds the remaining amount, return a failure message.
            if (totalPaid != 0 && payment.Amount > (invoice.Amount - invoice.AmountPaid))
            {
                return new ProcessInvoiceResultDTO
                {
                    Message = "the payment is greater than the partial amount remaining",
                    IsSuccess = false,
                };
            }

            // If the payment completes the invoice, finalize the payment.
            if ((invoice.Amount - invoice.AmountPaid) == payment.Amount)
            {
                return FinalizePayment(
                    invoice,
                    payment,
                    "final partial payment received, invoice is now fully paid"
                );
            }

            // Otherwise, finalize the payment as a partial payment.
            return FinalizePayment(
                invoice,
                payment,
                "another partial payment received, still not fully paid"
            );
        }

        /// <summary>
        /// Handles invoices with no prior payments.
        /// </summary>
        /// <param name="invoice">The invoice to handle.</param>
        /// <param name="payment">The payment to process.</param>
        /// <returns>A result indicating the success or failure of the operation.</returns>
        private ProcessInvoiceResultDTO HandleNewPayment(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
            {
                return new ProcessInvoiceResultDTO
                {
                    Message = "the payment is greater than the invoice amount",
                    IsSuccess = false,
                };
            }

            if (invoice.Amount == payment.Amount)
            {
                return FinalizePayment(invoice, payment, "invoice is now fully paid");
            }

            return FinalizePayment(invoice, payment, "invoice is now partially paid");
        }

        /// <summary>
        /// Finalizes a payment by updating the invoice and saving it.
        /// </summary>
        /// <param name="invoice">The invoice to update.</param>
        /// <param name="payment">The payment to process.</param>
        /// <param name="message">The message to include in the result.</param>
        /// <returns>A result indicating the success of the operation.</returns>
        private ProcessInvoiceResultDTO FinalizePayment(
            Invoice invoice,
            Payment payment,
            string message
        )
        {
            invoice.AmountPaid += payment.Amount;

            if (invoice.Type == InvoiceType.Commercial)
            {
                invoice.TaxAmount += payment.Amount * 0.14m;
            }

            invoice.Payments.Add(payment);
            invoice.Save();

            return new ProcessInvoiceResultDTO { Message = message, IsSuccess = true };
        }
    }
}
