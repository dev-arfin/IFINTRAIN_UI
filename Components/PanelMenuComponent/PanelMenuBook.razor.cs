using Microsoft.AspNetCore.Components;

namespace IFinancing360_TRAINING_UI.Components.PanelMenuComponent
{
	public partial class PanelMenuBook
	{
		[Parameter] public RenderFragment? ChildContent { get; set; }
		[Parameter] public string? UserID { get; set; }


		List<Menu> menus = [
		];

		protected override async Task OnParametersSetAsync()
		{

			if (!string.IsNullOrWhiteSpace(UserID))
			{
				string BasePath = $"inquiry/inquirybook/{UserID}";

				menus.AddRange([
					new Menu { Title = "Info", Url = BasePath, Exact = true },
					new Menu { Title = "Outstanding", Url = $"{BasePath}/outstanding" },
					new Menu { Title = "History", Url = $"{BasePath}/history" },
				]);

				menus = menus.DistinctBy(x => x.Title).ToList();
			}
			await base.OnParametersSetAsync();
		}
	}
}
