// 這是一段JS使用了JQuery和DataTables插件來初始化並加載一個資料表
var dataTable;
// $(document).ready() 當DOM完全宰入且準備好時，就會執行其中的程式碼
$(document).ready(function () {
    loadDataTable();
});

// 呼叫DataTables插件來處理資料表的初始化
function loadDataTable() {
    dataTable = $("#tblData").DataTable({
        "ajax": { url: "/Admin/Company/GetAll" },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "streetAddress", "width": "15%" },
            { "data": "city", "width": "15%" },
            { "data": "state", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <a href="/admin/company/upsert?id=${data}" class="btn btn-primary mx-2">
                            <i class="bi bi-pencil-square"></i> Edit</a>
			            <a onClick=Delete('/admin/company/delete/${data}') class="btn btn-danger mx-2">
                            <i class="bi bi-trash-fill"></i> Delete</a>
                        </div>`
                },
                "width": "25%"
            }
        ]
    });
}

function Delete(url){
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'Delete',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}