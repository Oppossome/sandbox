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
using assetmanager;

[UseTemplate]
public class SpawnMenu : Panel
{
	public Panel SpawnPanel { get; set; }
	public ToolsMenu ToolPanel { get; set; }

	public SpawnMenu()
	{
		this.BindClass( "hidden", () => !(Input.Down( InputButton.Menu ) || HasFocus) );
		CreateSpawnMenu();
	}

	private Func<Panel> Empty = () => null;

	private void CreateSpawnMenu()
	{
		var tabSplit = SpawnPanel.AddChild<TabSplit>();

		// Prop Tab Creation

		tabSplit.Register("Props").WithPanel( () =>
		{
			CategorySplit catSplit = new();

			catSplit.Register( "Sandbox" ).WithPanel( () => {
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
			} ).SetActive();

			foreach ( var kvp in Assets.RegisteredModels() ) 
			{
				catSplit.Register( kvp.Key ).WithPanel( () =>
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
		} ).SetActive();

		// Entity Tab Creation
		tabSplit.Register( "Entities" ).WithPanel( () =>
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
		canvas.Layout.ItemHeight = 150;
		canvas.Layout.ItemWidth = 150;
		canvas.AddClass( "canvas" );
		return canvas;
	}

	private string GetFileName(string name)
	{
		Match fileMatch = Regex.Match( name, @"(\w+)\.\w+$" );
		return fileMatch.Groups[1].Value;
	}
}
