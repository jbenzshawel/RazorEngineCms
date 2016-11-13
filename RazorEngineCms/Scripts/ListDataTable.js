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

// constructor paramsObj is an object with params selector (selector of element) 
// and order (col number order e.g [[1, "desc" ]]
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
        this.setupDataTable();
    }
}

ListDataTable.prototype.setupDataTable = function () {
    var dataTableConfig = {
        // callback funciton that hides pagination if only one page of results 
        "drawCallback": function() {
            var api = this.api();
            // try to get display length from api context but fall back to 10 
            var rowsPerPage = api.context[0]._iDisplayLength || 10;
            if (api.data() != null && api.data().length < rowsPerPage + 1) {
                $(".pagination").hide();
            }
        }
    };
    // add default sort order if set 
    if (typeof (this.order) === "object") {
        dataTableConfig.order = this.order;
    }

    return $(this.selector).DataTable(dataTableConfig);
}

// @param selector (optional) string element selector 
// returns DataTable object for selector or null
ListDataTable.prototype.DataTable = function (selector) {
    // use object property if selector param not set 
    if (this.selector != null && typeof (selector) === "undefined") {
        selector = this.selector;
    }

    if (typeof (selector) === "string" & $(selector).length > 0) {
        return $(selector).DataTable();
    }

    return null;
}

// draws a data table corresponding to the selector 
ListDataTable.prototype.draw = function (pageToBegining) {
    if (typeof (pageToBegining) === "undefined")
        pageToBegining = false;
    this.DataTable().draw(pageToBegining);
}

// Removes row in data table with a column value matching the integer passed in
// @param value is int of value in table column 
ListDataTable.prototype.deleteRowWithInt = function (value) {
    var className = "remove-this-row";
    if (this.addClassToRowWithInt(value, className)) {
        this.DataTable().row("." + className).remove().draw(false);
    }
}

ListDataTable.prototype.copyRowWithInt = function(value) {
    var className = "copy-this-row";
    this.addClassToRowWithInt(value, className);
    var rowData = [];
    $(className).find('td').each(function () {
        rowData.push($(this).text());
    });
    if (rowData.length > 0) {
        this.DataTable().row.add(rowData).draw(false);
    }
}

ListDataTable.prototype.addClassToRowWithInt = function (value, className) {
    var $cols = $(this.selector + " td");
    if ($cols.length > 0) {
        // loop over columns until find match with value (always delete just first row found)
        var callbackMatches = function(key) {
            var column = $cols[key];
            var matchVal = parseInt($(column).text(), 10);
            if (!isNaN(matchVal) && matchVal === parseInt(value, 10)) {
                $(column).parent().addClass(className); // add class to target to row
                return true;
            }
            return false;
        };
        return Object.keys($cols).some(callbackMatches);
    }
    return false;
}