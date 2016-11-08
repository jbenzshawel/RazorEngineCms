/**
 * ListDataTable
 * Class for creating data tables. DataTables dependency loaded  
 * before creating class. Example: 
 * var listDataTable = new ListDataTable({
 *      selector: "#pageListTable",
 *      order: [[3, "desc"]]
 * });                          
 */

// load datatables 
$.ajax({
    url: "//cdn.datatables.net/v/bs-3.3.6/dt-1.10.12/datatables.min.js",
    type: "GET",
    async: false
});

var ListDataTable = function (paramsObj) {
    if (typeof (paramsObj) != "object" || paramsObj == null) {
        logger.logError("Error: paramsObj cannot be null in ListDataTable.");
        return;
    }
    this.selector = null, this.order = null, this.dataTable = null;
    if (paramsObj.hasOwnProperty("selector")) {
        this.selector = paramsObj.selector;
    }

    if (paramsObj.hasOwnProperty("order")) {
        this.order = paramsObj.order;
    }

    if (this.selector != undefined) {
        this.dataTable = this.setupDataTable();
    }
}

ListDataTable.prototype.setupDataTable = function () {
    return $(this.selector).DataTable({
        "order": this.order,
        // callback funciton that hides pagination if only one page of results 
        "drawCallback": function () {
            var api = this.api();
            // try to get display length from api context but fall back to 10 
            var rowsPerPage = api.context[0]._iDisplayLength || 10;
            if (api.data() != null && api.data().length < rowsPerPage + 1) {
                $(".pagination").hide();
            }
        }
    });
}

ListDataTable.prototype.getDataTable = function () {
    if (this.selector != null) {
        this.dataTable = $(this.selector).DataTable();
        return this.dataTable;
    }
    return null;
}

// draws a data table corresponding to the selector 
ListDataTable.prototype.draw = function() {
    this.getDataTable().draw();
}

// Removes row in data table with a column value matching the integer passed in
// @param value is int of value in table column 
ListDataTable.prototype.deleteRowWithInt = function (value) {
    var cols = $("#pageListTable td");
    for (var i = 0, column; column = cols[i++];) {
        var matchVal = parseInt($(column).text(), 10);
        if (!isNaN(matchVal) && matchVal === parseInt(value, 10)) {
            $(column).parent().addClass("remove-this-row"); // add class to target to row
            break; 
        }
    }
    if ($(".remove-this-row").length > 0) {
        this.getDataTable().row('.remove-this-row').remove().draw(false);
    }
}