using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.UI.Tests;
using Sandbox.Tools;
using assetmanager;

[UseTemplate]
public class SpawnMenu : Panel
{
	public Panel SpawnPanel { get; set; }
	public Panel ToolPanel { get; set; }

	public SpawnMenu()
	{
		this.BindClass( "hidden", () => !(Input.Down( InputButton.Menu ) || HasFocus) );
		CreateSpawnMenu();
		CreateToolMenu();
	}

	private Func<Panel> Empty = () => null;

	private void CreateToolMenu()
	{
		var tabSplit = ToolPanel.AddChild<TabSplit>();

		tabSplit.Register( "Tools", () =>
		 {
			 CategorySplit catSplit = new();
			 ToolPanel.BindClass( "small", () => catSplit.CurrentContent is null ); // Shrink if no tool menu

			 foreach ( var entry in Library.GetAllAttributes<BaseTool>() )
			 {
				 if ( entry.Title == "BaseTool" ) continue;

				 Button toolButton = catSplit.Register( entry.Title, Empty, () => //TODO: Display tool specific menu
				  {
					  ConsoleSystem.Run( "tool_current", entry.Name );
					  ConsoleSystem.Run( "inventory_current", "weapon_tool" );
				  } );

				 if ( entry.Name == ConsoleSystem.GetValue( "tool_current" ) )
					 toolButton.Click();
			 }

			 return catSplit;
		 } );

		tabSplit.Register( "Utils", () =>
		{
			CategorySplit catSplit = new();

			catSplit.RegisterButton( "Undo", () =>
				ConsoleSystem.Run( "undo" ) );

			catSplit.RegisterButton( "Undo All", () =>
				ConsoleSystem.Run( "undo", -1 ) );

			return catSplit;
		} );
	}

	private void CreateSpawnMenu()
	{
		var tabSplit = SpawnPanel.AddChild<TabSplit>();

		tabSplit.Register( "Props", () =>
		{
			CategorySplit catSplit = new();

			catSplit.Register( "Sandbox", () => {
				var scrollPanel = MakeScrollPanel();

				scrollPanel.OnCreateCell = ( cell, data ) =>
				{
					string modelName = (string)data;
					var spIcon = new SpawnIcon( GetFileName( modelName ) )
						.WithRenderedIcon( modelName );

					spIcon.WithCallback( () =>
						ConsoleSystem.Run( "spawn", $"models/{modelName}" ) );

					cell.AddChild(spIcon);
				};


				int c = 0;
				foreach ( var file in FileSystem.Mounted.FindFile( "models", "*.vmdl_c", true ) )
				{
					if ( string.IsNullOrWhiteSpace( file ) ) continue;
					if ( file.Contains( "clothes" ) ) continue;
					if ( file.Contains( "_lod0" ) ) continue;

					scrollPanel.AddItem( file.Remove( file.Length - 2 ) );
				}

				return scrollPanel;
			} );

			foreach ( var kvp in Assets.RegisteredModels() ) 
			{
				catSplit.Register( kvp.Key, () =>
				{
					var scrollPanel = MakeScrollPanel();

					scrollPanel.OnCreateCell = ( cell, data ) =>
					{
						string modelName = (string)data;
						var spIcon = new SpawnIcon( GetFileName( modelName ) );

						spIcon.WithCallback( () =>
							ConsoleSystem.Run( "SpawnModel", modelName) );

						cell.AddChild( spIcon );
					};

					foreach ( string path in kvp.Value )
						scrollPanel.AddItem( path );

					return scrollPanel;
				} );
			}


			return catSplit;
		} );

		tabSplit.Register( "Entities", () =>
		{
			var scrollPanel = MakeScrollPanel();

			scrollPanel.OnCreateCell = ( cell, data ) =>
			{
				var entry = (LibraryAttribute)data;
				var spIcon = new SpawnIcon( entry.Title )
					.WithIcon( $"/entity/{entry.Name}.png" );

				spIcon.WithCallback( () =>
					ConsoleSystem.Run( "spawn_entity", entry.Name ) );

				cell.AddChild( spIcon );
			};

			var ents = Library.GetAllAttributes<Entity>().Where( x => x.Spawnable ).OrderBy( x => x.Title ).ToArray();
			foreach ( var entry in ents ) scrollPanel.AddItem( entry );
			return scrollPanel;
		} );

	}

	private VirtualScrollPanel MakeScrollPanel()
	{
		VirtualScrollPanel canvas = new();
		canvas.Layout.AutoColumns = true;
		canvas.Layout.ItemHeight = 160;
		canvas.Layout.ItemWidth = 160;
		canvas.AddClass( "canvas" );
		return canvas;
	}

	private string GetFileName(string name)
	{
		Match fileMatch = Regex.Match( name, @"(\w+)\.\w+$" );
		return fileMatch.Groups[1].Value;
	}
}
