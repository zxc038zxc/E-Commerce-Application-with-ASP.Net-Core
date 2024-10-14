// 這是一段JS使用了JQuery和DataTables插件來初始化並加載一個資料表
var dataTable;
// $(document).ready() 當DOM完全宰入且準備好時，就會執行其中的程式碼
$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else if (url.includes("completed")) {
        loadDataTable("completed");
    }
    else if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else {
        loadDataTable();
    }
});

// 呼叫DataTables插件來處理資料表的初始化
function loadDataTable(status) {
    dataTable = $("#tblData").DataTable({
        "ajax": { url: "/admin/order/getall?status=" + status },
        "columns": [
            { "data": "id", "width": "5%" },
            { "data": "name", "width": "5%" },
            { "data": "phoneNumber", "width": "15%" },
            { "data": "applicationUser.email", "width": "10%" },
            { "data": "orderStatus", "width": "10%" },
            { "data": "orderTotal", "width": "10%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i></a>
                        </div>`
                },
                "width": "25%"
            }
        ]
    });
}