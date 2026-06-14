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
    const visibleRows = grid.getVisibleRows();
    const rowData = visibleRows[rowIndex].data;

    DevExpress.ui.dialog.confirm(`<i>Bạn có chắc chắn muốn xóa '${rowData.Name}'?</i>`, "Xác nhận").then(function (dialogResult) {
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
        width = 200;

    return $(id).dxButton({
        text: text,
        width: width,
        type: "default",
        stylingMode: style,
        icon: "bi bi-download",
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

function createGridAddButton(buttonId, gridId, width, style) {
    if (buttonId == undefined)
        buttonId = "#grid-add-button";

    if (gridId == undefined)
        gridId = "#main-grid";

    if (width == undefined)
        width = "auto";

    if (style == undefined)
        style = "text";

    return createButton(buttonId, "Thêm", width, style, "bi bi-plus-circle-fill", function () {
        $(gridId).dxDataGrid("addRow");
    });
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

function appendAddButtonToGrid(gridId) {
    if (gridId == undefined)
        gridId = "#main-grid";

    let buttonsDiv = createGridBottomButtonsDiv();
    $(gridId).first().append(buttonsDiv);
    let buttonId = "grid-add-button"
    buttonsDiv.append($(`<div id="${buttonId}">`));
    createGridAddButton("#" + buttonId, gridId);
}

var saveUrl = '@Url.Action("SaveItems", "Item")';

function save() {
    let grid = $("#main-grid").dxDataGrid("instance");

    const editedRows = grid.option("editing.changes");
    const editedIds = editedRows.filter(i => i.type == "update").map(i => i.key.Id);
    // console.log("editedRows", editedRows);
    // console.log("editedIds", editedIds);

    var details = []
    let rows = grid.getVisibleRows();
    console.log(rows);
    for (const row of rows) {
        if (row.rowType == "data" && (row.isNewRow || editedIds.includes(row.data.Id))) {
            if (row.data.Name == undefined || row.data.Name == null || row.data.Name == "") {
                DevExpress.ui.dialog.alert(`Hãy nhập vào đầy đủ Tên.`, "Cảnh báo");
                return;
            }
            details.push(row.data);
        }
    }
    console.log(details);

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