# E-Commerce-Application-with-ASP.Net-Core
E-Commerce Application with ASP.Net Core MVC / Entity Framework

# Data Access Layer (DAL)
## Entity Framework 作為 ORM 與資料庫互動
透過`Add-Migration`、`Update-Database`，進行資料庫操作
![image](https://github.com/user-attachments/assets/6682b161-55e3-49b9-a5cf-7f24bfe77206)

## Repository Pattern
將DAL封裝，將具體的資料庫存取實作隱藏於`Repository`
使業務邏輯不直接依賴於資料庫的細節，便於後續修改或是替換資料庫
![image](https://github.com/user-attachments/assets/208e50a1-2df4-4ecf-9316-4ce57cb13198)

IRepository
```csharp=
public interface IRepository<T> where T : class
{
  IEnumerable<T> GetAll(string? includeProperties = null);
  T Get(Expression<Func<T, bool>> filter, string? includeProperties = null);
  void Add(T entity);
  void Remove(T entity);
  void DeleteRange(IEnumerable<T> entitys);
}
```

CategoryRepository
```csharp=
public class CategoryRepository : Repository<Category>, ICategoryRepository
{
	public ApplicationDbContext _dbContext;

	public CategoryRepository(ApplicationDbContext db) : base(db)
	{
		_dbContext = db;
	}
	public void Update(Category obj)
	{
		_dbContext.Categories.Update(obj);
	}
}
```

## Unit Of Work Pattern
通常與`Repository`一起使用
當一個單位操作涉及多個資料表時，`Unit Of Work`可以確保這些操作全部成功或是錯誤時通知，避免數據不一致
![image](https://github.com/user-attachments/assets/3487d615-cd41-47ec-9863-fec48ca1020d)

---

# Areas分區結構
將不同的模塊或功能分隔開來。
分為 Admin、Customer、Identity 等區域，這些區域代表應用程式中的不同功能或使用者角色。
![image](https://github.com/user-attachments/assets/44a7dbce-c5a9-4cb0-9fbb-8a0ccadd51d9)

`[Area]`：在各個Controller設定Area，模塊化應用程式
`[Authorize]`：限制訪問權限，只有已經登入的用戶才能訪問
![image](https://github.com/user-attachments/assets/bb7ac68f-5f6e-4d9c-bc0f-01d105fe3369)

## Identity模組
導入Identity，來實現用戶的身分驗證和授權系統，處理用戶註冊、登入、密碼重置等相關功能。

### Register添加自己需要定義的用戶資料
1. 於`Register InputModel` 添加 (Company屬性、Role屬性)，並且設為`SelectListItem`，提供下拉選單
	![image](https://github.com/user-attachments/assets/d2a20744-d630-4e01-a5e3-c3210356a257)
2. 於`Register OnGetAsync` 添加各類型，並初始化列表資料
![image](https://github.com/user-attachments/assets/f5184299-e7a0-4db6-9251-93b5c6554261)

實現動態生成角色資料和公司列表的功能，並將資料綁訂到`Razor Pages`中

---
# 導入JavaScript函式庫
## Toastr，搭配`TempData`優化通知介面
用來處理使用者創建、刪除資料是否成功之訊息
![image](https://github.com/user-attachments/assets/3b6ef061-a972-4fa9-8bd4-29e7fce83480)
由於這通知只需要讀取一次後就清除，顯是一個短暫的狀態，所以使用`TempData`
```html=
@if (TempData["success"] != null)
{
	<script src="~/lib/jquery/dist/jquery.min.js"></script>
	<script src="//cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/js/toastr.min.js"></script>
	<script type="text/javascript">
		toastr.success('@TempData["success"]');
	</script>
}
@if (TempData["error"] != null)
{
	<script src="~/lib/jquery/dist/jquery.min.js"></script>
	<script src="//cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/js/toastr.min.js"></script>
	<script type="text/javascript">
		toastr.success('@TempData["error"]');
	</script>
}
```

## SweetAlert2，優化產品刪除確認UI
![image](https://github.com/user-attachments/assets/8c509a34-a4b6-4604-b4c7-260092f88a12)

使用了AJAX和前端提示框來處理刪除操作。
當用戶確認刪除後，透過AJAx發送刪除要求，後端處理邏輯，返回Json來通知刪除結果
### 1. 後端`Delete`Action，返回JSON格式的儲存訊息
```csharp=
[HttpDelete]
public IActionResult Delete(int? id)
{
  /// 刪除邏輯
  /// ---
	return Json(new { success = true, message = "Delete Successful" });
}
```

### 2. 前端`JavaScript`刪除函數
```javascript=
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
```
- `SweetAlert2`(`Swal.fire`)：彈出一個確認刪除的對話框，詢問用戶是否確認刪除
- `ShowCaneclButton: true`：允許用戶取消操作

當用戶確認刪除(`result.isConfirmed`)時，使用`jQuery.ajax()`發送一個`Delete`請求到指定`url`。
如果後端刪除成功，通知`toastr.success`，並重新仔入資料表格('dataTable.ajax.reload()`)，以更新刪除後的結果。


# 表單客戶端驗證
引入客戶端驗證腳本，實現表單提交前的即時驗證，避免一些不必要的後端驗證請求，減少伺服器負擔
```html=
@section Scripts {
	@{
		<partial name="_ValidationScriptsPartial" />
	}
}
```
![image](https://github.com/user-attachments/assets/99369f7b-3a13-4a23-a873-373b1a8bffb5)
