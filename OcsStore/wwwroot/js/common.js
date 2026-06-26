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

function onGridContentReadyExpandFirstGroup(e) {
    defaultOnGridContentReady(e);
    expandFirstTableGroup();
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
    console.log("deleteActionCellTemplate", options.data.AllowDelete);
    if (options.data.AllowDelete == undefined || options.data.AllowDelete)
        container.html(`<a href="#" class="icon-link" onclick="deleteRowData(${options.rowIndex});"><i class="bi bi-trash"></i></a>`)
}

function editTimeCellTemplate(container, options) {
    container.html(`<a href="#" class="text-link" onClick="editRowDateTime(${options.rowIndex});">${options.text}</a>`);
}

String.prototype.lowercaseFirstLetter = function () {
    return this.charAt(0).toLowerCase() + this.slice(1);
};

var deleteUrl = '@Url.Action("Delete", "Item")';
var deleteRowKeyFields = [];

function deleteRowData(rowIndex) {
    let grid = $("#main-grid").dxDataGrid("instance");
    const hasNameColumn = grid.columnOption("Name") !== undefined;
    const visibleRows = grid.getVisibleRows();
    const rowData = visibleRows[rowIndex].data;
    const name = (hasNameColumn ? `'${rowData.Name}'` : "dữ liệu");
    let data = {};
    if (deleteRowKeyFields.length > 0) {
        for (keyField of deleteRowKeyFields) {
            data[keyField] = rowData[keyField];
        }
    } else {
        for (var i = 0; i < dataRowKeyFields.length; i++) {
            data[dataRowKeyParamNames[i]] = rowData[dataRowKeyFields[i]];
        }
    }
    console.log("deleteRowData", rowIndex, data);

    DevExpress.ui.dialog.confirm(`<i>Bạn có chắc chắn muốn xóa ${name}?</i>`, "Xác nhận").then(function (dialogResult) {
        if (dialogResult) {
            $.ajax({
                url: deleteUrl,
                method: "POST",
                "data": data,
                success: function (result) {
                    reloadData();
                },
                error: handleAjaxError
            });
        }
    });
}

var editDateTimeUrl = '@Url.Action("EditDateTime", "Item")';
var editDateTimePopup;

function editRowDateTime(rowIndex) {
    let grid = $("#main-grid").dxDataGrid("instance");
    const hasNameColumn = grid.columnOption("Name") !== undefined;
    const visibleRows = grid.getVisibleRows();
    const rowData = visibleRows[rowIndex].data;
    const name = (hasNameColumn ? `'${rowData.Name}'` : "dữ liệu");
    let data = {};

    if (typeof setEditDateTimeParams === "function") {
        setEditDateTimeParams(data, rowData);
    } else {
        for (var i = 0; i < dataRowKeyFields.length; i++) {
            data[dataRowKeyParamNames[i]] = rowData[dataRowKeyFields[i]];
        }
    }
    console.log("editRowDateTime", rowIndex, data);

    let popupContainer = $("#edit-date-time-popup");
    if (popupContainer.length == 0) {
        popupContainer = $(`<div id="edit-date-time-popup" />`)
        $("#main-content").append(popupContainer);
    }

    if (editDateTimePopup == undefined) {
        editDateTimePopup = $("#edit-date-time-popup").dxPopup({
            width: "auto",
            height: "auto",
            visible: false,
            dragEnabled: true,
            hideOnOutsideClick: true,
            showTitle: true,
            title: "Sửa ngày giờ",
            position: {
                my: "top",
                at: "top",
                of: "window",
                offset: { x: 0, y: 100 }
            },
            contentTemplate: function () {
                const container = $("<div />");
                appendDateTimeFields({ container: container, width: "18rem", idPrefix: "edit-popup", direction: "vertical" });
                return container;
            },
            toolbarItems: [{
                widget: "dxButton",
                toolbar: "bottom",
                location: "center",
                options: {
                    width: "auto",
                    height: "auto",
                    text: "Bỏ qua",
                    stylingMode: "outlined",
                    onClick: function () {
                        editDateTimePopup.hide();
                    }
                }
            }, {
                widget: "dxButton",
                toolbar: "bottom",
                location: "center",
                options: {
                    width: "auto",
                    height: "auto",
                    text: "Lưu",
                    stylingMode: "contained",
                    onClick: function () {
                        editRowDateTimeData(data);
                    }
                }
            }]
        }).dxPopup("instance");
    }
    editDateTimePopup.show();
}

