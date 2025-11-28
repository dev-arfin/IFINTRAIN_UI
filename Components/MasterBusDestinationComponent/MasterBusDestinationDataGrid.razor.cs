using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.MasterBusDestinationComponent
{
  public partial class MasterBusDestinationDataGrid
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    #endregion

    #region Parameter
    [Parameter, EditorRequired] public string? BusID { get; set; }
    #endregion

    #region Component field
    public DataGrid<JsonObject> dataGrid = null!;
    MultipleSelectLookup<JsonObject>? destinationLookup = null!;
    #endregion

    #region Class field
    public JsonObject rowHeader = new();
    #endregion

    #region OnParametersSetAsync
    protected override async Task OnParametersSetAsync()
    {
       Console.WriteLine($"[DEBUG] BusID di MasterBusDestinationDataGrid: {BusID}");
      await GetRowHeader();
      await base.OnParametersSetAsync();
    }
    #endregion

    #region GetRowHeader
    public async Task GetRowHeader()
    {
      if (string.IsNullOrWhiteSpace(BusID))
        return;

      Loading.Show();
      var res = await IFINTEMPLATEClient.GetRow<JsonObject>(
        "MasterBus", "GetRowByID", new { ID = BusID });

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
      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("MasterBusDestination", "GetRowsByBusID", new
        {
          args.Keyword,
          args.Offset,
          args.Limit,
          BusID = BusID
        });

      return res?.Data;
    }
    #endregion

    #region LoadDestinationLookUp
    protected async Task<List<JsonObject>?> LoadDestinationLookUp(DataGridLoadArgs args)
    {
      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("MasterDestination", "GetRowsForLookUp", new
        {
          Keyword = args.Keyword,
          Offset = args.Offset,
          Limit = args.Limit,
          BusID = BusID
        });

      return res?.Data;
    }
    #endregion

    #region Add
    public async void Add()
    {
      var selectedData = destinationLookup!.GetSelected();

      if (!selectedData.Any())
      {
        await NoDataSelectedAlert();
        return;
      }

       var data = destinationLookup.GetSelected().Select(x => SetAuditInfo(
        new JsonObject
        {
          ["BusID"] = BusID,
          ["DestinationID"] = x["ID"]?.GetValue<string>(),
          ["TotalQuantity"] = 0,
          ["PriceAmount"] = 0m
        }
      )).ToList();

      Loading.Show();
      var res = await IFINTEMPLATEClient.Post("MasterBusDestination", "InsertList", data);
      Loading.Close();

      await dataGrid.Reload();
      await destinationLookup.Reload();
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
          continue;

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

      var res = await IFINTEMPLATEClient.Put("MasterBusDestination", "UpdateByIDList", list);

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

    // Ambil hanya ID-nya
    var ids = selectedData
      .Select(row => row["ID"]?.GetValue<string>())
      .Where(id => !string.IsNullOrWhiteSpace(id))
      .ToArray();

    var res = await IFINTEMPLATEClient.Delete(
      "MasterBusDestination", "DeleteByID", ids);

    if (res != null && res.Result > 0)
    {
      await dataGrid.Reload();
    }

    selectedData.Clear();
    Loading.Close();
    StateHasChanged();
  }
}
#endregion

  }
}
