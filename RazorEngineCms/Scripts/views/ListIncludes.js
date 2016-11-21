requirejs(["jquery", "Vue", "Default", "include", "listDataTable", "datatables"], function ($, Vue, Default, include, listDataTable, datatables) {
    "use strict";
    var includeModel = new Include();
    var deleteModalVue = new Vue({
        data: {
            type: "include",
            modalHeader: "Delete Include?",
            name: ""
        }
    });
    var includeListTable = new ListDataTable({
        selector: "#includeListTable",
        order: [[3, "desc"]]
    });
    $(function () {
        $("body").on("click", "#confirmDelete", function () {
            confirmDelete();
        });
        $("body").on("click", "a.copy-include", function (e) {
            e.preventDefault();
            copyInclude($(this).attr("data-includeid"));
        });
        $("body").on("click", "a.delete-include", function (e) {
            e.preventDefault();
            includeModel.selInclude = { id: $(this).attr("data-includeid") };
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
        var storedInclude = null;
        if (includeModel.hasOwnProperty("selInclude")) {
            storedInclude = includeModel.selInclude;
        }

        if (storedInclude != null) {
            var response = includeModel.delete(storedInclude.id, "#alertMsgs").responseJSON;
            if (response != null && response.Status != true && typeof (response.Errors) === "object") {
                response.Errors.forEach(function (err) { console.log(err); });
            } else {
                $("[data-includeid=\"" + storedInclude.id + "\"]").parent().parent().remove();
            }
        }
        
        $("#deleteModal").modal("hide");
    }
    // called by "a.copy-include" click event 
    function copyInclude(id) {
        var copyResult = includeModel.copy(id, "#alertMsgs");
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