function editRowDateTimeData(data) {
    data.date = $("#edit-popup-date-box").dxDateBox("instance").option("value").toISOString();
    data.time = $("#edit-popup-time-box").dxDateBox("instance").option("text");
    console.log("editRowDateTimeData", data);
    $.ajax({ url: editDateTimeUrl, method: "POST", "data": data,
        success: function (result) {
            editDateTimePopup.hide();
            reloadData();
        },
        error: handleAjaxError
    });
}

function appendButton(e) {
    console.log("appendButton", e);
    const div = appendControlDiv(e);
    div.dxButton({
        text: e.text,
        width: e.width,
        type: "default",
        stylingMode: e.style,
        icon: e.icon,
        onClick: e.onClick
    });
    return div.dxButton("instance");
}

function appendControlDiv(e) {
    normalizeIdParam(e);

    const div = $(`<div id="${e.id}" ${e.fill ? "class='flex-fill'" : ""}>`);

    normalizeContainerParam(e);
    e.container.append(div);

    normalizeWidthHeightParam(e);

    return div;
}

function appendSaveButton(e) {
    if (e.action == undefined)
        e.action = "save";
        
    normalizeButtonParam(e);

    return appendButton(e);
}

function createUndoButton(width, style) {
    if (style == undefined)
        style = "outlined";

    return appendButton({ id: "#undo-button", text: "Bỏ qua", width: width, style: style, icon: "undo", onClick: undo });
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

    return appendButton(buttonId, "Thêm", "auto", "contained", "bi bi-plus", onClickFunc);
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
    if (grid.hasEditData())
       grid.cancelEditData();

    grid.refresh();
}

function appendUndoSaveButtonsToGrid(gridId) {
    appendButtonToGrid({ gridId: gridId, level: 2, action: "undo", fill: true, onClick: undo });
    appendButtonToGrid({ gridId: gridId, level: 2, action: "save", fill: true, onClick: save });
}

function appendSaveButtonToGrid(e) {
    if (e == undefined)
        e = {};

    if (e.action == undefined)
        e.action = "save";

    if (e.level == undefined)
        e.level = 2;

    if (e.width == undefined)
        e.width = "50%";

    appendButtonToGrid(e);
}

function appendTopAddButtonToGrid(e) {
    if (e == undefined)
        e = {};

    if (e.action == undefined)
        e.action = "add";

    if (e.position == undefined)
        e.position = "top";

    appendButtonToGrid(e);
}

function appendBottomAddButtonToGrid(e) {
    if (e == undefined)
        e = {};

    if (e.action == undefined)
        e.action = "add";

    if (e.position == undefined)
        e.position = "bottom";

    appendButtonToGrid(e);
}

function gridButtonContainer(e) {
    setupGridButtonParam(e);

    let container = $(`#${e.containerId}`);
    if (container.length === 0) {
        const padding = (e.position == "bottom" ? "pt" : "pb") + "-" + (e.level * 2);
        container = $(`<div id="${e.containerId}" class="${padding} text-${e.align} d-flex justify-content-${e.align} gap-3">`);
        if (e.position == "bottom")
            $(`#${e.gridId}`).first().append(container);
        else
            $(`#${e.gridId}`).first().prepend(container);
    }
    return container;
}

function normalizeButtonParam(e) {
    if (e.action == undefined)
        e.action = "add";

    if (e.idPrefix == undefined)
        e.idPrefix = e.action;

    if (e.idPostfix == undefined)
        e.idPostfix = "button";

    if (e.style == undefined) {
        if (e.action == "undo")
            e.style = "outlined";
        else if (e.level == 1 && e.position == "bottom")
            e.style = "text";
        else
            e.style = "contained";
    }

    if (e.icon == undefined) {
        if (e.action == "save") {
            e.icon = "bi bi-download";
        } else if (e.action == "add") {
            e.icon = (e.style == "contained" ? "bi bi-plus" : "bi bi-plus-circle-fill");
        } else if (e.action == "undo") {
            e.icon = "undo";
        }
    }

    if (e.text == undefined) {
        if (e.action == "save") {
            e.text = "Lưu";
        } else if (e.action == "add") {
            e.text = "Thêm";
        } else if (e.action == "undo") {
            e.text = "Bỏ qua";
        }
    }

    if (e.onClick == null) {
        if (e.action == "save") {
            e.onClick = save;
        }
    }
} 

