requirejs(["jquery", "Vue", "Default", "page", "listDataTable", "datatables"], function ($, Vue, Default, page, listDataTable, datatables) {
    "use strict";
    var deleteModalVue = new Vue({
        data: {
            type: "page",
            modalHeader: "Delete Page?",
            name: ""
        }
    });
    var pageListTable = new ListDataTable({
        selector: "#pageListTable",
        order: [[3, "desc"]]
    });
    var pageModel = new Page();
    $(function () {
        $("body").on("click", "#confirmDelete", function () {
            confirmDelete();
        });
        $("body").on("click", "a.copy-page", function (e) {
            e.preventDefault();
            var pageItem = pageModel.listItem(this);
            copyPage(pageItem.pageId, pageItem.pageSection, pageItem.pageName);
        });
        $("body").on("click", "a.delete-page", function (e) {
            e.preventDefault();
            var pageItem = pageModel.listItem(this);
            pageModel.pageItem = pageItem;
            // set tokens in vue modal and show it
            deleteModalVue.name = " /" + pageItem.pageSection + "/" + pageItem.pageName;
            deleteModalVue.$mount("#deleteModal");
            Vue.nextTick(function () {
                $("#deleteModal").modal("show");
            });
       });
    }); // end document ready function

    // called on confirmation of delete modal
    function confirmDelete() {
        var storedPage = null;
        if (pageModel.hasOwnProperty("pageItem")) {
            storedPage = pageModel.pageItem;
        }
        if (storedPage != null && storedPage.hasOwnProperty("pageId")) {
            var deleteCallback = function(data) {
                var deleteStatus = data != null ? data.Status : false;
                if (deleteStatus) {
                    pageListTable.deleteRowWithInt(storedPage.pageId);
                }
                $("#deleteModal").modal("hide");
            };
            pageModel.delete(storedPage.pageId, storedPage.pageSection,
                storedPage.pageName, "#alertMsgs", deleteCallback);
       } 
    }

    // called by "a.copy-page" click event 
    function copyPage(id, section, name) {
        if (id > 0 && section != null && section.length > 0) {
            var copyCallback = function(data) {
                var copiedPageId = 0;
                if (data != null) {
                    copiedPageId = data.NewId;
                }
                var newRow = {};
                newRow.view = '<a href="{{ section }}/{{ name }}">View</a>',
                newRow.edit = '<a href="/CMS/Edit/{{ section }}/{{ name }}">Edit</a>';
                newRow.copy = '<a href="#" data-pageid="{{ copiedPageId }}" data-pagename="{{ name }}" data-pagesection="{{ section }}" class="copy-page">Copy</a>';
                newRow.delete = '<a href="#" data-pageid="{{ copiedPageId }}" data-pagename="{{ name }}" data-pagesection="{{ section }}" class="delete-page">Delete</a>';
                $.each(newRow, function (key, value) {
                    newRow[key] = value.replace("{{ section }}", section).replace("{{ name }}", name)
                                                .replace("{{ copiedPageId }}", copiedPageId);
                });
                if (copiedPageId > 0) {
                    pageListTable.DataTable()
                        .row.add([copiedPageId, section, name, _default.dateTime(),
                            newRow.view, newRow.edit, newRow.copy, newRow.delete]);
                    pageListTable.DataTable().column(3).data().sort().draw();
                } else if (data != null && data.Errors != null && data.Errors.length > 0) {
                    data.Errors.forEach(function (error) {
                        logger.logError(error);
                    });
                }
            };
            pageModel.copy(id, section, name, "#alertMsgs", copyCallback);
            
        } // end if if and section are set 
    }
});