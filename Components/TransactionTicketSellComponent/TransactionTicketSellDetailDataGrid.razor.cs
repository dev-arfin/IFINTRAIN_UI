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
    // ID header transaksi (FK ke detail)
    [Parameter, EditorRequired] public string? TransactionID { get; set; }

    // Kalau mau disable editing saat status POST/CANCEL
    [Parameter] public bool ReadOnly { get; set; }
    #endregion

    #region Component field
    DataGrid<JsonObject> dataGrid = null!;
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
        // pastikan ID detail ada
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
    
    //  #region GetRowbyTransactionID
    // public async Task GetRowbyTransactionID()
    // {
    //   Loading.Show();
    //   var res = await IFINTEMPLATEClient.GetRow<JsonObject>("TransactionTicketSell", "GetRowbyTransactionID", new { ID = TransactionID });
   
    //   if (res?.Data != null)
    //   {
    //      TransactionID = res.Data;
    //   }
    //    TransactionID = res?.Data?["ID"]?.GetValue<string>();
    //   Loading.Close();
    // }
    // #endregion


  }
}
