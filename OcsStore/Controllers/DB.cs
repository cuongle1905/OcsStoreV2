using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using OcsStore.Models;
using System.Data.Entity;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using ZstdSharp.Unsafe;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace OcsStore
{
    public static class DB
    {
        public static int GetNewId(MyDbContext _context, string tableName)
        {
            var result = _context.Database.SqlQueryRaw<int>("select ifnull(max(`id`) + 1, 1) from `" + tableName + "`").ToArray();
            if (result.Length > 0)
                return result[0];

            return 1;
        }

        public static long GetNewStoreTransactionOrdinal(MyDbContext _context, DateTime date, string time, int tranId)
        {
            long ordinal = long.Parse(date.ToString("yyMMdd") + time.Replace(":", "")) * 1000000 + tranId % 1000000;
            return ordinal;
        }

        // `calculate_strans_soh`(storeId smallint, itemId int, unitId smallint, p_lot varchar(10), yy tinyint, p_ordinal bigint)
        public static void UpdateStoreTransactions(MyDbContext _context, short storeId, int itemId, short unitId, long ordinal, bool ignoreError = false)
        {
            decimal soh = 0, value = 0, ave = 0;
            long fromOrdinal;

            var prevTran = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == 1 && i.Ordinal < ordinal).OrderByDescending(i => i.Ordinal).FirstOrDefault();

            if (prevTran != null)
            {
                fromOrdinal = prevTran.Ordinal;
                soh = prevTran.Soh ?? 0;
                value = prevTran.Value ?? 0;
                ave = prevTran.Ave ?? 0;
            }
            else
                fromOrdinal = 0;

            /* 	SELECT id, `type`, detail_id, quantity, price, ordinal
		          INTO tranId, v_type, detailId, v_quantity, v_price, v_ordinal
		          FROM store_transaction where store = storeId and item = itemId and unit = unitId and ordinal > v_ordinal order by ordinal LIMIT 1;*/
            var trans = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == 1 && i.Ordinal > fromOrdinal).OrderBy(i => i.Ordinal).ToArray();
            int lastTranId = 0;
            foreach (var tran in trans)
            {
                if (tran.Type == StoreTransactionType.Inventory)
                {
                    /* SELECT soh, ave INTO inventory_soh, inventory_ave from inventory_detail where id = detailId;
				        SET v_quantity = inventory_soh - v_soh;
				        SET v_price = v_ave;
				        IF v_quantity != 0 AND inventory_ave > 0 THEN
					        SET v_price = (inventory_soh * inventory_ave - v_value) / v_quantity;
					        SET v_ave = inventory_ave;
				        ELSE
					        SET v_price = v_ave;
				        END IF;
				        SET v_soh = inventory_soh;
				        SET v_value = v_soh * v_ave;
				        UPDATE store_transaction set quantity = v_quantity, price = v_price, soh = v_soh, `value` = v_value, ave = v_ave where id = tranId; */
                    var inventoryDetail = _context.InventoryDetails.FirstOrDefault(i => i.Id == tran.DetailId);
                    if (inventoryDetail != null)
                    {
                        decimal quantity = inventoryDetail.Soh - soh;
                        decimal price;
                        if (quantity != 0 && inventoryDetail.Ave > 0)
                        {
                            price = (inventoryDetail.Soh * inventoryDetail.Ave - value) / quantity;
                            ave = inventoryDetail.Ave;
                        }
                        else
                        {
                            price = ave;
                        }
                        soh = inventoryDetail.Soh;
                        value = soh * ave;

                        tran.Quantity = quantity;
                        tran.Price = price;
                    }
                    else
                    {
                        _context.StoreTransactions.Remove(tran);
                    }
                }
                else
                {
                    if (IsValidTransaction(_context, tran))
                    {
                        soh += tran.Quantity;

                        ////if (!ignoreError && soh < 0)
                        ////{
                        ////    var itemName = _context.Items.FirstOrDefault(i => i.Id == itemId).Name;
                        ////    throw new InvalidOperationException($"Tồn kho < 0 '{itemName}' {tran.Date.ToString("dd/MM/yyyy")}");
                        ////}

                        if (tran.Quantity < 0)
                        {
                            tran.Price = ave;
                            value += tran.Quantity * tran.Price;
                        }
                        else
                        {
                            value += tran.Quantity * tran.Price;
                            if (soh != 0)
                                ave = value / soh;
                        }
                    }
                    else
                    {
                        _context.StoreTransactions.Remove(tran);
                    }
                }
                tran.Soh = soh;
                tran.Value = value;
                tran.Ave = ave;
                _context.StoreTransactions.Update(tran);
                lastTranId = tran.Id;
            }
            _context.SaveChanges();
            UpdateLastStoreTransaction(_context, storeId, itemId, unitId, lastTranId);
        }

        static bool IsValidTransaction(MyDbContext _context, StoreTransaction tran)
        {
            if (tran.DetailId == null)
                return (tran.Type == StoreTransactionType.Receiving && _context.Receivings.Count(i => i.Id == tran.MainId) > 0) || (tran.Type == StoreTransactionType.Processing && _context.Processings.Count(i => i.Id == tran.MainId) > 0) || (tran.Type == StoreTransactionType.Billing && _context.Bills.Count(i => i.Id == tran.MainId) > 0);

            return (tran.Type == StoreTransactionType.Receiving && _context.ReceivingDetails.Count(i => i.Id == tran.DetailId) > 0) || (tran.Type == StoreTransactionType.Processing && _context.ProcessingInputs.Count(i => i.Id == tran.DetailId) > 0) || (tran.Type == StoreTransactionType.Billing && _context.BillDetails.Count(i => i.Id == tran.DetailId) > 0);
        }

        public static void UpdateLastStoreTransaction(MyDbContext _context, short storeId, int itemId, short unitId, int lastTranId)
        {
            var lastStoreTransaction = _context.LastStoreTransactions.FirstOrDefault(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId);
            if (lastStoreTransaction != null)
            {
                if (lastTranId > 0)
                {
                    lastStoreTransaction.LastTransaction = lastTranId;
                    _context.LastStoreTransactions.Update(lastStoreTransaction);
                }
                else
                {
                    _context.LastStoreTransactions.Remove(lastStoreTransaction);
                }
            }
            else if (lastTranId > 0)
            {
                lastStoreTransaction = new LastStoreTransaction() { Store = storeId, Item = itemId, Unit = unitId, LastTransaction = lastTranId };
                _context.LastStoreTransactions.Add(lastStoreTransaction);
            }
            _context.SaveChanges();
        }

        public static void DeleteStoreTransaction(MyDbContext _context, int tranId, short storeId, int itemId, short unitId, long ordinal)
        {
            /*  IF p_lot IS NULL THEN
		            SELECT ordinal into v_ordinal from store_transaction
		              where `type` = p_type and store = storeId and item = itemId and unit = unitId and ordinal > p_ordinal order by ordinal asc LIMIT 1;
	            ELSE
		            SELECT ordinal into v_ordinal from store_transaction
		              where `type` = p_type and store = storeId and item = itemId and unit = unitId and lot = p_lot and `year` = yy and ordinal > p_ordinal
                      order by ordinal asc LIMIT 1;
                END IF; */
            long fromOrdinal = 0;
            var nextTran = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId).OrderBy(i => i.Ordinal).First();
            if (nextTran != null)
            {
                fromOrdinal = nextTran.Ordinal;
            }
            else
            {
                var prevTran = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId).OrderByDescending(i => i.Ordinal).First();
                if (prevTran != null)
                {
                    fromOrdinal = prevTran.Ordinal;
                }
            }

            var tran = _context.StoreTransactions.FirstOrDefault(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && i.Ordinal == ordinal);
            var isNegativeQuantity = tran.Quantity <= 0;
            if (tran != null)
            {
                _context.StoreTransactions.Remove(tran);
                _context.SaveChanges();
            }
            UpdateStoreTransactions(_context, storeId, itemId, unitId, fromOrdinal, isNegativeQuantity);
        }

        public static void DeleteStoreTransaction(MyDbContext _context, StoreTransaction fromStoreTransaction)
        {
            DeleteStoreTransaction(_context, fromStoreTransaction.Id, fromStoreTransaction.Store, fromStoreTransaction.Item, fromStoreTransaction.Unit, fromStoreTransaction.Ordinal);
        }

        public static void DeleteStoreTransaction(MyDbContext _context, sbyte type, int mainId, int? detailId)
        {
            var tran = _context.StoreTransactions.FirstOrDefault(i => i.Type == type && i.MainId == mainId && i.DetailId == detailId);
            if (tran != null)
                DeleteStoreTransaction(_context, tran);
        }

        public static void UpdateStoreTransactionDateTime(MyDbContext _context, StoreTransaction tran, DateTime date, string time)
        {
            tran.Date = date;
            tran.Time = time;

            var oldOrdinal = tran.Ordinal;
            tran.Ordinal = GetNewStoreTransactionOrdinal(_context, date, time, tran.Id);

            _context.StoreTransactions.Update(tran);
            _context.SaveChanges();

            UpdateStoreTransactions(_context, tran.Store, tran.Item, tran.Unit, Math.Min(oldOrdinal, tran.Ordinal), tran.Quantity <= 0);
        }

        public static void UpdateStoreTransactionDateTime(MyDbContext _context, sbyte type, int mainId, int? detailId, DateTime date, string time)
        {
            var tran = _context.StoreTransactions.FirstOrDefault(i => i.Type == type && i.MainId == mainId && i.DetailId == detailId);
            if (tran != null)
                UpdateStoreTransactionDateTime(_context, tran, date, time);
        }

        public static void UpdateStoreTransactionDateTimes(MyDbContext _context, sbyte type, int mainId, int[] detailIds, DateTime date, string time)
        {
            foreach (var detailId in detailIds)
            {
                UpdateStoreTransactionDateTime(_context, type, mainId, detailId, date, time);
            }
        }

        public static void UpdateAllStoreTransactions(MyDbContext _context)
        {
            var data = _context.StoreTransactions.Select(i => new  { i.Store, i.Item, i.Unit }).Distinct().ToArray();
            foreach(var r in data)
            {
                UpdateItemAllStoreTransactions(_context, r.Store, r.Item, r.Unit);
            }
        }

        public static void UpdateItemAllStoreTransactions(MyDbContext _context, short storeId, int itemId, short unitId)
        {
            UpdateStoreTransactions(_context, storeId, itemId, unitId, 0, true);
        }
    }
}
