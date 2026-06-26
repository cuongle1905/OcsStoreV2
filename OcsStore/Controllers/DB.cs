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

        public static void UpdateStockForReceiving(MyDbContext _context, Receiving r, List<ReceivingDetail> details)
        {
            /*  DELETE FROM store_transaction where `type` = 1 and main_id = receivingId; */

            sbyte yy = (sbyte)(r.Date.Year % 100);

            /* insert into store_transaction (id, date, time, type, store, main_id, detail_id, item, unit, `year`, quantity, price, `user`, ordinal)
                values (tranId, v_date, v_time, 1, storeId, receivingId, detailId, itemId, unitId, yy, v_quantity, v_price, userId, v_ordinal);
             */
            var tranId = GetNewId(_context, "store_transaction");

            foreach (ReceivingDetail d in details)
            {
                try
                {
                    UpdateStockForReceivingDetail(_context, tranId, yy, r, d);
                }
                catch
                {
                    throw;
                }
                tranId++;
            }
        }

        public static void UpdateStockForReceivingDetail(MyDbContext _context, int tranId, sbyte yy, Receiving r, ReceivingDetail d)
        {
            /* insert into store_transaction (id, date, time, type, store, main_id, detail_id, item, unit, `year`, quantity, price, `user`, ordinal)
                values (tranId, v_date, v_time, 1, storeId, receivingId, detailId, itemId, unitId, yy, v_quantity, v_price, userId, v_ordinal); */
            
            var ordinal = GetNewStoreTransactionOrdinal(_context, yy, r.Date, r.Time, tranId);
            var tran = new StoreTransaction() { Id = tranId, Date = r.Date, Time = r.Time, Type = 1, Store = r.Store, MainId = r.Id, DetailId = d.Id, Item = d.Item, Unit = d.Unit, Year = yy, Quantity = d.Quantity, Price = d.Price, User = r.User, Ordinal = ordinal };
            
            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            UpdateStoreTransactions(_context, r.Store, d.Item, d.Unit, null, yy, ordinal, true);
        }

        public static void UpdateStockForBillLotDetail(MyDbContext _context, int tranId, Bill b, BillDetail bd, BillLotDetail d)
        {
            /* insert into store_transaction (id, date, time, type, store, main_id, detail_id, item, unit, `year`, quantity, `user`, ordinal)
                values (tranId, v_date, v_time, 3, storeId, billId, detailId, itemId, unitId, yy, -v_quantity, userId, v_ordinal); */

            var ordinal = GetNewStoreTransactionOrdinal(_context, d.Year, b.Date, b.Time, tranId);
            var tran = new StoreTransaction() { Id = tranId, Date = b.Date, Time = b.Time, Type = 3, Store = b.Store, MainId = b.Id, DetailId = d.Id, Item = bd.Item, Unit = bd.Unit, Lot = d.Lot, Year = d.Year, Quantity = -d.Quantity, User = b.UserCreated, Ordinal = ordinal };

            _context.StoreTransactions.Add(tran);
            _context.SaveChanges();

            UpdateStoreTransactions(_context, b.Store, bd.Item, bd.Unit, d.Lot, d.Year, ordinal, true);
        }

        public static long GetNewStoreTransactionOrdinal(MyDbContext _context, sbyte yy, DateTime date, string time, int tranId)
        {
            //  SET v_ordinal = (concat(yy * 10000 + month(v_date) * 100 + day(v_date), REPLACE(v_time, ':', '')) + 0) * 1000000 + tranId % 1000000;
            long ordinal = long.Parse(((int)yy * 10000 + date.Month * 100 + date.Day).ToString() + time.Replace(":", "")) * 1000000 + tranId % 1000000;
            return ordinal;
        }

        // `calculate_strans_soh`(storeId smallint, itemId int, unitId smallint, p_lot varchar(10), yy tinyint, p_ordinal bigint)
        public static void UpdateStoreTransactions(MyDbContext _context, short storeId, int itemId, short unitId, string lot, sbyte yy, long ordinal, bool ignoreError = false)
        {
            if (!string.IsNullOrEmpty(lot))
            {
                try
                {
                    UpdateLotStoreTransactions(_context, storeId, itemId, unitId, lot, yy, ordinal, ignoreError);
                }
                catch
                {
                    throw;
                }
            }
            decimal soh = 0, value = 0, ave = 0;
            long fromOrdinal;

            var prevTran = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && i.Ordinal < ordinal).OrderByDescending(i => i.Ordinal).FirstOrDefault();

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
            var trans = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && i.Ordinal > fromOrdinal).OrderBy(i => i.Ordinal).ToArray();
            int lastTranId = 0;
            foreach (var tran in trans)
            {
                if (tran.Type == 4)
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
                    value += tran.Quantity * ave;
                    soh += tran.Quantity;

                    if (!ignoreError && soh < 0)
                    {
                        var itemName = _context.Items.FirstOrDefault(i => i.Id == itemId).Name;
                        throw new InvalidOperationException($"Tồn kho < 0 '{itemName}' {tran.Date.ToString("dd/MM/yyyy")}");
                    }

                    if (tran.Quantity < 0)
                    {
                        /* SET v_value = v_value + v_quantity * v_ave, v_soh = v_soh + v_quantity;
                        IF p_lot is null THEN
                            UPDATE store_transaction set price = v_ave, soh = v_soh, `value` = v_value, ave = v_ave where id = tranId;
                        ELSE
                            UPDATE store_transaction set soh = v_soh, `value` = v_value, ave = v_ave where id = tranId;
                        END IF; */
                        if (string.IsNullOrEmpty(lot))
                        {
                            tran.Price = ave;
                        }
                    }
                    else
                    {
                        /* SET v_value = v_value + v_quantity * v_price, v_soh = v_soh + v_quantity;
                            SET v_ave = if(v_soh != 0, v_value / v_soh, 0);
                            UPDATE store_transaction set soh = v_soh, `value` = v_value, ave = v_ave where id = tranId; */
                        if (soh != 0)
                            ave = value / soh;
                    }
                }
                tran.Soh = soh;
                tran.Value = value;
                tran.Ave = ave;
                _context.StoreTransactions.Update(tran);
                lastTranId = tran.Id;
            }
            _context.SaveChanges();
            UpdateLastStoreTransaction(_context, storeId, itemId, unitId, "", yy, lastTranId);
        }

        public static void UpdateLotStoreTransactions(MyDbContext _context, short storeId, int itemId, short unitId, string lot, sbyte yy, long ordinal, bool ignoreError = false)
        {
            /*	SELECT id, lot_soh, lot_value, lot_ave, ordinal INTO tranId, v_soh, v_value, v_ave, v_ordinal
                FROM store_transaction where store = storeId and item = itemId and unit = unitId and lot = p_lot and `year` = yy and ordinal < p_ordinal
                order by ordinal desc limit 1; */
            decimal soh = 0, value = 0, ave = 0;
            long fromOrdinal;

            var prevTran = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && i.Lot == lot && i.Year == yy && i.Ordinal < ordinal).OrderByDescending(i => i.Ordinal).FirstOrDefault();

            if (prevTran != null)
            {
                fromOrdinal = prevTran.Ordinal;
                soh = prevTran.LotSoh ?? 0;
                value = prevTran.LotValue ?? 0;
                ave = prevTran.LotAve ?? 0;
            }
            else
                fromOrdinal = 0;

            /*
             * SELECT id, `type`, detail_id, quantity, price, ordinal
              INTO tranId, v_type, detailId, v_quantity, v_price, v_ordinal
              FROM store_transaction where store = storeId and item = itemId and unit = unitId and lot = p_lot and `year` = yy and ordinal > v_ordinal order by ordinal; */
            var trans = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && i.Lot == lot && i.Year == yy && i.Ordinal > fromOrdinal).OrderBy(i => i.Ordinal).ToArray();
            int lastTranId = 0;
            foreach (var tran in trans)
            {
                if (tran.Type == 4)
                {
                    /* SELECT soh, ave INTO inventory_soh, inventory_ave from inventory_detail where id = detailId;
					    SET v_quantity = inventory_soh - ifnull(v_soh, 0);
                        SET v_price = ifnull(v_ave, 0);
                        IF v_quantity != 0 AND inventory_ave > 0 THEN
						    SET v_price = (inventory_soh * inventory_ave - v_value) / v_quantity;
						    SET v_ave = inventory_ave;
					    ELSE
						    SET v_price = v_ave;
                        END IF;
                        SET v_soh = inventory_soh;
                        SET v_value = v_soh * v_ave;
					    UPDATE store_transaction set quantity = v_quantity, price = v_price, lot_soh = v_soh, `lot_value` = v_value, lot_ave = v_ave where id = tranId;
                     */
                    var inventoryDetail = _context.InventoryDetails.FirstOrDefault(i => i.Id == tran.DetailId);
                    if (inventoryDetail == null)
                    {
                        _context.StoreTransactions.Remove(tran);
                        continue;
                    }

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
                    value += tran.Quantity * ave;
                    soh += tran.Quantity;

                    if (!ignoreError && soh < 0)
                    {
                        var itemName = _context.Items.FirstOrDefault(i => i.Id == itemId).Name;
                        throw new InvalidOperationException($"Tồn kho < 0 '{itemName} {lot}' {tran.Date.ToString("dd/MM/yyyy")}");
                    }

                    if (tran.Quantity < 0)
                    {
                        /* SET v_value = v_value + v_quantity * v_ave, v_soh = v_soh + v_quantity;
                            UPDATE store_transaction set price = v_ave, lot_soh = v_soh, `lot_value` = v_value, lot_ave = v_ave where id = tranId; */
                        tran.Price = ave;
                    }
                    else
                    {
                        /* SET v_value = v_value + v_quantity * v_price, v_soh = v_soh + v_quantity;
                        SET v_ave = v_value / v_soh;
                        UPDATE store_transaction set lot_soh = v_soh, `lot_value` = v_value, lot_ave = v_ave where id = tranId; */
                        if (soh != 0)
                            ave = value / soh;
                    }
                }

                tran.LotSoh = soh;
                tran.LotValue = value;
                tran.LotAve = ave;
                _context.StoreTransactions.Update(tran);
                lastTranId = tran.Id;
            }
            _context.SaveChanges();
            UpdateLastStoreTransaction(_context, storeId, itemId, unitId, lot, yy, lastTranId);
        }

        public static void UpdateLastStoreTransaction(MyDbContext _context, short storeId, int itemId, short unitId, string lot, sbyte yy, int lastTranId)
        {
            /* IF lastTranId IS NULL THEN
			        DELETE FROM last_store_transaction where store = storeId and item = itemId and unit = unitId and `year` = yy and lot = p_lot;
                ELSEIF exists (SELECT * FROM last_store_transaction where store = storeId and item = itemId and unit = unitId and `year` = yy and lot = p_lot) THEN
			        UPDATE last_store_transaction set last_transaction = lastTranId where store = storeId and item = itemId and unit = unitId and `year` = yy and lot = p_lot;
		        ELSE
			        insert into last_store_transaction (store, item, unit, `year`, lot, last_transaction) values (storeId, itemId, unitId, yy, p_lot, lastTranId);
		        END IF;
             */
            var lastStoreTransaction = _context.LastStoreTransactions.FirstOrDefault(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && (string.IsNullOrEmpty(lot) ? i.Lot == "" : i.Lot == lot && i.Year == yy));
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
                lastStoreTransaction = new LastStoreTransaction() { Store = storeId, Item = itemId, Unit = unitId, Year = yy, Lot = lot, LastTransaction = lastTranId };
                _context.LastStoreTransactions.Add(lastStoreTransaction);
            }
            _context.SaveChanges();
        }

        public static void DeleteStoreTransaction(MyDbContext _context, int tranId, short storeId, int itemId, short unitId, string lot, sbyte yy, long ordinal)
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
            var nextTran = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && (string.IsNullOrEmpty(lot) ? true : i.Lot == lot && i.Year == yy)).OrderBy(i => i.Ordinal).First();
            if (nextTran != null)
            {
                fromOrdinal = nextTran.Ordinal;
            }
            else
            {
                var prevTran = _context.StoreTransactions.Where(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && (string.IsNullOrEmpty(lot) ? true : i.Lot == lot && i.Year == yy)).OrderByDescending(i => i.Ordinal).First();
                if (prevTran != null)
                {
                    fromOrdinal = prevTran.Ordinal;
                }
            }

            var tran = _context.StoreTransactions.FirstOrDefault(i => i.Store == storeId && i.Item == itemId && i.Unit == unitId && i.Ordinal == ordinal && (string.IsNullOrEmpty(lot) ? true : i.Lot == lot && i.Year == yy));
            if (tran != null)
            {
                _context.StoreTransactions.Remove(tran);
            }

            UpdateStoreTransactions(_context, storeId, itemId, unitId, lot, yy, fromOrdinal);
        }

        public static void DeleteStoreTransaction(MyDbContext _context, StoreTransaction fromStoreTransaction)
        {
            DeleteStoreTransaction(_context, fromStoreTransaction.Id, fromStoreTransaction.Store, fromStoreTransaction.Item, fromStoreTransaction.Unit, fromStoreTransaction.Lot, fromStoreTransaction.Year, fromStoreTransaction.Ordinal);
        }

        public static void UpdateStoreTransactionDateTime(MyDbContext _context, StoreTransaction fromStoreTransaction, DateTime date, string time)
        {
            fromStoreTransaction.Date = date;
            fromStoreTransaction.Time = time;

            var oldOrdinal = fromStoreTransaction.Ordinal;
            fromStoreTransaction.Ordinal = GetNewStoreTransactionOrdinal(_context, fromStoreTransaction.Year, date, time, fromStoreTransaction.Id);

            _context.StoreTransactions.Update(fromStoreTransaction);
            _context.SaveChanges();

            UpdateStoreTransactions(_context, fromStoreTransaction.Store, fromStoreTransaction.Item, fromStoreTransaction.Unit, fromStoreTransaction.Lot, fromStoreTransaction.Year, Math.Min(oldOrdinal, fromStoreTransaction.Ordinal));
        }

        public static void DeleteStoreTransactionsForProcessing(MyDbContext _context, Processing processing)
        {
            var outputStoreTransaction = _context.StoreTransactions.FirstOrDefault(i => i.Type == 2 && i.MainId == processing.Id && i.DetailId == null);
            if (outputStoreTransaction != null)
                DeleteStoreTransaction(_context, outputStoreTransaction);

            var processingLotDetails = _context.ProcessingLotInputViews.Where(i => i.Processing == processing.Id).ToArray();
            foreach (var detail in processingLotDetails)
            {
                var inputStoreTransaction = _context.StoreTransactions.FirstOrDefault(i => i.Type == 2 && i.MainId == processing.Id && i.DetailId == detail.Id);
                if (inputStoreTransaction != null)
                    DeleteStoreTransaction(_context, inputStoreTransaction);
            }
        }

        public static void UpdateStoreTransactionDateTimesForProcessing(MyDbContext _context, Processing processing)
        {
            var outputStoreTransaction = _context.StoreTransactions.FirstOrDefault(i => i.Type == 2 && i.MainId == processing.Id && i.DetailId == null);
            if (outputStoreTransaction != null)
                UpdateStoreTransactionDateTime(_context, outputStoreTransaction, processing.Date, processing.Time);

            var details = _context.ProcessingLotInputViews.Where(i => i.Processing == processing.Id).ToArray();
            foreach (var detail in details)
            {
                var inputStoreTransaction = _context.StoreTransactions.FirstOrDefault(i => i.Type == 2 && i.MainId == processing.Id && i.DetailId == detail.Id);
                if (inputStoreTransaction != null)
                    UpdateStoreTransactionDateTime(_context, inputStoreTransaction, processing.Date, processing.Time);
            }
        }

        public static void UpdateStoreTransactionDateTimesForReceiving(MyDbContext _context, Receiving receiving)
        {
            var details = _context.ReceivingDetails.Where(i => i.Receiving == receiving.Id).ToArray();
            foreach (var detail in details)
            {
                var storeTransaction = _context.StoreTransactions.FirstOrDefault(i => i.Type == 1 && i.MainId == receiving.Id && i.DetailId == detail.Id);
                if (storeTransaction != null)
                    UpdateStoreTransactionDateTime(_context, storeTransaction, receiving.Date, receiving.Time);
            }
        }
    }
}
