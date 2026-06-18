Date.prototype.ddMM = function () {
    const dd = this.getDate().toString().padStart(2, '0');
    // JavaScript months are 0-indexed, so we add 1
    const MM = (this.getMonth() + 1).toString().padStart(2, '0');
    return `${dd}${MM}`;
};

Date.prototype.ddMMyyyy = function () {
    const dd = this.getDate().toString().padStart(2, '0');
    // JavaScript months are 0-indexed, so we add 1
    const MM = (this.getMonth() + 1).toString().padStart(2, '0');
    const yyyy = this.getFullYear();
    return `${dd}/${MM}/${yyyy}`;
};

function defaultOnGridContentReady(e) {
    $(".dx-header-row > td").css("text-align", "center");

    if (typeof expandCollapseData === "function") {
        expandCollapseData();
    }
}

function defaultOnCellPrepared(e) {
    if (e.rowType === 'group' && e.column.command === 'expand') {
        e.cellElement.css('display', 'none');
    }
}

function defaultOnEditorPreparing(e) {
    if (e.parentType === "dataRow") {
        e.editorOptions.valueChangeEvent = "keyup";
    }
}

function cssColorForItemType(itemType) {
    return (itemType == 1 ? "green" : (itemType == 3 ? "blue" : "brown"));
}

function displayCell(container, options, prefix, prefixColor) {
    container.html(`<div><span class='${prefixColor} me-2'>${prefix}</span>${options.text}</div>`);
}

function sohCellTemplate(container, options) {
    let sohWarning = options.data.SohWarning;
    console.log("sohWarning", sohWarning);
    if (sohWarning) {
        container.html(`<div class='red'>${options.text}</div>`);
    } else {
        container.text(options.text);
    }
}

function percentCellTemplate(container, options) {
    container.html(`${options.text}<span style='font-size:0.9rem; margin-left:0.2rem;'>%</span>`);
}

function onNumberBoxFocusIn(e) {
    // Find the nested input element and select all text
    const inputElement = e.element.get(0).querySelector("input.dx-texteditor-input");
    if (inputElement) {
        inputElement.select();

        // Prevent the browser's native mouseup event from immediately un-selecting the text
        $(inputElement).one("mouseup", function (event) {
            event.preventDefault();
        });
    }
}

function emptyZeroNumberCellText(cellInfo) {
    if (cellInfo.value === null || cellInfo.value === 0 || cellInfo.value === undefined) {
        return ""; // Set your empty text or placeholder here
    }
    return cellInfo.valueText; // Returns the default formatted value
}

function onRowPreparedInactive(e) {
    if (e.rowType === "data" && e.data.Inactive) {
        e.rowElement.css("color", "var(--red)");
        e.rowElement.css("text-decoration", "line-through");
    }
}

function gridDataLoadedForUsedData(records) {
    // console.log("records", records);
    const notUsedRecord = records.find(i => i.Used == false);
    // console.log("notUsedRecord", notUsedRecord);
    $("#main-grid").dxDataGrid("columnOption", "Action", "visible", notUsedRecord != undefined);
}

function deleteActionCellTemplate(container, options) {
    if (!options.data.Used)
        container.html(`<a href="#" class="icon-link" onclick="deleteRowData(${options.rowIndex})"><i class="bi bi-trash"></i></a>`)
}

String.prototype.lowercaseFirstLetter = function () {
    return this.charAt(0).toLowerCase() + this.slice(1);
};

var deleteUrl = '@Url.Action("Delete", "Item")';

function deleteRowData(rowIndex) {
    let grid = $("#main-grid").dxDataGrid("instance");
    const hasNameColumn = grid.columnOption("Name") !== undefined;
    const visibleRows = grid.getVisibleRows();
    const rowData = visibleRows[rowIndex].data;
    const name = (hasNameColumn ? `'${rowData.Name}'` : "dữ liệu");
    let data = {};
    for (keyField of dataRowKeyFields) {
        data[keyField] = rowData[keyField];
    }
    console.log("deleteRowData", data);

    DevExpress.ui.dialog.confirm(`<i>Bạn có chắc chắn muốn xóa ${name}?</i>`, "Xác nhận").then(function (dialogResult) {
        if (dialogResult) {
            $.ajax({
                url: deleteUrl,
                method: "POST",
                "data": data,
                success: function (result) {
                    reloadData();
                },
                error: function (xhr, status, error) {
                    console.log("xhr", xhr, "status", status, "error", error);
                    DevExpress.ui.dialog.alert("Có lỗi xảy ra. Vui lòng thử lại sau.", "Cảnh báo");
                }
            });
        }
    });
}

