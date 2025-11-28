using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.TransactionTicketSellComponent
{
  public partial class TransactionTicketSellDocDataGrid
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    #endregion

    #region Parameter
    // ID di sini adalah ID header transaksi (TransactionTicketSell.ID)
    [Parameter] public string ID { get; set; } = null!;
    #endregion

    #region Component field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      if (string.IsNullOrWhiteSpace(ID))
        return new List<JsonObject>();

      var res = await IFINTEMPLATEClient.GetRows<JsonObject>(
        "TransactionTicketSellDoc",
        "GetRowsByTransactionID",
        new
        {
          TransactionID = ID,
          args.Keyword,
          args.Offset,
          args.Limit
        });

      return res?.Data;
    }
    #endregion

    #region Add
    private void Add()
    {
      // Route untuk halaman Info/Add.
      // Nanti form-nya bisa pakai route:
      // /borrowtransaction/transactionticket/{ID}/sellingdocument/add
      NavigationManager.NavigateTo($"/borrowtransaction/transactionticket/{ID}/sellingdocument/add");
    }
    #endregion

    #region Delete
    private async Task Delete()
    {
      var selectedData = dataGrid.selectedData;

      if (!selectedData.Any())
      {
        await NoDataSelectedAlert();
        return;
      }

      bool? userConfirm = await Confirm();
      if (userConfirm != true)
        return;

      Loading.Show();

      var idList = selectedData.Select(row => row["ID"]?.GetValue<string>())
        .Where(id => !string.IsNullOrWhiteSpace(id)).ToArray();

      if (idList.Length > 0)
      {
        await IFINTEMPLATEClient.Delete(
          "TransactionTicketSellDoc",
          "DeleteByID",
          idList);
      }

      await dataGrid.Reload();
      dataGrid.selectedData.Clear();

      Loading.Close();
      StateHasChanged();
    }
    #endregion
  }
}
