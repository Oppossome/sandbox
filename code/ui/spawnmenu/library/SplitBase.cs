using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
public class SplitBase : Panel
{
	public Panel CategoryContent { get; set; }
	public Panel CategoryList { get; set; }

	Dictionary<string, Panel> CategoryPanels = new();
	public string CurrentCategory;
	public Panel CurrentContent;

	public void Register( string catName, Func<Panel> catBuilder, Action callback = null )
	{
		Button catButton = CategoryList.Add.Button( catName, () =>
		{
			SwitchCategory( catName, catBuilder );
			callback?.Invoke();
		} );

		catButton.BindClass( "current",
			() => CurrentCategory == catName );

		if ( CategoryList.ChildrenCount == 1 )
			SwitchCategory( catName, catBuilder );
	}

	public void RegisterButton( string catName, Action callback = null )
	{
		Button catButton = CategoryList.Add.Button( catName, () =>
			callback?.Invoke() );
	}

	protected void SwitchCategory( string catName, Func<Panel> catBuilder )
	{
		if ( CurrentContent is not null ) CurrentContent.Style.Display = DisplayMode.None;
		CurrentCategory = catName;

		if ( CategoryPanels.TryGetValue( catName, out Panel catPanel ) )
		{
			catPanel.Style.Display = DisplayMode.Flex;
			CurrentContent = catPanel;
			return;
		}

		if ( catBuilder.Invoke() is not Panel newPanel )
		{
			CurrentContent = null;
			return;
		}

		CategoryContent.AddChild( newPanel );
		CategoryPanels[catName] = newPanel;
		CurrentContent = newPanel;
	}
}