function createButton(id, text, width, style, icon, onClickFunc) {
    if (width == undefined)
        width = 160;

    return $(id).dxButton({
        text: text,
        width: width,
        type: "default",
        stylingMode: style,
        icon: icon,
        onClick: onClickFunc
    });
}

function createSaveButton(width, style) {
    if (style == undefined)
        style = "contained";

    return createButton("#save-button", "Lưu", width, style, "bi bi-download", save)
}

function createUndoButton(width, style) {
    if (style == undefined)
        style = "outlined";

    return createButton("#undo-button", "Bỏ qua", width, style, "undo", undo)
}

function createGridAddButton(buttonId, gridId, onClickFunc) {
    if (buttonId == undefined)
        buttonId = "#grid-add-button";

    if (gridId == undefined)
        gridId = "#main-grid";

    if (onClickFunc == undefined) {
        onClickFunc = function () {
            $(gridId).dxDataGrid("addRow");
        };
    }

    return createButton(buttonId, "Thêm", "auto", "text", "bi bi-plus-circle-fill", onClickFunc);
}

function createGridTopAddButton(buttonId, gridId, onClickFunc) {
    if (buttonId == undefined)
        buttonId = "#grid-top-add-button";

    if (gridId == undefined)
        gridId = "#main-grid";

    if (onClickFunc == undefined) {
        onClickFunc = function () {
            $(gridId).dxDataGrid("addRow");
        };
    }

    return createButton(buttonId, "Thêm", "auto", "contained", "bi bi-plus", onClickFunc);
}

function createGridBottomButtonsDiv() {
    return $(`<div id="grid-bottom-buttons" class="pt-2 text-end d-flex justify-content-end gap-3">`);
}

function createBottomButtonsDiv() {
    return $(`<div id="bottom-buttons" class="pt-4 text-center d-flex justify-content-center gap-3">`);
}

function createGridTopButtonsDiv() {
    return $(`<div id="grid-top-buttons" class="pb-3 text-end d-flex justify-content-end gap-3">`);
}

function undo() {
    reloadData();
}

function reloadData() {
    let grid = $("#main-grid").dxDataGrid("instance");
    grid.cancelEditData();
    grid.refresh();
}

function appendUndoSaveButtonsToGrid(gridId) {
    if (gridId == undefined)
        gridId = "#main-grid";

    let buttonsDiv = createBottomButtonsDiv();
    $(gridId).first().append(buttonsDiv);
    buttonsDiv.append($(`<div id="undo-button">`));
    buttonsDiv.append($(`<div id="save-button">`));
    createUndoButton();
    createSaveButton();
}

function addBottomButtonToGrid(gridId, buttonId, buttonText, width, style, buttonIcon, onClickFunc) {
    let buttonsDiv = createBottomButtonsDiv();
    $(gridId).first().append(buttonsDiv);
    buttonsDiv.append($(`<div id="${buttonId}">`));
    return createButton("#" + buttonId, buttonText, width, style, buttonIcon, onClickFunc);
}

function addBottomSaveButtonToGrid(gridId, buttonId, onClickFunc) {
    let buttonsDiv = createBottomButtonsDiv();
    $(gridId).first().append(buttonsDiv);
    buttonsDiv.append($(`<div id="${buttonId}">`));
    return createButton("#" + buttonId, "Lưu", "50%", "contained", "bi bi-download", onClickFunc);
}

function appendAddButtonToGrid(gridId, onClickFunc) {
    if (gridId == undefined)
        gridId = "#main-grid";

    let buttonsDiv = createGridBottomButtonsDiv();
    $(gridId).first().append(buttonsDiv);
    let buttonId = "grid-add-button"
    buttonsDiv.append($(`<div id="${buttonId}">`));
    return createGridAddButton("#" + buttonId, gridId, onClickFunc);
}

function appendTopAddButtonToGrid(gridId, onClickFunc) {
    if (gridId == undefined)
        gridId = "#main-grid";

    let buttonsDiv = createGridTopButtonsDiv();
    $(gridId).first().prepend(buttonsDiv);
    let buttonId = "grid-top-add-button"
    buttonsDiv.append($(`<div id="${buttonId}">`));
    return createGridTopAddButton("#" + buttonId, gridId, onClickFunc);
}

var saveUrl = '@Url.Action("SaveItems", "Item")';
var dataRowKeyFields = ["Id"];

function checkSameRowDataKeys(rowData1, rowData2) {
    // console.log("rowData1", rowData1, "rowData2", rowData2);
    for (keyField of dataRowKeyFields) {
        // console.log("keyField", keyField, "key1", rowData1[keyField], "key2", rowData2[keyField]);
        if (rowData1[keyField] != rowData2[keyField])
            return false;
    }
    return true;
}

