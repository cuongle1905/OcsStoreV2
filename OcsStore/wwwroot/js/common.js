Date.prototype.ddMM = function () {
    const dd = this.getDate().toString().padStart(2, '0');
    // JavaScript months are 0-indexed, so we add 1
    const MM = (this.getMonth() + 1).toString().padStart(2, '0');
    return `${dd}${MM}`;
};

function defaultOnGridContentReady() {
    $(".dx-header-row > td").css("text-align", "center");
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

var deleteUrl = '@Url.Action("Delete", "Item")';

function deleteRowData(rowIndex) {
    let grid = $("#main-grid").dxDataGrid("instance");
    const hasNameColumn = grid.columnOption("Name") !== undefined;
    const visibleRows = grid.getVisibleRows();
    const rowData = visibleRows[rowIndex].data;
    const name = (hasNameColumn ? `'${rowData.Name}'` : "dữ liệu");

    DevExpress.ui.dialog.confirm(`<i>Bạn có chắc chắn muốn xóa ${name}?</i>`, "Xác nhận").then(function (dialogResult) {
        if (dialogResult) {
            $.ajax({
                url: deleteUrl,
                method: "POST",
                data: { id: rowData.Id },
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

function createGridBottomButtonsDiv() {
    return $(`<div id="grid-bottom-buttons" class="pt-2 text-end d-flex justify-content-end gap-3">`);
}

function createBottomButtonsDiv() {
    return $(`<div id="bottom-buttons" class="pt-4 text-center d-flex justify-content-center gap-3">`);
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

function appendAddButtonToGrid(gridId, onClickFunc) {
    if (gridId == undefined)
        gridId = "#main-grid";

    let buttonsDiv = createGridBottomButtonsDiv();
    $(gridId).first().append(buttonsDiv);
    let buttonId = "grid-add-button"
    buttonsDiv.append($(`<div id="${buttonId}">`));
    return createGridAddButton("#" + buttonId, gridId, onClickFunc);
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

const detailArrowSvg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 640 640" fill="currentColor" stroke="currentColor"><path d="M64 320C64 461.4 178.6 576 320 576C461.4 576 576 461.4 576 320C576 178.6 461.4 64 320 64C178.6 64 64 178.6 64 320zM305 441C295.6 450.4 280.4 450.4 271.1 441C261.8 431.6 261.7 416.4 271.1 407.1L358.1 320.1L271.1 233.1C261.7 223.7 261.7 208.5 271.1 199.2C280.5 189.9 295.7 189.8 305 199.2L409 303C418.4 312.4 418.4 327.6 409 336.9L305 441z"/></svg>`