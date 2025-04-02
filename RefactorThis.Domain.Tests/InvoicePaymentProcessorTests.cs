﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Domain.DTOs;
using RefactorThis.Persistence;
using RefactorThis.Persistence.Models;
using RefactorThis.Persistence.Models.Enums;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {
            var repo = new InvoiceRepository();

            Invoice invoice = null;
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment();
            var failureMessage = "";

            try
            {
                var result = paymentProcessor.ProcessPayment(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.AreEqual("There is no invoice matching this payment", failureMessage);
        }

        [Test]
        public void ProcessPayment_Should_ReturnSuccessDTO_When_NoPaymentNeeded()
        {
            var repo = new InvoiceRepository();

            var invoice = new Invoice(repo)
            {
                Amount = 0,
                AmountPaid = 0,
                Payments = null,
            };

            repo.Add(invoice);

            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment();

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("no payment needed", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureDTO_When_InvoiceAlreadyFullyPaid()
        {
            var repo = new InvoiceRepository();

            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 10,
                Payments = new List<Payment> { new Payment { Amount = 10 } },
            };
            repo.Add(invoice);

            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment();

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("invoice was already fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureDTO_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } },
            };
            repo.Add(invoice);

            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment { Amount = 6 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(
                "the payment is greater than the partial amount remaining",
                result.Message
            );
        }

        [Test]
        public void ProcessPayment_Should_ReturnSuccessDTO_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } },
            };
            repo.Add(invoice);

            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment { Amount = 5 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(
                "final partial payment received, invoice is now fully paid",
                result.Message
            );
        }

        [Test]
        public void ProcessPayment_Should_ReturnSuccessDTO_When_InvoiceIsPartiallyPaid()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>(),
            };
            repo.Add(invoice);

            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment { Amount = 5 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("invoice is now partially paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>() { new Payment() { Amount = 10 } },
            };
            repo.Add(invoice);

            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment() { Amount = 10 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("invoice was already fully paid", result.Message);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 5,
                Payments = new List<Payment> { new Payment { Amount = 5 } },
            };
            repo.Add(invoice);

            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment() { Amount = 1 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(
                "another partial payment received, still not fully paid",
                result.Message
            );
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(repo)
            {
                Amount = 10,
                AmountPaid = 0,
                Payments = new List<Payment>(),
            };
            repo.Add(invoice);

            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment() { Amount = 1 };

            var result = paymentProcessor.ProcessPayment(payment);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("invoice is now partially paid", result.Message);
        }
    }
}
