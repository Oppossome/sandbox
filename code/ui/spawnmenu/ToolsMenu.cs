using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.Tools;

public class ToolsMenu : TabSplit
{
	Dictionary<string, SplitButton> ToolButtons = new();

	public ToolsMenu()
	{
		StyleSheet.Load("/ui/spawnmenu/ToolsMenu.scss");
		ToolList();
	}

	private Func<Panel> Empty = () => null;

	private void ToolList()
	{
		var tabSplit = AddChild<TabSplit>();

		tabSplit.Register( "Tools" ).WithPanel( () =>
		{
			CategorySplit catSplit = new();

			BindClass( "small", () => catSplit.CurrentButton is null || catSplit.CurrentButton.GetPanel() is null );

			foreach ( var entry in Library.GetAllAttributes<BaseTool>() )
			{
				if ( entry.Title == "BaseTool" ) continue;

				SplitButton toolButton = catSplit.Register( entry.Title ).WithCallback( () =>
				{
					ConsoleSystem.Run( "tool_current", entry.Name );
					ConsoleSystem.Run( "inventory_current", "weapon_tool" );
				} );

				ToolButtons[entry.Name] = toolButton;
				if ( entry.Name == ConsoleSystem.GetValue( "tool_current" ) )
					DisplayCurrent( toolButton );

			}

			return catSplit;
		} ).SetActive();

		tabSplit.Register( "Utils" ).WithPanel( () =>
		{
			CategorySplit catSplit = new();

			catSplit.Register( "Undo" ).WithCallback( () =>
				ConsoleSystem.Run( "undo" ), true );

			catSplit.Register( "Undo All" ).WithCallback( () =>
				ConsoleSystem.Run( "undo", -1 ), true );

			if( Local.Client.IsListenServerHost )
				catSplit.Register( "Undo Everyone" ).WithCallback( () =>
					ConsoleSystem.Run( "undo_everyone" ), true );

			return catSplit;
		} );
	}

	private void DisplayCurrent( SplitButton tBtn )
	{
		if ( Local.Pawn is null ) return;

		Tool userTool = (Tool)Local.Pawn.Children.FirstOrDefault( x => x is Tool );

		if ( userTool != null && userTool.CurrentTool != null )
		{
			Panel sPanel = userTool.CurrentTool.MakeSettingsPanel();
			tBtn.SetPanel( sPanel ).SetActive();
		}
	}


	BaseTool LastTool = null;
	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is null )
			return;

		Tool userTool = (Tool)Local.Pawn.Children.FirstOrDefault( x => x is Tool );
		string lastToolName = LastTool is BaseTool tool ? tool.ClassInfo.Name : "";

		if ( userTool.CurrentTool is not BaseTool currentTool || currentTool.ClassInfo.Name == lastToolName )
			return;

		var entry = Library.GetAttribute( currentTool.GetType() );
		LastTool = currentTool;

		if ( ToolButtons.TryGetValue( entry.Name, out SplitButton tBtn ) )
		{
			Panel sPanel = currentTool.MakeSettingsPanel();
			tBtn.SetPanel( sPanel ).SetActive();
		}
	}
}

