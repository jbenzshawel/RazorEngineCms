requirejs(["jquery", "Vue", "Default", "include", "listDataTable", "datatables"], function ($, Vue, Default, include, listDataTable, datatables) {
    "use strict";
    var includeObj = new Include();
    var deleteModalVue = new Vue({
        data: {
            type: "include",
            modalHeader: "Delete Include?",
            name: ""
        }
    });
    var SESSION_KEY = "selectedInclude";
    var includeListTable = new ListDataTable({
        selector: "#includeListTable",
        order: [[3, "desc"]]
    });
    $(function () {
        sessionStorage.clear();
        $("body").on("click", "#confirmDelete", function () {
            confirmDelete();
        });
        $("a.copy-include").click(function (e) {
            e.preventDefault();
            copyInclude($(this).attr("data-includeid"));
        });
        $("a.delete-include").click(function (e) {
            e.preventDefault();
            var selInclude = { id: $(this).attr("data-includeid") };
            sessionStorage.setItem(SESSION_KEY, JSON.stringify(selInclude));
            // set tokens in vue modal and show it
            deleteModalVue.name = $(this).attr("data-includename");
            deleteModalVue.$mount("#deleteModal");
            Vue.nextTick(function () {
                $("#deleteModal").modal("show");
            });
        });
    }); // end document ready function

    // called on confirmation of delete modal
    function confirmDelete() {
        if (SESSION_KEY != null) {
            var storedInclude = null;
            try {
                storedInclude = JSON.parse(sessionStorage.getItem(SESSION_KEY))
            }
            catch (ex) {
                console.log(ex);
            }
            if (storedInclude != null) {
                var response = includeObj.delete(storedInclude.id, "#alertMsgs").responseJSON;
                if (response != null && response.Status != true && typeof (response.Errors) === "object") {
                    response.Errors.forEach(function (err) { console.log(err); });
                } else {
                    $("[data-includeid=\"" + storedInclude.id + "\"]").parent().parent().remove();
                }
                sessionStorage.removeItem(SESSION_KEY);
            }
        }
        $("#deleteModal").modal("hide");
    }
    // called by "a.copy-include" click event 
    function copyInclude(id) {
        var copyResult = includeObj.copy(id, "#alertMsgs");
        if (id > 0) {
            var newInclude = null;
            if (copyResult != null && copyResult.responseJSON.hasOwnProperty("NewInclude")) {
                newInclude = copyResult.responseJSON.NewInclude;
            }
            if (newInclude != null) {
                includeListTable.DataTable().row.add([
                                    newInclude.Id,
                                    newInclude.Name,
                                    newInclude.Type,
                                    _default.dateTime(),
                                    "<a href=\"/CMS/Edit/" + newInclude.Id + "\">Edit</a>",
                                    "<a href=\"#\"" +
                                    "data-includeid=\"" + newInclude.Id + "\"" +
                                    "class=\"copy-include\">" +
                                    "Copy</a>",
                                    "<a href=\"#\"" +
                                    "data-includeid=\"" + newInclude.Id + "\"" +
                                    "class=\"delete-include\">" +
                                    "Delete</a>"]);
                includeListTable.DataTable().column(3).data().sort().draw();
            } // end if copiedPageId > 0 
        } else if (copyResult.responseJSON.Errors != null && copyResult.responseJSON.Errors.length > 0) { // end if copiedIncludeId > 0 
            copyResult.responseJSON.Errors.forEach(function (error) {
                logger.logError(error);
            });
        }
    }
});