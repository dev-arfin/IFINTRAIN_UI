using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.BookHistoryComponent
{
  public partial class BookHistoryOutstanding
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
      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("BorrowTransactionDetail", "GetRowsForOutStanding", new
      { 
        args.Keyword, 
        args.Offset, 
        args.Limit,
        BookID = BookID
      });
    
      return res?.Data;
    }
    #endregion

   
  }
}