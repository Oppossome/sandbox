using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class EntryCategory
{
	public static EntryCategory Host = new( "Host", 0 );
	public static EntryCategory Player = new( "Player", 1 );
	public static EntryCategory Offline = new( "Recently Left", 3 );
	public static EntryCategory Bot = new( "Bot", 2 );

	public string Name;
	public int Order;

	public EntryCategory( string name, int order = 0)
	{
		Name = name;
		Order = order;
	}
}

[UseTemplate]
public class PlayerEntry : Panel
{
	public string Entities { get; set; } = "0";
	public string Username { get; set; }
	public string Deaths { get; set; }
	public string Kills { get; set; }

	TimeSince OfflineTime;
	public Client Owner;
	long UserId;

	public PlayerEntry( Client cl )
	{
		UserId = cl.PlayerId;
		Owner = cl;
	}

	protected override void OnClick( MousePanelEvent e )
	{
		PopupMenu popup = new( this );

		if( Owner.IsValid() )
		{
			popup.AddButton( "Goto", () =>
				GotoPlayer( UserId.ToString() ) );
		}

		if( Local.Client.IsListenServerHost )
		{
			popup.AddButton( "Undo All", () =>
			   CleanupPlayer( UserId.ToString() ) );

			if( UserId != Local.Client.PlayerId && Owner.IsValid() )
			{
				popup.AddButton( "Kick", () =>
					 KickPlayer( UserId.ToString() ) );
			}
		}
	}

	protected override void OnRightClick( MousePanelEvent e )
	{
		OnClick( e );
	}

	public EntryCategory GetCategory()
	{
		if ( !Owner.IsValid() ) {
			if ( OfflineTime <= 60 || Entities != "0" ) return EntryCategory.Offline;
			return null;
		} else OfflineTime = 0;

		if ( Owner.IsListenServerHost ) return EntryCategory.Host;
		if ( Owner.IsBot ) return EntryCategory.Bot;
		return EntryCategory.Player;
	}

	RealTimeSince LastUpdate = 0;

	public override void Tick()
	{
		base.Tick();

		if ( !IsVisible || LastUpdate < 0.1f ) 
			return;

		LastUpdate = 0;

		if( UserId != 0 )
		{
			if( SandboxGame.Instance.UndoCount.TryGetValue( UserId, out var undoCount ) )
			{
				Entities = undoCount.ToString();
			}
			else
			{
				Entities = "0";
			}
		}

		if ( !Owner.IsValid() )
		{
			Deaths = "-";
			Kills = "-";
			return;
		}

		Username = Owner.Name;
		UserId = Owner.PlayerId;
		Deaths = Owner.GetInt( "deaths" ).ToString();
		Kills = Owner.GetInt( "kills" ).ToString();
	}

	[ServerCmd]
	public static void GotoPlayer(string userRaw)
	{
		if ( !long.TryParse( userRaw, out var userId ) )
			return;

		Client target = Client.All.FirstOrDefault( x => x.PlayerId == userId );

		if ( ConsoleSystem.Caller is not Client cl || target == null )
			return;

		Notifications.Send( To.Single( target ), $"{cl.Name} teleported to you" );
		cl.Pawn.Position = target.Pawn.Position;
	}

	[ServerCmd]
	public static void CleanupPlayer( string userRaw )
	{
		if ( ConsoleSystem.Caller is not Client cl || !cl.IsListenServerHost )
			return;

		if ( !long.TryParse( userRaw, out var userId ) )
			return;

		UndoHandler.DoUndo( userId, -1 );
	}

	[ServerCmd]
	public static void KickPlayer( string userRaw )
	{
		if ( ConsoleSystem.Caller is not Client cl || !cl.IsListenServerHost )
			return;

		if ( !long.TryParse( userRaw, out var userId ) )
			return;

		Client target = Client.All.FirstOrDefault( x => x.PlayerId == userId );
		if ( target != null ) target.Kick();
	}
}
