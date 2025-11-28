using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.BookHistoryComponent
{
  public partial class BookHistoryDataGrid
  {
    #region Service
    [Inject] IFINTEMPLATEClient IFINTEMPLATEClient { get; set; } = null!;
        #endregion

        #region Parameter

        #endregion

        #region Component field
        DataGrid<JsonObject> dataGrid = null!;

    #endregion

    #region Class field

    #endregion

    #region LoadData
    protected async Task<List<JsonObject>?> LoadData(DataGridLoadArgs args)
    {
      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("MasterBook", "GetRows", new 
      { 
        args.Keyword, 
        args.Offset, 
        args.Limit 
      });
    
      return res?.Data;
    }
    #endregion

   
  }
}