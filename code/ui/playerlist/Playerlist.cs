using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
public class Playerlist : Panel
{
	Dictionary<EntryCategory, Panel> CatePanels = new();
	List<PlayerEntry> Entries = new();
	public Panel Body { get; set; }

	private Panel GetCategory( EntryCategory category )
	{
		if ( CatePanels.ContainsKey( category ) ) return CatePanels[category];
		Panel catePanel = Body.Add.Panel( "category" );
		Panel header = catePanel.Add.Panel( "catheader" );
		header.Add.Label( category.Name, "cattitle" );
		CatePanels[category] = catePanel;

		Body.SortChildren( ( Panel pnl ) =>
		{
			if ( !CatePanels.ContainsValue( pnl ) ) return -1;
			return CatePanels.FirstOrDefault( x => x.Value == pnl ).Key.Order;
		} );

		return catePanel;
	}


	public override void Tick()
	{
		base.Tick();

		SetClass( "open", Input.Down( InputButton.Score ) );
		if ( !IsVisible ) return;

		var players = Entries.Select( x => x.Owner );
		foreach( Client cl in Client.All.Except( players ) )
			Entries.Add( new PlayerEntry( cl ) );

		foreach( PlayerEntry entry in Entries.ToList() )
		{
			var category = entry.GetCategory();

			if( category == null)
			{
				Entries.Remove( entry );
				entry.Delete();
				continue;
			}

			if ( entry.Parent != GetCategory( category ) )
				entry.Parent = GetCategory( category );
		}

		foreach(var kvp in CatePanels.ToList() )
		{
			if ( kvp.Value.ChildrenCount > 1 ) continue;
			CatePanels.Remove( kvp.Key );
			kvp.Value.Delete();
		}
	}
}