function setupGridButtonParam(e) {
    if (e.gridId == undefined)
        e.gridId = "main-grid";

    if (e.level == undefined)
        e.level = 1;

    if (e.position == undefined)
        e.position = "bottom";

    if (e.containerId == undefined)
        e.containerId = `${e.gridId}-${e.position}-buttons${e.level}`;

    normalizeButtonParam(e);

    if (e.id == undefined)
        e.id = `${e.containerId}-${e.action}`;

    if (e.align == undefined)
        e.align = (e.level == 1 ? "end" : "center");

    if (e.width == undefined)
        e.width = "auto";

    if (e.height == undefined)
        e.height = "2rem";

    if (e.action == "add" && e.onClick == undefined) {
        e.onClick = function () {
            $(`#${e.gridId}`).dxDataGrid("addRow");
        };
    }
}

function appendButtonToGrid(e) {
    const container = gridButtonContainer(e);
    // const cssClass = (e.fill ? "flex-fill" : "")
    // container.append($(`<div id="${e.id}" class="${cssClass}">`));
    e.container = container;
    return appendButton(e);
}

var saveUrl = '@Url.Action("SaveItems", "Item")';
var dataRowKeyFields = ["Id"];
var dataRowKeyParamNames = ["Id"];
var dataRowDateFields = [];

function checkSameRowDataKeys(rowData1, rowData2) {
    console.log("rowData1", rowData1, "rowData2", rowData2);
    for (keyField of dataRowKeyFields) {
        console.log("keyField", keyField, "key1", rowData1[keyField], "key2", rowData2[keyField]);
        if (rowData1[keyField] != rowData2[keyField])
            return false;
    }
    return true;
}

var goBackAfterSaving = false;

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

            if (typeof allowToSaveRowData === "function" && !allowToSaveRowData(row.data))
                continue;

            for (const dateField of dataRowDateFields) {
                console.log("save dateField", row.data[dateField]);
                if (row.data[dateField] instanceof Date)
                    row.data[dateField] = row.data[dateField].toISOString(); // To send to API param correctly
            }

            if (typeof prepareRowDataBeforeSaving === "function")
                prepareRowDataBeforeSaving(row.data);

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
                    if (typeof doAfterSaving === "function")
                        doAfterSaving();
                    else if (goBackAfterSaving)
                        location.href = goBackUrl;
                    else
                        reloadData();
                    // });
                },
                error: handleAjaxError
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

function addDateOverlay(e, dateDiv) {
    const div = $(`<div class="date-overlay-layer">`);
    dateDiv.append(div);

    const dateBox = dateDiv.dxDateBox("instance");
    const date = dateBox.option("value");
    div.html(`<div>${date.ddMMyyyy()}</div>`);

    dateBox.option("onValueChanged", function (ee) {
        $(`#${e.id} .date-overlay-layer`).html(`<div>${ee.value.ddMMyyyy()}</div>`);

        if (typeof e.onValueChanged === "function") {
            e.onValueChanged(ee);
        }
    });
}

// containerId, idPrefix, dateWidth, timeWidth, height, onDateChanged
function appendDateTimeFields(e) {
    const div = appendFlexContainer(e);
    appendDateField({ container: div, idPrefix: e.idPrefix, width: (e.dateWidth ?? e.width), height: e.height, onValueChanged: e.onDateChanged });
    appendTimeField({ container: div, idPrefix: e.idPrefix, width: (e.timeWidth ?? e.width), height: e.height, onValueChanged: e.onTimeChanged });
}

function appendFlexContainer(e) {
    if (e.direction == undefined)
        e.direction = "horizontal";

    const isVertical = (e.direction == "vertical" || e.direction == "column");
    const flexDirection = (isVertical ? " flex-column" : "");
    const gap = (isVertical ? " gap-1" : " gap-3");
    const div = $(`<div class="d-flex${flexDirection}${gap}">`);

    normalizeContainerParam(e);
    e.container.append(div);
    return div;
}

function appendDateField(e) {
    console.log("appendDateField", e);
    if (e.title == undefined)
        e.title = "Ngày";

    normalizeContainerParam(e);
    const dataField = appendDataFieldContainer({ container: e.container });

    appendFieldTitle({ container: dataField, title: e.title });

    return appendDateBox({ container: dataField, idPrefix: e.idPrefix, idPostfix: "date-box", width: e.width, height: e.height, onValueChanged: e.onValueChanged })
}

