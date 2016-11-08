/**
 * ListDataTable
 * Class for creating data tables that also loads datatables dependency 
 * Usage: 
 * var listDataTable = new ListDataTable({
 *      selector: "#pageListTable",
 *      order: [[3, "desc"]]
 *   });                          
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
    this.selector = null, this.order = null;
    if (paramsObj.hasOwnProperty("selector")) {
        this.selector = paramsObj.selector;
    }

    if (paramsObj.hasOwnProperty("order")) {
        this.order = paramsObj.oreder;
    }

    if (this.selector != undefined) {
        this.setupDataTable();
    }
}

ListDataTable.prototype.setupDataTable = function () {
    $(this.selector).DataTable({
        "order": this.order,
        // callback funciton that hides pagination if only one page of results 
        "drawCallback": function () {
            var api = this.api();
            // try to get display length from api context but fall back to 10 
            var rowsPerPage = api.context != null && api.context.length > 0 ? api.context[0]._iDisplayLength : 10;
            if (api.data() != null && api.data().length < rowsPerPage + 1) {
                $(".pagination").hide();
            }
        }
    });
}