function save() {
    let grid = $("#main-grid").dxDataGrid("instance");
    const hasNameColumn = grid.columnOption("Name") !== undefined;

    const editedRows = grid.option("editing.changes").filter(i => i.type == "update").map(i => i.key);
    console.log("editedRows", editedRows);

    var details = []
    let rows = grid.getVisibleRows();
    console.log(rows);
    for (const row of rows) {
        if (row.rowType == "data" && (row.isNewRow || editedRows.find(i => checkSameRowDataKeys(i, row.data)) != undefined)) {
            if (hasNameColumn && (row.data.Name == undefined || row.data.Name == null || row.data.Name == "")) {
                DevExpress.ui.dialog.alert(`Hãy nhập vào đầy đủ Tên.`, "Cảnh báo");
                return;
            }

            if (typeof checkRowDataBeforeSaving === "function" && !checkRowDataBeforeSaving(row.data))
                return;

            details.push(row.data);
        }
    }
    console.log("details", details);

    if (details.length == 0) {
        DevExpress.ui.dialog.alert("Hãy chỉnh sửa dữ liệu trước khi lưu.", "Cảnh báo");
        return;
    }

    DevExpress.ui.dialog.confirm("<i>Bạn có chắc chắn lưu dữ liệu?</i>", "Xác nhận").then(function (dialogResult) {
        if (dialogResult) {
            $.ajax({
                url: saveUrl,
                method: "POST",
                data: { data: details },
                success: function (result) {
                    // DevExpress.ui.dialog.alert("Đã lưu dữ liệu.", "Thông báo").then(function(dialogResult) {
                    reloadData();
                    // });
                },
                error: function (xhr, status, error) {
                    console.log("xhr", xhr, "status", status, "error", error);
                    DevExpress.ui.dialog.alert("Có lỗi xảy ra. Vui lòng thử lại sau.", "Cảnh báo");
                }
            });
        }
    });
}

const detailArrowIcon = "bi bi-arrow-right-circle-fill";

function lotTableCellTemplate(container, options) {
    let itemType = options.data.ItemType;
    let cssColor = cssColorForItemType(itemType);
    let cssClass = "";
    let text = options.text;
    let lot = options.data.Lot;
    if (lot != null && lot != "") {
        container.addClass("gray");
    }

    if (options.column.dataField == "ItemName" || options.column.dataField == "Name") {
        let iconText = "";
        if (lot != null && lot != "") {
            cssClass = "ps-3";
            text = text + " " + lot;
        } else {
            iconText = dotSymbolHtml(cssColor); // •
        }
        text = `${iconText}<a class='text-link' href='/stockcard?item=${options.data.Item}&lot=${options.data.Lot}&year=${options.data.Year}'>${text}</a>`;
    }

    let html = `<div class='${cssClass}'>${text}</div>`
    container.html(html);
}

function dotSymbolHtml(cssColor) {
    return `<span class='${cssColor} me-1 dot-symbol'>●</span>`;
}

// Extend the Number prototype safely
if (!Number.prototype.format) {
    Object.defineProperty(Number.prototype, 'format', {
        value: function (mask) {
            // Check if mask specifically targets the '#,#00' behavior
            if (mask === '#,#00') {
                return new Intl.NumberFormat('en-US', {
                    minimumIntegerDigits: 1,
                    minimumFractionDigits: 0,
                    maximumFractionDigits: 0
                }).format(this);
            }
            if (mask === '#,#00.00') {
                return new Intl.NumberFormat('en-US', {
                    minimumIntegerDigits: 1,
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                }).format(this);
            }

            // Fallback default formatting for other masks
            return this.toLocaleString();
        },
        enumerable: false, // Keeps it hidden from for...in loops
        configurable: true,
        writable: true
    });
}

function expandFirstTableGroup(tableId) {
    const grid = $(tableId ?? "#main-grid").dxDataGrid("instance");
    let rows = grid.getVisibleRows();

    const firstGroupRow = rows.find(row => row.rowType === "group");
    // console.log("firstGroupRow", firstGroupRow);

    if (firstGroupRow != undefined) {
        const firstGroupKey = firstGroupRow.key;
        // console.log("firstGroupRowKey", firstGroupKey);
        grid.expandRow(firstGroupKey);
    }
}

var fixedRightColumnField = "Value";

function onCellPreparedGroupFixedRightColumn(e) {
    if (e.rowType === 'group') {
        if (e.column.dataField === fixedRightColumnField) {
            const value = e.row.data.aggregates[0];
            e.cellElement.text(value.format("#,#00"));
        }
    }
}

