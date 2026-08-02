using Microsoft.EntityFrameworkCore.Internal;
using OcsStore.Models;
using ZstdSharp.Unsafe;

namespace OcsStore
{
    public class DBBilling
    {
        public static void UpdateStoreTransactionsForMissingBillDetails(MyDbContext _context)
        {
            var tranId = DB.GetNewId(_context, "store_transaction");
            var detailIdQuery = _context.StoreTransactions.Where(i => i.Type == StoreTransactionType.Billing).Select(i => i.DetailId);
            var missingBillDetails = _context.BillDetails.Where(i => !detailIdQuery.Contains(i.Id)).ToArray();
            foreach (var billDetail in missingBillDetails)
            {
                var bill = _context.Bills.FirstOrDefault(i => i.Id == billDetail.Bill);
                UpdateStoreTransactionForBillDetail(_context, tranId++, bill, billDetail);
            }
        }

        public static void UpdateStoreTransactionForBillDetail(MyDbContext _context, int tranId, Bill b, BillDetail d)
        {
            var ordinal = DB.GetNewTransactionOrdinal(_context, b.Date, b.Time, tranId);
            var buExchange = (decimal)_context.Units.FirstOrDefault(i => i.Id == d.Unit).BuExchange;

            var tran = new StoreTransaction() { Id = tranId, Date = b.Date, Time = b.Time, Type = StoreTransactionType.Billing, Store = b.Store, MainId = b.Id, DetailId = d.Id, Item = d.Item, Unit = 1, Quantity = -d.Quantity * buExchange, Price = d.Price / buExchange, User = b.UserCreated, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            DB.UpdateStoreTransactions(_context, b.Store, d.Item, d.Unit, ordinal);
        }

        public static void DeleteStoreTransactionsForBill(MyDbContext _context, Bill bill)
        {
            DB.DeleteStoreTransaction(_context, StoreTransactionType.Billing, bill.Id, null);

            var detailIds = _context.BillDetailViews.Where(i => i.Bill == bill.Id).Select(i => i.Id).ToArray();
            foreach (var detailId in detailIds)
            {
                DB.DeleteStoreTransaction(_context, StoreTransactionType.Billing, bill.Id, detailId);
            }
        }

        public static void UpdateStoreTransactionDateTimesForBill(MyDbContext _context, Bill bill)
        {
            DB.UpdateStoreTransactionDateTime(_context, StoreTransactionType.Billing, bill.Id, null, bill.Date, bill.Time);

            var detailIds = _context.BillDetailViews.Where(i => i.Bill == bill.Id).Select(i => i.Id).ToArray();
            DB.UpdateStoreTransactionDateTimes(_context, StoreTransactionType.Billing, bill.Id, detailIds, bill.Date, bill.Time);
        }

        public static bool EditBillDateTime(MyDbContext _context, int id, DateTime date, string time, out string errorMessage)
        {
            var bill = _context.Bills.FirstOrDefault(i => i.Id == id);
            if (bill != null)
            {
                bill.Date = Common.GetLocalDateWithoutTime(date);
                bill.Time = time;
                _context.Database.BeginTransaction();
                _context.Bills.Update(bill);
                _context.SaveChanges();
                try
                {
                    UpdateStoreTransactionDateTimesForBill(_context, bill);
                }
                catch (Exception ex)
                {
                    _context.Database.RollbackTransaction();
                    errorMessage = ex.Message;
                    return false;
                }
                _context.Database.CommitTransaction();
            }
            errorMessage = null;
            return true;
        }

        public static void RemoveInvalidCustomerTransactions(MyDbContext _context)
        {
            var invalidTransactionIds = _context.InvalidCustomerTransactionIdViews.Select(i => i.Id).ToArray();
            var invalidTransactions = _context.CustomerTransactions.Where(i => invalidTransactionIds.Contains(i.Id)).ToArray();
            if (invalidTransactions.Length > 0)
            {
                foreach (var transaction in invalidTransactions)
                {
                    _context.CustomerTransactions.Remove(transaction);
                    _context.SaveChanges();
                    UpdateCustomerTransactionDebt(_context, transaction.Customer, transaction.Ordinal);
                }
            }
        }

        public static void UpdateMissingPayments(MyDbContext _context)
        {
            var bills = _context.PaidBillWithoutPaymentViews.ToArray();
            var id = DB.GetNewId(_context, "payment");
            foreach (var b in bills)
            {
                CreatePayment(_context, b, 1, id++);
            }
        }

        public static Payment CreatePayment(MyDbContext _context, IBill b, short userId, int paymentId = 0)
        {
            var id = (paymentId > 0 ? paymentId : DB.GetNewId(_context, "payment"));
            DateTime date = DateTime.Today;
            string time = DateTime.Now.ToString("HH:mm");
            var payment = new Payment() { Id = id, Customer = b.Customer, Date = DateOnly.FromDateTime(date), Time = time, Amount = b.TotalValue, UserCreated = b.UserPaid ?? 1 };
            payment.DateCreated = payment.Date;
            payment.TimeCreated = payment.Time;

            _context.Payments.Add(payment);

            var paymentDetail = new PaymentDetail() { Payment = payment.Id, Bill = b.Id, Amount = b.TotalValue, PaidFullBill = true };
            _context.PaymentDetails.Add(paymentDetail);
            _context.SaveChanges();

            CreateCustomerTransactionsForPayment(_context, payment);

            var bill = _context.Bills.FirstOrDefault(i => i.Id == b.Id);
            if (bill != null)
            {
                bill.Paid = true;
                bill.DatePaid = date;
                bill.TimePaid = time;
                bill.UserPaid = userId;
                _context.Bills.Update(bill);
                _context.SaveChanges();
            }

            return payment;
        }

        public static void UpdateMissingCustomerTransactions(MyDbContext _context)
        {
            var id = DB.GetNewId(_context, "customer_transaction");
            var bills = _context.MissingTransactionBillViews.ToArray();
            foreach (var b in bills)
            {
                CreateCustomerTransactionsForBill(_context, b, id++);
            }

            var payments = _context.MissingTransactionPaymentViews.ToArray();
            foreach (var p in payments)
            {
                CreateCustomerTransactionsForPayment(_context, p, id++);
            }
        }

        public static void CreateCustomerTransactionsForBill(MyDbContext _context, ITransactionBill b, int tranId = 0)
        {
            var id = (tranId > 0 ? tranId : DB.GetNewId(_context, "customer_transaction"));

            var ordinal = DB.GetNewTransactionOrdinal(_context, b.Date, b.Time, id);
            var tran = new CustomerTransaction() { Id = id, Ordinal = ordinal, Customer = b.Customer, MainId = b.Id, Type = CustomerTransactionType.Bill, Date = DateOnly.FromDateTime(b.Date), Time = b.Time, Amount = b.TotalValue, User = b.UserCreated, IsCompleted = true };

            var paymentDetail = _context.PaymentDetails.FirstOrDefault(i => i.Bill == b.Id);
            if (paymentDetail != null)
                tran.IsCompleted = paymentDetail.PaidFullBill;

            _context.CustomerTransactions.Add(tran);
            _context.SaveChanges();
            UpdateCustomerTransactionDebt(_context, b.Customer, ordinal);
        }

        public static void CreateCustomerTransactionsForPayment(MyDbContext _context, ITransactionPayment p, int tranId = 0)
        {
            var id = (tranId > 0 ? tranId : DB.GetNewId(_context, "customer_transaction"));

            var ordinal = DB.GetNewTransactionOrdinal(_context, p.Date.ToDateTime(TimeOnly.MinValue), p.Time, id);
            var tran = new CustomerTransaction() { Id = id, Ordinal = ordinal, Customer = p.Customer, MainId = p.Id, Type = CustomerTransactionType.Payment, Date = p.Date, Time = p.Time, Amount = p.Amount, User = (short)p.UserCreated, IsCompleted = p.IsCompleted ?? true };
            _context.CustomerTransactions.Add(tran);
            _context.SaveChanges();
            UpdateCustomerTransactionDebt(_context, p.Customer, ordinal);
        }

        public static void DeletePaymentForBill(MyDbContext _context, int billId)
        {
            var paymentDetails = _context.PaymentDetails.Where(i => i.Bill == billId).ToArray();
            if (paymentDetails.Length > 0)
            {
                foreach (var detail in paymentDetails)
                {
                    var paymentId = detail.Payment;
                    DeleteCustomerTransactionForPayment(_context, paymentId);

                    _context.PaymentDetails.Remove(detail);

                    var payment = _context.Payments.FirstOrDefault(i => i.Id == paymentId);
                    _context.Payments.Remove(payment);
                }
            }
        }

        public static void DeleteCustomerTransactionForBill(MyDbContext _context, int billId)
        {
            var transaction = _context.CustomerTransactions.FirstOrDefault(i => i.Type == CustomerTransactionType.Bill && i.MainId == billId);
            if (transaction != null)
            {
                _context.CustomerTransactions.Remove(transaction);
                _context.SaveChanges();

                UpdateCustomerTransactionDebt(_context, transaction.Customer, transaction.Ordinal);
            }
        }

        public static void DeleteCustomerTransactionForPayment(MyDbContext _context, int paymentId)
        {
            var transaction = _context.CustomerTransactions.FirstOrDefault(i => i.Type == CustomerTransactionType.Payment && i.MainId == paymentId);
            if (transaction != null)
            {
                _context.CustomerTransactions.Remove(transaction);
                _context.SaveChanges();

                UpdateCustomerTransactionDebt(_context, transaction.Customer, transaction.Ordinal);
            }
        }

        public static void UpdateCustomerTransactionDebt(MyDbContext _context, short customerId, long fromOrdinal)
        {
            decimal debt = 0;
            var tran = _context.CustomerTransactions.Where(i => i.Customer == customerId && i.Ordinal < fromOrdinal).OrderByDescending(i => i.Ordinal).FirstOrDefault();
            if (tran != null)
                debt = tran.Debt;

            var trans = _context.CustomerTransactions.OrderBy(i => i.Ordinal).Where(i => i.Customer == customerId && i.Ordinal >= fromOrdinal).ToArray();
            foreach (var t in trans)
            {
                if (t.Type == CustomerTransactionType.Bill)
                    debt += t.Amount;
                else
                    debt -= t.Amount;

                t.Debt = debt;
                _context.CustomerTransactions.Update(t);
            }
            var customer = _context.Customers.FirstOrDefault(i => i.Id == customerId);
            customer.Debt = debt;
            _context.Customers.Update(customer);
            _context.SaveChanges();
        }

        public static void UpdateCustomerTransactionsStatus(MyDbContext _context)
        {
            var incompleteBillTrans = _context.CustomerTransactions.Where(i => i.Type == CustomerTransactionType.Bill && !i.IsCompleted).ToArray();
            foreach (var tran in incompleteBillTrans)
            {
                var paymentDetails = _context.PaymentDetails.Where(i => i.Bill == tran.MainId).ToArray();
                if (paymentDetails.Length > 0)
                {
                    if (paymentDetails[0].PaidFullBill)
                    {
                        tran.IsCompleted = true;
                        _context.CustomerTransactions.Update(tran);
                    }
                }
            }
            _context.SaveChanges();
        }
    }
}
