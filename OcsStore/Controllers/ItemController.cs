using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using OcsStore.Models;
using System.Data.Entity;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;

namespace OcsStore.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ItemController: Controller
    {
        private MyDbContext _context;

        public ItemController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult GetItemViews(int groupId, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ItemViews.Where(i => i.Group == groupId), loadOptions);
            return Ok(result);
        }

        public Item[] GetItems(short groupId)
        {
            return _context.Items.Where(i => i.Group == groupId).ToArray();
        }

        [HttpPost]
        public IActionResult GetItemManagementViews(int groupId, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ItemManagementViews.Where(i => i.Group == groupId), loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult GetNormalUnits(DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.UnitManagementViews.Where(i => i.Id != i.BaseUnit), loadOptions);
            return Ok(result);
        }

        public Unit[] GetBaseUnits()
        {
            return _context.Units.Where(i => i.Id == i.BaseUnit).ToArray();
        }

        public List<ItemView> GetReceivingItems()
        {
            return _context.ItemViews.Where(i => i.ItemType == Item.Receving).ToList();
        }

        public ItemView GetItemView(int itemId)
        {
            return _context.ItemViews.FirstOrDefault(i => i.Id == itemId);
        }

        public ItemView GetItemViewOfGroup(int itemGroupId)
        {
            return _context.ItemViews.FirstOrDefault(i => i.Group == itemGroupId);
        }

        public Item GetItem(int itemId)
        {
            return _context.Items.FirstOrDefault(i => i.Id == itemId);
        }

        public Unit[] GetUnits()
        {
            return _context.Units.ToArray();
        }

        public ItemGroup[] GetItemGroups()
        {
            return _context.ItemGroups.ToArray();
        }

        [HttpPost]
        public IActionResult Save(Item item)
        {
            SaveItem(item);
            return Ok();
        }

        private void SaveItem(Item item)
        {
            if (item.Id == 0)
            {
                try
                {
                    item.Id = _context.Items.Max(i => i.Id) + 1;
                }
                catch
                {
                    item.Id = 1;
                }
                if (string.IsNullOrEmpty(item.FullName))
                    item.FullName = item.Name;

                if (string.IsNullOrEmpty(item.Code))
                    item.Code = item.Name;

                _context.Items.Add(item);

                if (item.Group == 2)
                {
                    var materialId = item.Id + 1;
                    string materialName = item.Name + " N.Xanh";
                    var material = new Item() { Id = materialId, Name = materialName, Code = materialName, FullName = materialName };
                    _context.Items.Add(material);

                    var itemMaterial = new ItemMaterial() { Item = item.Id, Material = materialId };
                    _context.ItemMaterials.Add(itemMaterial);
                }
            }
            else
            {
                _context.Items.Update(item);

                if (item.Group == 2)
                {
                    var itemMaterial = _context.ItemMaterials.FirstOrDefault(i => i.Item == item.Id);
                    if (itemMaterial != null)
                    {
                        var material = _context.Items.FirstOrDefault(i => i.Id == itemMaterial.Material);
                        if (material != null)
                        {
                            string materialName = item.Name + " N.Xanh";
                            material.Name = materialName;
                            material.Code = materialName;
                            material.FullName = materialName;
                            _context.Items.Update(material);
                        }
                    }
                }
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveItems(Item[] data)
        {
            foreach (Item item in data)
            {
                SaveItem(item);
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (_context.ReceivingDetails.FirstOrDefault(i => i.Item == id) == null
                && _context.ProcessingInputs.FirstOrDefault(i => i.Item == id) == null)
            {
                var item = _context.Items.FirstOrDefault(i => i.Id == id);
                if (item != null)
                {
                    if (item.Group == 2)
                    {
                        var itemMaterial = _context.ItemMaterials.FirstOrDefault(i => i.Item == item.Id);
                        if (itemMaterial != null)
                        {
                            var material = _context.Items.FirstOrDefault(i => i.Id == itemMaterial.Material);
                            if (material != null && _context.ReceivingDetails.FirstOrDefault(i => i.Item == material.Id) == null
                                    && _context.ProcessingInputs.FirstOrDefault(i => i.Item == material.Id) == null)
                            {
                                _context.ItemMaterials.Remove(itemMaterial);
                                _context.Items.Remove(material);
                                _context.Items.Remove(item);
                            }
                        }
                    }
                    else
                    {
                        _context.Items.Remove(item);
                    }
                    _context.SaveChanges();
                }
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult DeleteUnit(int id)
        {
            if (_context.ReceivingDetails.FirstOrDefault(i => i.Unit == id) == null
                && _context.ProcessingInputs.FirstOrDefault(i => i.Unit == id) == null
                && _context.BillDetails.FirstOrDefault(i => i.Unit == id) == null)
            {
                var unit = _context.Units.FirstOrDefault(i => i.Id == id);
                if (unit != null)
                {
                    _context.Units.Remove(unit);
                    _context.SaveChanges();
                }
            }
            return Ok();
        }

        private void SaveUnit(Unit unit)
        {
            if (unit.Id == 0)
            {
                try
                {
                    unit.Id = (short)(_context.Units.Max(i => i.Id) + 1);
                }
                catch
                {
                    unit.Id = 1;
                }
                if (string.IsNullOrEmpty(unit.FullName))
                    unit.FullName = unit.Name;

                _context.Units.Add(unit);
            }
            else
            {
                _context.Units.Update(unit);
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveUnits(Unit[] data)
        {
            foreach (Unit unit in data)
            {
                SaveUnit(unit);
            }
            return Ok();
        }

        [HttpPost]
        public IActionResult GetMaterialViews(int groupId, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.MaterialViews.Where(i => i.ItemGroup == groupId), loadOptions);
            return Ok(result);
        }

        public Item[] GetAllItems()
        {
            return _context.Items.ToArray();
        }

        public Item[] GetItemsForMaterials()
        {
            return _context.Items.Where(i => i.Group > 1).ToArray();
        }

        public Item[] GetMaterialItems(int itemId)
        {
            var itemGroup = _context.Items.FirstOrDefault(i => i.Id  == itemId).Group;
            return _context.Items.Where(i => i.Group < itemGroup).ToArray();
        }

        [HttpPost]
        public IActionResult GetMaterials(int itemId, DataSourceLoadOptions loadOptions)
        {
            var result = DataSourceLoader.Load(_context.ItemMaterials.Where(i => i.Item == itemId), loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult DeleteMaterial(int item, int material)
        {
            var itemMaterial = _context.ItemMaterials.FirstOrDefault(i => i.Item == item && i.Material == material);
            if (itemMaterial != null)
            {
                _context.ItemMaterials.Remove(itemMaterial);
                _context.SaveChanges();
            }
            return Ok();
        }

        private void SaveMaterial(ItemMaterial itemMaterial)
        {
            var existingData = _context.ItemMaterials.FirstOrDefault(i => i.Item == itemMaterial.Item && i.Material == itemMaterial.Material);
            if (existingData != null)
            {
                _context.Entry(existingData).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                _context.ItemMaterials.Update(itemMaterial);
            }
            else
            {
                _context.ItemMaterials.Add(itemMaterial);
            }
            _context.SaveChanges();
        }

        [HttpPost]
        public IActionResult SaveMaterials(ItemMaterial[] data)
        {
            foreach (ItemMaterial itemMaterial in data)
            {
                SaveMaterial(itemMaterial);
            }
            return Ok();
        }

        public int FirstMaterialIdToCreateItem()
        {
            return _context.Items.FirstOrDefault(i => i.Group == 2).Id;
        }

        public int SecondMaterialIdToCreateItem()
        {
            return _context.Items.FirstOrDefault(i => i.Group == 2 && i.Name.ToLower().StartsWith("ao")).Id;
        }

        [HttpPost]
        public IActionResult GetItemCoupleMaterials(int material1, int material2, DataSourceLoadOptions loadOptions)
        {
            var itemMaterials = _context.ItemMaterialViews.Where(i => i.ItemGroup == 3).ToList();

            List<ItemCoupleMaterialView> data = new List<ItemCoupleMaterialView>();
            for (int i = 0; i <= 10; i++)
            {
                ItemCoupleMaterialView v = new ItemCoupleMaterialView() { Selected = false, Item = 0, Material1 = material1,MaterialName1 = GetItem(material1).Name, Quantity1 = (10 - i), Material2 = material2, MaterialName2 = GetItem(material2).Name, Quantity2 = i};

                if (i == 0)
                    v.CalculatedName = "100" + v.MaterialName1;
                else if (i == 10)
                    v.CalculatedName = "100" + v.MaterialName2;
                else
                    v.CalculatedName = "Cf " + (10 - i) + v.MaterialName1 + " - " + i + v.MaterialName2;


                var itemMaterial1s = itemMaterials.Where(i => i.Material ==  material1 && i.Quantity * 10 == v.Quantity1).ToArray();
                foreach (var itemMaterial1 in itemMaterial1s)
                {
                    var itemMaterial2 = itemMaterials.FirstOrDefault(i => i.Item == itemMaterial1.Item && i.Material == material2 && i.Quantity * 10 == v.Quantity2);
                    if (itemMaterial2 != null)
                    {
                        v.Item = itemMaterial2.Item;
                        v.ItemName = itemMaterial2.ItemName;
                        break;
                    }
                }

                data.Add(v);
            }

            var result = DataSourceLoader.Load(data, loadOptions);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult SaveItemCoupleMaterials(ItemCoupleMaterialView[] data)
        {
            foreach (var record in data)
            {
                var item = new Item() { Id = 0, Name = record.CalculatedName, Group = 3 };
                SaveItem(item);

                var itemMaterial1 = new ItemMaterial() { Item = item.Id, Material = record.Material1, Quantity = record.Quantity1 / 10 };
                _context.ItemMaterials.Add(itemMaterial1);

                var itemMaterial2 = new ItemMaterial() { Item = item.Id, Material = record.Material2, Quantity = record.Quantity2 / 10 };
                _context.ItemMaterials.Add(itemMaterial2);

                _context.SaveChanges();
            }
            return Ok();
        }
    }
}
