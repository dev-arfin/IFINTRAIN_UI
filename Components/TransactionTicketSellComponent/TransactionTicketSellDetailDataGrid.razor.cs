using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.TransactionTicketSellComponent
{
  public partial class TransactionTicketSellDetailDataGrid
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? TransactionID { get; set; }
    [Parameter, EditorRequired] public string? ID { get; set; }

    // Kalau mau disable editing saat status POST/CANCEL
    [Parameter] public bool ReadOnly { get; set; }
    #endregion

    #region Component field
    DataGrid<JsonObject> dataGrid = null!;
    public JsonObject rowTransaction { get; set; } = [];
     public JsonObject row = new();
    #endregion

    #region Class field
    // cache data supaya bisa di-loop saat Save
    private List<JsonObject> rows = new();
    #endregion


// TransactionTicketSellDetailDataGrid.razor.cs

        #region LoadData
protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
{
    if (string.IsNullOrWhiteSpace(TransactionID))
        return new List<JsonObject>();

    var res = await IFINTEMPLATEClient.GetRows<JsonObject>("TransactionTicketSellDetail","GetRowsByTransactionID", new 
        {
            args.Keyword,
            args.Offset,
            args.Limit,
            TransactionID
        });

    rows = res?.Data ?? new List<JsonObject>();
    return rows;
}
#endregion

#region OnParametersSetAsync
protected override async Task OnParametersSetAsync()
{
    // kalau TransactionID ada, baru load transaksi induk
    if (!string.IsNullOrWhiteSpace(TransactionID))
    {
        await GetRowTransaction();
    }

    await base.OnParametersSetAsync();
}
#endregion

private bool IsLocked =>
    rowTransaction["Status"]?.GetValue<string>() is "POST" or "CANCEL";


     #region GetRow
    public async Task GetRow()
    {
      Loading.Show();
      var res = await IFINTEMPLATEClient.GetRow<JsonObject>("TransactionTicketSellDoc", "GetRowByID", ID);

      if (res?.Data != null)
      {
        row = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion





    #region OnCellChanged
    private Task OnCellChanged(JsonObject row, string field, object? value)
    {
      row[field] = value?.ToString();
      return Task.CompletedTask;
    }
    #endregion

    #region Save
    private async Task Save()
    {
      if (ReadOnly) return;
      if (rows.Count == 0) return;

      foreach (var item in rows)
      {

        var id = item["ID"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(id))
          continue;

        JsonObject data = new()
        {
          ["ID"]            = id,
          ["TransactionID"] = item["TransactionID"]?.GetValue<string>(),
          ["Code"]          = item["Code"]?.GetValue<string>(),
          ["NIK"]           = item["NIK"]?.GetValue<string>() ?? "",
          ["Name"]          = item["Name"]?.GetValue<string>() ?? "",
          ["PhoneNO"]       = item["PhoneNO"]?.GetValue<string>() ?? ""
        };

        data = SetAuditInfo(data);

        await IFINTEMPLATEClient.Put(
          "TransactionTicketSellDetail","UpdateByID",          
          data
        );
      }

    

      await dataGrid.Reload();
    }
    #endregion
    
#region GetRowTransaction
    public async Task GetRowTransaction()
    {
      Loading.Show();
      var res = await IFINTEMPLATEClient.GetRow<JsonObject>("TransactionTicketSell", "GetRowByID", new
      {
        ID = TransactionID
      });

      if (res?.Data != null)
      {
        rowTransaction = res.Data;
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion


  }
}