function appendTimeField(e) {
    if (e.title == undefined)
        e.title = "Giờ";

    normalizeContainerParam(e);
    const dataField = appendDataFieldContainer({ container: e.container });

    appendFieldTitle({ container: dataField, title: e.title })

    return appendTimeBox({ container: dataField, idPrefix: e.idPrefix, idPostfix: "time-box", width: e.width, height: e.height, onValueChanged: e.onValueChanged })
}

function appendDataFieldContainer(e) {
    normalizeContainerParam(e);
    const div = $(`<div class="data-field">`);
    e.container.append(div);
    return div;
}

function appendFieldTitle(e) {
    normalizeContainerParam(e);
    const text = (e.title != undefined && e.title != "" ? e.title + ":" : "&nbsp;");
    e.container.append($(`<div class="field-title">${text}</div>`));
}

function appendDateBox(e) {
    console.log("appendDateBox", e);
    const div = appendControlDiv(e);
    div.dxDateBox({
        width: e.width,
        height: e.height,
        inputAttr: { "aria-label": "Date" },
        type: "date",
        value: new Date(),
        displayFormat: "dd/MM/yyyy",
        dropDownOptions: {
            position: { of: `#${e.id}`, at: "left bottom", my: "left top", offset: "0 2" }
        }
    });

    const dateBox = div.dxDateBox("instance");
    addDateOverlay(e, div);
    return dateBox;
}

function appendTimeBox(e) {
    const div = appendControlDiv(e);
    div.dxDateBox({
        width: e.width,
        height: e.height,
        type: "time",
        value: new Date(),
        displayFormat: "HH:mm"
    });
    return div.dxDateBox("instance");
}

function appendSelectField(e) {
    normalizeContainerParam(e);
    const dataField = appendDataFieldContainer({ container: e.container });

    appendFieldTitle({ container: dataField, title: e.title });

    e.container = dataField;
    return appendSelectBox(e);
}

function appendSelectBox(e) {
    normalizeContainerParam(e);
    normalizeDataParam(e);
    normalizeSelectBoxParam(e);
    const div = appendControlDiv(e);
    div.dxSelectBox({
        dataSource: e.dataSource,
        width: e.width,
        height: e.height,
        displayExpr: e.nameField,
        valueExpr: e.idField,
        value: e.value,
        acceptCustomValue: e.acceptCustomValue,
        onFocusIn: e.onFocusIn,
        onValueChanged: (e.acceptCustomValue ? function (ee) { selectControlInputText(ee.element); if (typeof e.onValueChanged === "function") e.onValueChanged(ee); } : e.onValueChanged),
        placeholder: "Chọn...",
        dropDownOptions: {
            position: {
                of: `#${e.id}`,
                at: "left bottom",
                my: "left top",
                offset: "0 2"
            }
        }
    });
    const selectBox = div.dxSelectBox("instance");
    return selectBox;
}

function appendCheckField(e) {
    normalizeContainerParam(e);
    const dataField = appendDataFieldContainer({ container: e.container });

    appendFieldTitle({ container: dataField, title: e.title ?? "" });

    e.container = dataField;
    return appendCheckBox(e);
}

function appendCheckBox(e) {
    normalizeContainerParam(e);
    const div = appendControlDiv(e);
    div.dxCheckBox({
        value: e.value ?? false,
        text: e.text,
        onValueChanged: e.onValueChanged
    });
    const checkBox = div.dxCheckBox("instance");
    return checkBox;
}

function appendTabs(e) {
    normalizeContainerParam(e);
    normalizeDataParam(e);

    if (e.itemTemplate == undefined) {
        e.itemTemplate = function (data, index, element) {
            element.append(data.Name);
        }
    }

    if (e.selectionMode == undefined)
        e.selectionMode = "multiple";

    if (e.width == undefined)
        e.width = "100%"

    if (e.height == undefined)
        e.height = "auto";

    console.log("appendTabs", e);
    const div = appendControlDiv(e);
    div.dxTabs({
        dataSource: e.dataSource,
        width: e.width,
        height: e.height,
        displayExpr: e.nameField,
        valueExpr: e.idField,
        showNavButtons: false,
        itemTemplate: e.itemTemplate,
        selectionMode: e.selectionMode,
        onSelectionChanged: e.onSelectionChanged
    });
}

