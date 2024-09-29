# E-Commerce-Application-with-ASP.Net-Core
E-Commerce Application with ASP.Net Core MVC / Entity Framework

# Data Access Layer (DAL)
## Entity Framework 作為 ORM 與資料庫互動
透過`Add-Migration`、`Update-Database`，進行資料庫操作
![image](https://github.com/user-attachments/assets/04f62d14-bbc5-45f6-8ee7-9b15c7d874bf)

## Repository Pattern
將DAL封裝，將具體的資料庫存取實作隱藏於`Repository`
使業務邏輯不直接依賴於資料庫的細節，便於後續修改或是替換資料庫
![image](https://github.com/user-attachments/assets/bbea9571-a5cc-4574-aa86-2368f7386877)

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
