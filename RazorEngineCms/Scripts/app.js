requirejs.config({
    paths: {
        "jquery": "/Scripts/vendor/jquery",
        "bootstrap": "/Scripts/vendor/bootstrap.min",
        "datatables": "/Scripts/vendor/dataTables.min",
        "Vue": "/Scripts/vendor/vue",
        "Default": "/Scripts/default",
        "page": "/Scripts/models/page",
        "include": "/Scripts/models/include",
        "listDataTable": "/Scripts/ListDataTable"
    },
    shim: {
        "bootstrap": { deps: ["jquery"] },
        "listDataTable": {
            deps: ["jquery"],
            exports: "ListDataTable"
        }
    }
});