function appendMaterialTagTabs(e) {
    e.dataSource = DevExpress.data.AspNet.createStore({
        key: "Id",
        loadUrl: "/api/Item/GetItemViews",
        loadMethod: "POST",
        loadParams: { GroupId: 2 },
        onBeforeSend(method, ajaxOptions) {
            ajaxOptions.xhrFields = { withCredentials: true };
        }
    })
    appendTabs(e);
}

function appendNumberField(e) {
    normalizeContainerParam(e);
    const dataField = appendDataFieldContainer({ container: e.container });

    appendFieldTitle({ container: dataField, title: e.title });

    e.container = dataField;
    return appendNumberBox(e);
}

function appendNumberBox(e) {
    normalizeNumberFormatParam(e);
    const div = appendControlDiv(e);
    div.dxNumberBox({
        width: e.width,
        height: e.height,
        format: e.format,
        min: e.min,
        max: e.max,
        showSpinButtons: false,
        elementAttr: { "class": "align-right-numberbox" },
        inputAttr: { "class": e.suffix != undefined ? "has-suffix" : "" },
        onFocusIn: onNumberBoxFocusIn,
        onValueChanged: e.onValueChanged
    });
    if (e.suffix != undefined) {
        div.children(".dx-texteditor-container").append($(`<span class="textbox-suffix">${e.suffix}</span>`));
    }
    return div.dxNumberBox("instance");
}

function appendIconField(e) {
    normalizeContainerParam(e);
    const dataField = appendDataFieldContainer({ container: e.container });

    appendFieldTitle({ container: dataField, title: e.title });

    dataField.append($(`<i class="${e.icon}">`));
}

function normalizeNumberFormatParam(e) {
    if (e.decimals == undefined)
        e.decimals = 0;

    if (e.format == undefined)
        e.format = (e.decimals == 0 ? "#,##0" : (e.decimals == 1 ? "#,##0.0" : "#,##0.00"));
}

function normalizeDataParam(e) {
    if (e.idField == undefined)
        e.idField = "Id";

    if (e.nameField == undefined)
        e.nameField = "Name";
}

function normalizeIdParam(e) {
    if (e.idPrefix == undefined)
        e.idPrefix = "";

    if (e.idPostfix == undefined)
        e.idPostfix = "dataField";

    if (e.id == undefined)
        e.id = (e.idPrefix != "" ? e.idPrefix + "-" : "") + e.idPostfix;
}

function normalizeContainerParam(e) {
    if (e.container == undefined)
        e.container = $(`#${e.containerId}`);
}

function normalizeWidthHeightParam(e) {
    if (e.width == undefined)
        e.width = "auto";

    if (e.height == undefined)
        e.height = "auto";
}

function normalizeSelectBoxParam(e) {
    if (e.acceptCustomValue == undefined) {
        e.acceptCustomValue = false;
    } else if (e.acceptCustomValue && e.onFocusIn == undefined) {
        e.onFocusIn = function (e) {
            selectControlInputText(e.element);
        }
    }
}

function selectControlInputText(element) {
    element.find("input.dx-texteditor-input").select();
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

function checkMinLength(text, minLength) {
    return text != undefined && text != null && text.length >= minLength;
}

Date.prototype.getDateWithoutTime = function () {
    return new Date(this.getFullYear(), this.getMonth(), this.getDate());
};

function today() {
    const now = new Date();
    return now.getDateWithoutTime();
}

function loadControlDataSource(control, url, params) {
    $.ajax({
        url: url,
        method: "POST",
        data: params,
        success: function (result) {
            console.log("loadDataFromServer", result)
            control.option("dataSource", result);
        },
        error: function (xhr, status, error) {
            console.log("xhr", xhr, "status", status, "error", error);
            // DevExpress.ui.dialog.alert("Có lỗi xảy ra. Vui lòng thử lại sau.", "Cảnh báo");
        }
    });
}

function getGridSelectedRowData(grid) {
    var items = [];
    let rows = grid.getVisibleRows();
    for (const row of rows) {
        if (row.rowType == "data" && row.data.Selected) {
            items.push(row.data);
        }
    }
    console.log("grid selected rows", items);
    return items;
}

function handleAjaxError (xhr, status, error) {
    console.log("xhr", xhr, "responseText", xhr.responseText, "status", status, "error", error);
    const errorMessage = (xhr.responseText != undefined ? xhr.responseText : "Có lỗi xảy ra. Vui lòng thử lại sau.");
    DevExpress.ui.dialog.alert(errorMessage, "Cảnh báo");
}