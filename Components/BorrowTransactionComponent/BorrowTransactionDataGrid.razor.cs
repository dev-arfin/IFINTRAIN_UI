using System.Text.Json.Nodes;
using iFinancing360.UI.Components;
using iFinancing360.UI.Helper.APIClient;
using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.BorrowTransactionComponent
{
  public partial class BorrowTransactionDataGrid
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
      var res = await IFINTEMPLATEClient.GetRows<JsonObject>("BorrowTransaction", "GetRows", new 
      { 
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
      NavigationManager.NavigateTo($"/borrowtransaction/transaction/add");
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

                await IFINTEMPLATEClient.Delete("BorrowTransaction", "DeleteByID", idList);

                await dataGrid.Reload();
                dataGrid.selectedData.Clear();

                Loading.Close();
                StateHasChanged();
            }
        }
    #endregion
  }
}