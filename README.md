# IFinancing360 UI

Pembuatan Web berfokus pada folder

-   Components : Berisikan komponen dari masing masing model
-   Pages : Halaman web, berisikan route dan penggunaan komponen

Pengembangan Web dapat menggunakan komponen Radzen dan komponen general yang telah dikembangkan Tim RnD (Dapat dilihat di repo [ iFinancing360.UI](https://github.com/iFinancing/iFinancing360.UI.git)).

Dokumentasi Radzen : https://blazor.radzen.com/dashboard

## Component

Umumnya terdapat 2 Komponen yaitu **DataGrid** (untuk List berbentuk Tabel) dan **Form** (Untuk form insert dan update). Hal ini menyesuaikan kebutuhan.

-   Component diletakkan pada direktori `Components`
-   Standar penamaan Folder dari component ialah {Nama_Model}Component
-   Standar penamaan file component ialah {Nama_Model}{Jenis_Komponen}. Contoh : SysGeneralCodeDataGrid
-   Setiap komponen memiliki file **_(blazor page)_** dengan ekstensi `.razor` dan **_(blazor class)_** dengan ekstensi `.razor.cs`

    -   SysGeneralCodeDataGrid.razor (blazor page): Berisikan HTML
    -   SysGeneralCodeDataGrid.razor.cs (blazor class): Berisikan **_property_** dan **_method_** yang digunakan pada **blazor page**

### Contoh DataGrid komponen

#### SysGeneralCodeDataGrid.razor

```cs
<RadzenStack>
	<!-- #region Toolbar -->
	<RadzenRow Gap="8">
		<RoleAccess Code="">
			<RadzenButton ButtonStyle="ButtonStyle.Primary" Text="Add" Click="@Add" />
		</RoleAccess>
		<RoleAccess Code="">
			<RadzenButton ButtonStyle="ButtonStyle.Danger" Text="Delete" Click="@Delete" Disabled="@(Loading.IsLoading)" />
		</RoleAccess>
	</RadzenRow>
	<!-- #endregion -->

	<!-- #region List Data -->
	// DataGrid dari iFinancing360.UI.Components.DataGrid
	<DataGrid ID="SysGeneralCodeDataGrid" @ref=@dataGrid TItem="SysGeneralCodeModel" LoadData="LoadData"
		AllowSelected="true">
		<DataGridColumn TItem="SysGeneralCodeModel" Property="Code" Title="Code" Width="20%"
			Link="@(row => $"/systemsetting/generalcode/{row.ID}")" />
		<DataGridColumn TItem="SysGeneralCodeModel" Property="Description" Title="Description" Width="60%" />
		<DataGridColumn TItem="SysGeneralCodeModel" Property="IsEditable" Title="Editable" Width="20%"
			TextAlign="TextAlign.Center" FormatString="YN" /> // FormatString "YN" untuk menampilkan "YES" atau "NO" untuk properti yang bersifat status
	</DataGrid>
	<!-- #endregion -->
</RadzenStack>
```

#### SysGeneralCodeDataGrid.razor.cs

```cs
using Data.Model;
using Data.Service;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_PMS_UI.Components.SysGeneralCodeComponent
{
	public partial class SysGeneralCodeDataGrid
	{
		#region Service
		[Inject] IFINSYSClient IFINSYSClient { get; set; } = null!;
		#endregion

		#region Component Field
		DataGrid<SysGeneralCodeModel> dataGrid = null!;
		#endregion

		#region Field

		#endregion

		#region OnInitialized
		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
		}
		#endregion

		#region LoadData
		protected async Task<List<SysGeneralCodeModel>?> LoadData(string keyword)
		{
			return await IFINSYSClient.GetRows(keyword, 0, 100);
		}
		#endregion

		#region Add
		private void Add()
		{
			NavigationManager.NavigateTo($"/systemsetting/generalcode/add");
		}
		#endregion

		#region Delete
		private async void Delete()
		{
			var selectedData = dataGrid.selectedData;

			if (!selectedData.Any())
			{
				await NoDataSelectedAlert();
				return;
			}

			bool? result = await Confirm();

			if (result == true)
			{
				Loading.Show();

				await IFINSYSClient.Delete("SysGeneralCode", "DeleteByID",dataGrid.selectedData.Select(row => row.ID).ToArray());

				await dataGrid.Reload();
				dataGrid.selectedData.Clear();

				Loading.Close();

				StateHasChanged();
			}
		}
		#endregion
	}
}
```

### Contoh Form komponen

#### SysGeneralCodeForm.razor

```cs
<!-- #region Form -->
<RadzenTemplateForm TItem="SysGeneralCodeModel" Data="@row" Submit=@OnSubmit>
	<RadzenStack>
		<!-- #region Toolbar -->
		<RadzenRow Gap="8">
			<RoleAccess Code="">
				<RadzenButton ButtonType="ButtonType.Submit" ButtonStyle="ButtonStyle.Primary" Text="Save"
					Disabled=@(Loading.IsLoading) />
			</RoleAccess>

			@if (ID != null)
			{
				<RoleAccess Code="">
					<RadzenButton ButtonStyle=@(row.IsEditable == 1 ? ButtonStyle.Danger : ButtonStyle.Success)
						Text=@(row.IsEditable == 1 ? "Non Editable" : "Editable") Click="@ChangeEditable" />
				</RoleAccess>
			}

			<RadzenButton ButtonStyle="ButtonStyle.Danger" Text="Back" Click="@Back" />
		</RadzenRow>
		<!-- #endregion -->

		<RadzenStack>
			<RadzenRow>
				<!-- #region Code -->
				<FormFieldTextBox Label="Code" Name="Code" @bind-Value="@row.Code" Max="50" Required="true" Disabled=@(ID != null) />
				<!-- #endregion -->

				<!-- #region Description -->
				<FormFieldTextArea Label="Description" Name="Description" @bind-Value="@row.Description" Max="4000"
					Required="true" />
				<!-- #endregion -->

				<!-- #region Is Editable -->
				<FormFieldSwitch Name="IsEditable" Label="Editable" @bind-Value="@row.IsEditable" Disabled />
				<!-- #endregion -->
			</RadzenRow>
		</RadzenStack>
	</RadzenStack>
</RadzenTemplateForm>
<!-- #endregion -->
```

#### SysGeneralCodeForm.razor.cs

```cs
using Data.Model;
using Data.Service;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_PMS_UI.Components.SysGeneralCodeComponent
{
	public partial class SysGeneralCodeForm
	{
		#region Service
		[Inject] IFINSYSClient IFINSYSClient { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter] public string? ID { get; set; }
		#endregion

		#region Component Field
		#endregion

		#region Field
		public SysGeneralCodeModel row = new();
		#endregion

		#region OnInitialized
		protected override async Task OnInitializedAsync()
		{
			if (ID != null)
			{
				await GetRow();
			}
			else
			{
				row.IsEditable = 1;
			}
			await base.OnInitializedAsync();
		}
		#endregion

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();
			row = await IFINSYSClient.GetRowByID(ID) ?? new();
			Loading.Close();
			StateHasChanged();
		}
		#endregion

		#region ChangeEditable
		private async Task ChangeEditable()
		{
			if (ID != null)
			{
				Loading.Show();
				var res = await IFINSYSClient.ChangeEditableStatus(row);

				if (res != null)
				{
					await GetRow();
					Loading.Close();
				}

				StateHasChanged();
			}
		}
		#endregion

		#region OnSubmit
		private async void OnSubmit(JsonObject data)
		{
			Loading.Show();

			#region Insert
			if (ID == null)
			{
				var res = await IFINSYSClient.Insert(row);

				if (res?.Data != null)
				{
					NavigationManager.NavigateTo($"/systemsetting/generalcode/{res.Data.ID}", true);
				}
			}
			#endregion

			#region Update
			else
			{
				await IFINSYSClient.UpdateByID(row);
			}

			Loading.Close();
			StateHasChanged();
			#endregion
		}
		#endregion

		#region Back
		private void Back()
		{
			NavigationManager.NavigateTo("/systemsetting/generalcode");
		}
		#endregion

	}
}
```

_NOTE : Jika Model tersebut Merupakan Child dari suatu Parent maka pada Form nya juga mencantumkan Sedikit informasi Parent. Umumnya ialah Code dan Deskripsi atau properti lain yang mudah dimengerti User_

### Contoh Form komponen Child

#### SysGeneralSubcodeForm.razor

```cs
<!-- #region Form -->
<RadzenTemplateForm TItem="SysGeneralSubcodeModel" Data="@row" Submit=@OnSubmit>
	<RadzenStack>
		<!-- #region Toolbar -->
		<RadzenRow Gap="8">
			<RadzenButton ButtonType="ButtonType.Submit" ButtonStyle="ButtonStyle.Primary" Text="Save" Disabled="@(Loading.IsLoading)" />
			@if (ID != null)
			{
				<RadzenButton ButtonStyle=@(row.IsActive == 1 ? ButtonStyle.Danger : ButtonStyle.Success) Text=@(row.IsActive ==
				1 ? "Non Active" : "Active") Click="@ChangeActive" />
			}
			<RadzenButton ButtonStyle="ButtonStyle.Danger" Text="Back" Click="@Back" />
		</RadzenRow>
		<!-- #endregion -->

		<RadzenStack>
			// Menampilkan General Code (Parent dari SubCode)
			<!-- #region General Code -->
			<RadzenRow>
				<!-- #region Code -->
				<FormFieldTextBox Label="General Code Code" Name="Code" @bind-Value="@rowGeneralCode.Code" Max="50"
					Required="true" Disabled />
				<!-- #endregion -->
				<!-- #region Description -->
				<FormFieldTextBox Label="General Code Description" Name="Description"
					@bind-Value="@rowGeneralCode.Description" Max="50" Required="true" Disabled />
				<!-- #endregion -->
			</RadzenRow>
			<!-- #endregion -->

			<!-- #region Sub Code -->
			<RadzenRow>
				<!-- #region Code -->
				<FormFieldTextBox Label="Code" Name="Code" @bind-Value="@row.Code" Max="50" Required="true" Disabled=@(ID !=
					null) />
				<!-- #endregion -->

				<!-- #region Description -->
				<FormFieldTextArea Label="Description" Name="Description" @bind-Value="@row.Description" Max="4000"
					Required="true" />
				<!-- #endregion -->

				<!-- #region Is Active -->
				<FormFieldSwitch Name="IsActive" Label="Active" @bind-Value="@row.IsActive" Disabled />
				<!-- #endregion -->

				<!-- #region OrderKey -->
				<FormFieldNumeric Label="Order Key" Name="OrderKey" @bind-Value="@row.OrderKey" Min="0" Required="true" />
				<!-- #endregion -->
			</RadzenRow>
			<!-- #endregion -->
		</RadzenStack>
	</RadzenStack>
</RadzenTemplateForm>
<!-- #endregion -->
```

#### SysGeneralSubcodeForm.razor.cs

```cs
using Data.Model;
using Data.Service;
using iFinancing360.UI.Components;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_PMS_UI.Components.SysGeneralSubcodeComponent
{
	public partial class SysGeneralSubcodeForm
	{
		#region Service
		[Inject] IFINSYSClient IFINSYSClient { get; set; } = null!;
		[Inject] SysGeneralSubcodeService SysGeneralSubcodeService { get; set; } = null!;
		#endregion

		#region Parameter
		[Parameter, EditorRequired] public string? ID { get; set; }
		[Parameter, EditorRequired] public string? GeneralCodeID { get; set; }
		#endregion

		#region Component Field
		#endregion

		#region Field
		public SysGeneralSubcodeModel row = new();
		public SysGeneralCodeModel rowGeneralCode = new();
		#endregion

		#region OnInitialized
		protected override async Task OnInitializedAsync()
		{
			if (ID != null)
			{
				await GetRow();
			}
			else
			{
				row.IsActive = 1;
				row.GeneralCodeID = GeneralCodeID;
			}

			await GetRowGeneralCode();
			await base.OnInitializedAsync();
		}
		#endregion

		#region GetRowGeneralCode
		public async Task GetRowGeneralCode()
		{
			Loading.Show();

			rowGeneralCode = await IFINSYSClient.GetRowByID(GeneralCodeID) ?? new();
			StateHasChanged();

			Loading.Close();
		}

		#endregion

		#region GetRow
		public async Task GetRow()
		{
			Loading.Show();

			row = await SysGeneralSubcodeService.GetRowByID(ID) ?? new();
			StateHasChanged();

			Loading.Close();
		}
		#endregion

		#region ChangeActive
		private async Task ChangeActive()
		{
			if (ID != null)
			{
				Loading.Show();
				var res = await SysGeneralSubcodeService.ChangeStatus(row);

				if (res != null)
				{
					await GetRow();
				}

				Loading.Close();
				StateHasChanged();
			}
		}
		#endregion

		#region OnSubmit
		private async void OnSubmit(JsonObject data)
		{
			Loading.Show();

			#region Insert
			if (ID == null)
			{
				var res = await SysGeneralSubcodeService.Insert(row);

				if (res?.Data != null)
				{
					NavigationManager.NavigateTo($"/systemsetting/generalcode/{GeneralCodeID}/generalsubcode/{res.Data.ID}", true);
				}
			}
			#endregion

			#region Update
			else
			{
				var res = await SysGeneralSubcodeService.UpdateByID(row);
			}
			#endregion

			Loading.Close();
			StateHasChanged();
		}
		#endregion

		#region Back
		private void Back()
		{
			NavigationManager.NavigateTo($"/systemsetting/generalcode/{GeneralCodeID}");
		}
		#endregion
	}
}

```

## Pages

-   File dibuat pada direktori `Pages` dengan nama direktori menyesuaikan **Menu**
-   Direktori Page memiliki _Suffix_ `Page`. Contoh Direktori : `GeneralCodePage`
-   Penamaan **File** bergantung pada **nama menu** atau **kegunaannya** sebagai contoh:

    -   Halaman yang menampilkan **list** SysGeneralCode : GeneralCodeList.razor
    -   Halaman yang menampilkan **info** SysGeneralCode : GeneralCodeInfo.razor
    -   dst.

-   **_Blazor Page_** memiliki routing (`@page "/namaroute"`) pada bagian atas **_blazor page_**

    ```cs
    @* File : GeneralCodePage\GeneralCodeList.razor *@

    @* Route *@
    @page "/systemsetting/generalcode"

    @*
    * HTML Element
    *@
    ```

### Standar Routing

-   Pemberian route dilakukan dengan menambahkan `@page "url"` pada bagian atas **Razor Page**
    ```html
    <!-- File : Menu\GeneralCodeList.razor -->
    @page "/systemsetting/generalcode"
    ```
-   Route sesuai dengan yang terdaftar pada `IFINSYS`
-   Contoh menu `General Code` (Child menu dari menu **System Setting**) pada Module `IFINSYS`

    -   Halaman List : `'systemsetting/generalcode'`
    -   Halaman Detail (Add) : `'systemsetting/generalcode/add'`
    -   Halaman Detail (Update) : `'systemsetting/generalcode/{ID}'`

-   Jika suatu menu bersarang atau memiliki halaman info lagi untuk childnya (General Code Info yang memiliki General Subcode Info) maka parameter pertama disesuaikan dengan _nama menu sebelumnya_ dan _barulah parameter kedua berupa `'{ID}'`_, Contoh kasus halaman `GeneralCode` yang memiliki detail `GeneralSubcode` :

    -   Halaman Info GeneralSubcode (Add) : `'systemsetting/generalcode/{GeneralCodeID}/generalsubcode/add'`
    -   Halaman Info GeneralSubcode (Update) : `'systemsetting/generalcode/{GeneralCodeID}/generalsubcode/{ID}'`

## Contoh Pages

### List Page

```html
<!-- File: Pages/GeneralCodePage/GeneralCodeList.razor  -->
<!-- Route -->
@page "/systemsetting/generalcode"

<!-- Import Component yang digunakan -->
@using IFinancing360_PMS_UI.Components.SysGeneralCodeComponent

<RoleAccess Code="">
    <PageContainer>
        <title Text="General Code List" />

        <SysGeneralCodeDataGrid />
    </PageContainer>
</RoleAccess>
<!-- #endregion Parent Lookup -->
```

### Info Page

`NOTE` :

-   Route Endpoint Halaman Info untuk insert/add : `/add`
-   Route Endpoint Halaman Info untuk update : `/{ID}`
-   Gunakan atribut `[Parameter]` untuk menangkap `Route Parameter`

```html
<!-- File: Pages/GeneralCodePage/GeneralCodeInfo.razor  -->
<!-- Route -->
@page "/systemsetting/generalcode/add" @page "/systemsetting/generalcode/{ID}"

<!-- Import Component yang digunakan -->
@using IFinancing360_PMS_UI.Components.SysGeneralCodeComponent @using
IFinancing360_PMS_UI.Components.SysGeneralSubcodeComponent

<RoleAccess Code="">
    <PageContainer>
        <title Text="General Code Info" />

        <SysGeneralCodeForm />

        @if (ID != null) {
        <SysGeneralSubcodeDataGrid GeneralCodeID="@ID" />
        }
    </PageContainer>
</RoleAccess>

@code {
<!-- Route Parameter -->
[Parameter] public string? ID { get; set; } }
```

### Info Page (Child)

```html
<!-- File: Pages/GeneralCodePage/GeneralSubcodeInfo.razor  -->
<!-- Route -->
@page "/systemsetting/generalcode/{GeneralCodeID}/generalsubcode/add" @page
"/systemsetting/generalcode/{GeneralCodeID}/generalsubcode/{ID}"

<!-- Import Component yang digunakan -->
@using IFinancing360_PMS_UI.Components.SysGeneralSubcodeComponent

<RoleAccess Code="">
    <PageContainer>
        <SysGeneralSubcodeForm GeneralCodeID="@GeneralCodeID" ID="@ID" />
    </PageContainer>
</RoleAccess>

@code {
<!-- Route Parameter -->
[Parameter] public string? ID { get; set; } [Parameter] public string?
GeneralCodeID { get; set; } }
```
