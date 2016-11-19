requirejs(["jquery", "Vue", "Default", "page", "listDataTable", "datatables"], function ($, Vue, Default, page, listDataTable, datatables) {
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
    var SESSION_KEY = "selectedPage";
    var pageObj = new Page();
    $(function () {
        sessionStorage.clear();
        $("body").on("click", "#confirmDelete", function () {
                    confirmDelete();
                });
        $("a.copy-page").click(function (e) {
                e.preventDefault();
                var pageItem = {
                    pageId: $(this).attr("data-pageid"),
                    pageName: $(this).attr("data-pagename"),
                    pageSection: $(this).attr("data-pagesection")
                };
                copyPage(pageItem.pageId, pageItem.pageSection, pageItem.pageName);
            });
        $("a.delete-page").click(function (e) {
                e.preventDefault();
                var pageItem = {
                    pageId: $(this).attr("data-pageid"),
                    pageName: $(this).attr("data-pagename"),
                    pageSection: $(this).attr("data-pagesection")
                };
                sessionStorage.setItem(SESSION_KEY, JSON.stringify(pageItem));
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
        if (SESSION_KEY != null) {
            var storedPage = null;
            try {
                storedPage = JSON.parse(sessionStorage.getItem(SESSION_KEY));
            } catch (ex) {
                console.log(ex);
            }
            if (storedPage != null && storedPage.hasOwnProperty("pageId")) {
                var deleteResult = pageObj.delete(storedPage.pageId,
                    storedPage.pageSection,
                    storedPage.pageName,
                    "#alertMsgs");
                var deleteStatus = deleteResult != null ? deleteResult.responseJSON.Status : false;
                if (deleteStatus) {
                    pageListTable.deleteRowWithInt(storedPage.pageId);
                } // end if deleteStatus true
            }
            sessionStorage.removeItem(SESSION_KEY);
        } // end if pageName != null
        $("#deleteModal").modal("hide");
    }

    // called by "a.copy-page" click event 
    function copyPage(id, section, name) {
        if (id > 0 && section != null && section.length > 0) {
            var copyResult = pageObj.copy(id, section, name, "#alertMsgs");
            var copiedPageId = 0;
            if (copyResult != null) {
                copiedPageId = copyResult.responseJSON.NewId;
            }
            var newRow = {};
            newRow.view = '<a href="{{ section }}/{{ name }}">View</a>',
            newRow.edit = '<a href="/CMS/Edit/{{ section }}/{{ name }}">Edit</a>';
            newRow.copy = '<a href="#" data-pageid="{{ copiedPageId }}" data-pagename="{{ name }}" data-pagesection="{{ section }}" class="copy-page">Copy</a>';
            newRow.delete = '<a href="#" data-pageid="{{ copiedPageId }}" data-pagename="{{ name }}" data-pagesection="{{ section }}" class="delete-page">Delete</a>';
            $.each(newRow, function(key, value) {
                newRow[key] = value.replace("{{ section }}", section).replace("{{ name }}", name)
                                            .replace("{{ copiedPageId }}", copiedPageId);
            });
            if (copiedPageId > 0) {
                pageListTable.DataTable()
                    .row.add([copiedPageId, section, name, _default.dateTime(), newRow.view, newRow.edit, newRow.copy, newRow.delete]);
                pageListTable.DataTable().column(3).data().sort().draw();
            } else if (copyResult.responseJSON.Errors != null && copyResult.responseJSON.Errors.length > 0) { // end if copiedPageId > 0 
                copyResult.responseJSON.Errors.forEach(function (error) {
                    logger.logError(error);
                });
            }
        } // end if if and section are set 
    }
});