function groupCellTemplateTextOnly(container, options) {
    // console.log("groupCellTemplate options", options);
    const groupContent = $(`<div class='d-flex align-items-center ms-3'>${options.text}</div>`)
    container.append(groupContent);
}


Date.prototype.addDays = function (days) {
    var date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}

function addDateOverlay(dateBoxId, onDateChangedFunc) {
    const dateDiv = $(`#${dateBoxId}`);
    const div = $(`<div class="date-overlay-layer">`);
    dateDiv.append(div);

    const dateBox = dateDiv.dxDateBox("instance");
    const date = dateBox.option("value");
    div.html(`<div>${date.ddMMyyyy()}</div>`);

    dateBox.option("onValueChanged", function (e) {
        $(`#${dateBoxId} .date-overlay-layer`).html(`<div>${e.value.ddMMyyyy()}</div>`);

        if (typeof onDateChangedFunc === "function") {
            onDateChangedFunc(e);
        }
    });
}

function createDateBox(dateBoxId, width, height, onDateChangedFunc) {
    $(`#${dateBoxId}`).dxDateBox({
        width: width,
        height: height,
        inputAttr: { "aria-label": "Date" },
        type: "date",
        value: new Date(),
        displayFormat: "dd/MM/yyyy",
        dropDownOptions: {
            position: { of: `#${dateBoxId}`, at: "left bottom", my: "left top", offset: "0 2" }
        }
    });

    addDateOverlay(dateBoxId, onDateChangedFunc);
}

function createTimeBox(id, width, height) {
    $("#" + id).dxDateBox({
        width: width,
        height: height,
        type: "time",
        value: new Date(),
        displayFormat: "HH:mm"
    });
}

function addDateTimeBoxes(containerId, idPrefix, dateWidth, timeWidth, height, onDateChangedFunc) {
    const div = $(`<div class="d-flex mb-3">`);
    $("#" + containerId).append(div);

    const dateField = $(`<div class="data-field">`);
    div.append(dateField);
    dateField.append($(`<div class="field-title">Ngày:</div>`));

    const dateBoxId = idPrefix + "dateBox";
    const dateBoxContainer = $(`<div id="${dateBoxId}">`);
    dateField.append(dateBoxContainer);
    createDateBox(dateBoxId, dateWidth, height, onDateChangedFunc);


    const timeField = $(`<div class="data-field ms-3">`);
    div.append(timeField);
    timeField.append($(`<div class="field-title">Giờ:</div>`));

    const timeBoxId = (idPrefix ?? "") + "timeBox";
    const timeBoxContainer = $(`<div id="${timeBoxId}">`);
    timeField.append(timeBoxContainer);
    createTimeBox(timeBoxId, timeWidth, height);
}

function subLineHeaderCellTemplate(container, options) {
    const texts = options.column.caption.split("\n");
    console.log("subLineHeaderCellTemplate", texts);
    container.html(`${texts[0]}<div style="font-size:0.9rem">${texts[1]}</div>`);
}

function addTabList(containerId, tabId, texts, selectedTabIndex, tabItemClickedFunc) {
    console.log("addTabList", texts);
    const tabDiv = $(`<ul id="${tabId}" class="nav nav-justified">`);
    $(`#${containerId}`).append(tabDiv);
    for (var i = 0; i < texts.length; i++) {
        console.log("addTabList", i, texts[i]);
        const tabItem = $(`<li class="nav-item">`);
        tabDiv.append(tabItem);
        const tabLink = $(`<a class="nav-link" href="#">${texts[i]}</a>`);
        tabItem.append(tabLink);
        const childIndex = i + 1;
        tabLink.on("click", function () {
            $(`#${tabId} .nav-link`).removeClass("active");
            $(`#${tabId} li:nth-child(${childIndex}) .nav-link`).addClass("active");
            tabItemClickedFunc(childIndex - 1);
        });
    }
    setSelectedTabIndex(tabId, selectedTabIndex);
}

function getSelectedTabIndex(tabId) {
    const items = $(`#${tabId}`).children();
    console.log("getSelectedTabIndex", items);
    for (var i = 0; i < items.length; i++) {
        if (items[i].children().first().hasClass("active"))
            return i;
    }
    return 0;
}

function setSelectedTabIndex(tabId, index) {
    $(`#${tabId} .nav-link`).removeClass("active");
    const childIndex = index + 1;
    $(`#${tabId} > :nth-child(${childIndex}) .nav-link`).trigger("click");
}

function isEmpty(text) {
    return text == undefined || text == null || text == "";
}