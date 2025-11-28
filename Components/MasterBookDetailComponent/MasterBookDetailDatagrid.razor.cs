using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.MasterBookDetailComponent
{
  public partial class MasterBookDetailDataGrid
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
    #endregion

    #region Parameter
     [Parameter, EditorRequired] public string? BookID { get; set; }
    #endregion

    #region Component field
    DataGrid<JsonObject> dataGrid = null!;
    #endregion

    #region Class field

    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("MasterBookDetail", "GetRows", new
      {
        args.Keyword,
        args.Offset,
        args.Limit,
        BookID = BookID
      });
    
      return res?.Data;
    }
    #endregion

    #region Add
    private void Add()
    {
      NavigationManager.NavigateTo($"/setting/book/{BookID}/bookdetail/add");
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
    
      if (userConfirm == true)
      {
        Loading.Show();
    
        var idList = dataGrid.selectedData.Select(row => row["ID"]?.GetValue<string>()).ToArray();
    
        await IFINTEMPLATEClient.Delete("Masterbookdetail", "DeleteByID", idList);
    
        await dataGrid.Reload();
        dataGrid.selectedData.Clear();
    
        Loading.Close();
        StateHasChanged();
      }
    }
    #endregion
  }
}