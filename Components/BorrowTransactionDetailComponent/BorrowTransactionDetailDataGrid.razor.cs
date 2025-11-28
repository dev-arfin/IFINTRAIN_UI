using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.BorrowTransactionDetailComponent
{
  public partial class BorrowTransactionDetailDataGrid
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    #endregion

    #region Parameter
     [Parameter, EditorRequired] public string? BorrowID { get; set; }
     [Parameter, EditorRequired] public string? Status { get; set; }
     [Parameter, EditorRequired] public EventCallback ReloadParent { get; set; }


    #endregion

    #region Component field
    public DataGrid<JsonObject> dataGrid = null!;

    MultipleSelectLookup<JsonObject>? booklookup = null!;
    #endregion

    #region Class field
     public JsonObject rowHeader = new();
    #endregion

    #region OnInitialized
    protected override async Task OnParametersSetAsync()
    {
      
      await GetRow();

      await base.OnParametersSetAsync();
    }

    #endregion

    #region GetRow
      public async Task GetRow()
      {
        Loading.Show();
        var res = await IFINTEMPLATEClient.GetRow<JsonObject>("BorrowTransaction", "GetRowByID", new { ID = BorrowID });

        if (res?.Data != null)
        {
          rowHeader = res.Data;
        }
        Loading.Close();
      }
    #endregion

    

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("BorrowTransactionDetail", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        BorrowID = BorrowID
      });

      return res?.Data;
    }
    #endregion
    
    #region LoadBookLookUp
		protected async Task<List<JsonObject>?> LoadBookLookUp(DataGridLoadArgs args)
		{
			var res = await IFINTEMPLATEClient.GetRows<JsonObject>("MasterBook", "GetRowsForLookUp", new
      {
        Keyword = args.Keyword,
        Offset = args.Offset,
        Limit = args.Limit,
        BorrowID = BorrowID
      });

			return res?.Data;
		}
    #endregion

    #region Add
    public async void Add()
    {
      var selectedData = booklookup.GetSelected();

      if (!selectedData.Any())
      {
        await NoDataSelectedAlert();
        return;
      }
      var data = booklookup.GetSelected().Select(x => SetAuditInfo(
        new JsonObject
        {
          ["BorrowID"] = BorrowID,
          ["BookID"] = x["ID"]?.GetValue<string>(),
          ["Quantity"] = 1,
          ["RentAmount"] = 0,
        }
      )).ToList();

      Loading.Show();
      var res = await IFINTEMPLATEClient.Post("BorrowTransactionDetail", "Insert", data);
      Loading.Close();

      await dataGrid.Reload();
      await booklookup.Reload();
    }
    #endregion



     #region Save
    public async Task Save(JsonObject data)
    {
      Loading.Show();

      List<JsonObject> list = [];

      foreach (var (key, value) in data)
      {
        var id = key.Split("_").Last();
        var objKey = key.Split("_").First();

        if (string.IsNullOrWhiteSpace(id))
        {
          continue;
        }

        if (list.Find(x => x["ID"]?.GetValue<string>() == id) == null)
        {
          list.Add(SetAuditInfo(
            new JsonObject()
            {
              ["ID"] = id,
            }
          ));
        }
        
        list.Find(x => x["ID"]?.GetValue<string>() == id)![objKey] = value?.DeepClone();
      }
      var res = await IFINTEMPLATEClient.Put("BorrowTransactionDetail", "UpdateByID", list);

      if (res.Result > 0)
      {
        await ReloadParent.InvokeAsync();
      }

      await dataGrid.Reload();
      Loading.Close();
    }
    #endregion

    #region Delete
    private async Task Delete()
    {
      var selectedData = dataGrid.selectedData;

      if (!selectedData.Any())
      {
        
        return;
      }

      bool? result = await Confirm();

      if (result == true)
      {
        Loading.Show();

        await ReloadParent.InvokeAsync();
        var res = await IFINTEMPLATEClient.Delete("BorrowTransactionDetail", "DeleteByID", selectedData.Select(row => SetAuditInfo(row)).ToArray());

        if (res != null)
        {
          if (res.Result > 0)
          {
            await ReloadParent.InvokeAsync();
            await dataGrid.Reload();
          }
        }
        dataGrid.selectedData.Clear();

        Loading.Close();
        StateHasChanged();
      }
    }
    #endregion
  }
}