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