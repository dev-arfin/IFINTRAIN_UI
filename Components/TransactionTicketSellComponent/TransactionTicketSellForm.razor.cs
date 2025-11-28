// MasterDetail

using Microsoft.AspNetCore.Components;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using System.Text.Json.Nodes;

namespace IFinancing360_TRAINING_UI.Components.TransactionTicketSellComponent
{
  public partial class TransactionTicketSellForm
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    [Inject] NavigationManager NavigationManager { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? ID { get; set; }

    private string? BusID;
    private string? BusCode;
    private string? BusName;
    private JsonObject? selectedBus;

    private string? DestinationID;
    private string? DestinationName;
    private decimal PriceAmount;
    private int AvailableQuantity;
    #endregion

    #region Component Field
    #endregion

    #region Field
    public JsonObject row = new();

    SingleSelectLookup<JsonObject> buslookup = null!;
    SingleSelectLookup<JsonObject> destinationlookup = null!;
    #endregion

    #region OnParametersSetAsync
    protected override async Task OnParametersSetAsync()
    {
      if (!string.IsNullOrWhiteSpace(ID))
      {
        await GetRow();
      }
      else
      {
        InitNewRow();
      }

      await base.OnParametersSetAsync();
    }
    #endregion

    #region InitNewRow
    private void InitNewRow()
    {
      row = new JsonObject
      {
        ["ID"] = null,
        ["Code"] = "",
        ["TransactionDate"] = DateTime.Now,
        ["BusID"] = null,
        ["BusName"] = "",
        ["DestinationID"] = null,
        ["DestinationName"] = "",
        ["PriceAmount"] = 0m,
        ["AvailableQuantity"] = 0,
        ["Quantity"] = 0,
        ["TotalAmount"] = 0m,
      };

      BusID = null;
      BusCode = null;
      BusName = null;
      DestinationID = null;
      DestinationName = null;
      PriceAmount = 0;
      AvailableQuantity = 0;
    }
    #endregion

    #region OnQuantityChanged
    
private Task OnQuantityChanged(int? val)
{
    int qty = val ?? 0;
    row["Quantity"] = qty;

    decimal price = row["PriceAmount"]?.GetValue<decimal>() ?? 0;
    row["TotalAmount"] = qty * price;

    StateHasChanged();
    return Task.CompletedTask;
}




    #endregion

    #region GetRow
    public async Task GetRow()
    {
      Loading.Show();

      var res = await IFINTEMPLATEClient.GetRow<JsonObject>("TransactionTicketSell","GetRowByID",new { ID = ID });

      if (res?.Data != null)
      {
        row = res.Data;

        BusID = row["BusID"]?.GetValue<string>();
        BusCode = row["Code"]?.GetValue<string>();
        BusName = row["BusName"]?.GetValue<string>();

        DestinationID = row["DestinationID"]?.GetValue<string>();
        DestinationName = row["DestinationName"]?.GetValue<string>();

        PriceAmount = row["PriceAmount"]?.GetValue<decimal>() ?? 0;
        AvailableQuantity = row["AvailableQuantity"]?.GetValue<int>() ?? 0;
      }

      Loading.Close();
    }
    #endregion

    #region LoadDestinationLookUp
    protected async Task<List<JsonObject>?> LoadDestinationLookUp(DataGridLoadArgs args)
    {
      var busId = row["BusID"]?.GetValue<string>() ?? BusID;

      if (string.IsNullOrWhiteSpace(busId))
        return new List<JsonObject>();

      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("MasterBusDestination","GetRowsForDestinationLookUp",new
          {
            Keyword = args.Keyword,
            Offset = args.Offset,
            Limit = args.Limit,
            BusID = busId
          });

      return res?.Data;
    }
    #endregion

    #region LoadBorrowerLookUp (Bus lookup)
    protected async Task<List<JsonObject>?> LoadBorrowerLookUp(DataGridLoadArgs args)
    {
      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("MasterBus","GetRowsForLookup",new
        {
          Keyword = args.Keyword,
          Offset = args.Offset,
          Limit = args.Limit,
        });

      return res?.Data;
    }
    #endregion

    #region OnBusSelected
    private void OnBusSelected(JsonObject? rowBus)
    {
      selectedBus = rowBus;

      BusID = rowBus?["ID"]?.GetValue<string>();
      BusCode = rowBus?["Code"]?.GetValue<string>();
      BusName = rowBus?["BusName"]?.GetValue<string>();

      // Simpan juga ke row agar terkirim ke backend saat Insert/Save
      row["BusID"] = BusID;
      row["BusName"] = BusName;
    }
    #endregion

    #region OnDestinationSelected
    private void OnDestinationSelected(JsonObject? select)
    {
      if (select is null) return;

      DestinationID = select["DestinationID"]?.GetValue<string>();
      DestinationName = select["DestinationName"]?.GetValue<string>() ?? "";
      PriceAmount = select["PriceAmount"]?.GetValue<decimal>() ?? 0m;
      AvailableQuantity = select["TotalQuantity"]?.GetValue<int>() ?? 0;

      row["DestinationID"] = DestinationID;
      row["DestinationName"] = DestinationName;
      row["PriceAmount"] = PriceAmount;
      row["AvailableQuantity"] = AvailableQuantity;
    }
    #endregion

    #region OnSubmit (Insert untuk baru, Save untuk existing)
    private async void OnSubmit(JsonObject data)
    {
      Loading.Show();

      data = SetAuditInfo(data);
      data = row.Merge(data); // gabungkan field dari row (BusID, DestinationID, dll)

      // INSERT (ID null) -> buat transaksi baru
      if (ID == null)
      {
        var res = await IFINTEMPLATEClient.Post("TransactionTicketSell","Insert",data);

        if (res?.Data != null && res.Data["ID"] != null)
        {
          // setelah insert, pindah ke halaman detail dengan ID baru
          var newID = res.Data["ID"]!.GetValue<string>();
          NavigationManager.NavigateTo($"/borrowtransaction/transactionticket/{newID}");
        }
      }
      // SAVE (ID sudah ada) -> hitung ulang quantity & total_amount di backend
      else
      {
        var res = await IFINTEMPLATEClient.Put("TransactionTicketSell","Save",data);

        if (res != null)
        {
          await GetRow();
        }
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region Back
    private void Back()
    {
      NavigationManager.NavigateTo($"/borrowtransaction/transactionticket/");
    }
    #endregion

    #region Post
    private async Task Post()
    {
      Loading.Show();

      var data = SetAuditInfo(row);

      var res = await IFINTEMPLATEClient.Put("TransactionTicketSell","Post",data);

      if (res != null)
      {
        await GetRow(); 
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region Cancel
    private async Task Cancel()
    {
      Loading.Show();

      var data = SetAuditInfo(row);

      var res = await IFINTEMPLATEClient.Put("TransactionTicketSell","Cancel",data);

      if (res != null)
      {
        await GetRow();
      }

      Loading.Close();
      StateHasChanged();
    }
    #endregion

    #region GetReportHTML
private async Task<string> GetReportHTML()
{
    Loading.Show();

    var result = await IFINTEMPLATEClient.GetRow("TransactionTicketSell","GetHTMLForReport",ID                              
    );

    string html = result?.Data?["HTML"]?.GetValue<string>() ?? "<p>No data</p>";

    Loading.Close();

    return html;
}
#endregion

#region PrintByID
private async Task PrintByID(string mimeType)
{
    if (ID == null)
        throw new Exception("ID NOT FOUND");

    var file = await IFINTEMPLATEClient.GetRow<JsonObject>(
        "TransactionTicketSell",
        "PrintDocument",
        new { MimeType = mimeType, ID = ID }
    );

    if (file?.Data != null)
    {
        var data    = file.Data;
        var content = data["Content"]?.GetValueAsByteArray();
        var name    = data["Name"]?.GetValue<string>();
        var type    = data["MimeType"]?.GetValue<string>();

        PreviewFile(content, name, type);  
    }
}
#endregion

  }
}
