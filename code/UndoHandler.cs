using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public struct UndoEntry
{
	public Func<bool> Undo;
	public Entity Prop;
}

public static class UndoHandler
{
	public static Dictionary<long, List<UndoEntry>> Props = new();

	public static int DoUndo(long userId, int count = 1 )
	{
		var userProps = Props.GetValueOrDefault( userId );
		if ( userProps == null ) return 0;
		int total = 0;

		if ( count == -1 )
			count = int.MaxValue;

		while(total < count && userProps.Count > 0 )
		{
			UndoEntry entry = userProps[userProps.Count - 1];
			userProps.Remove( entry );

			if ( AttemptUndo( entry ) )
				total++;
		}

		SandboxGame.Instance.UndoCount[userId] = userProps.Count;
		return total;
	}

	private static bool AttemptUndo(UndoEntry entry)
	{
		if( entry.Prop is not null )
		{
			if( entry.Prop.IsValid )
			{
				entry.Prop.Delete();
				return true;
			}

			return false;
		}

		if(entry.Undo != null)
			return entry.Undo();

		return false;
	}

	private static List<UndoEntry> GetPlayerEntry( Client client )
	{
		if ( !Props.ContainsKey( client.PlayerId ) )
		{
			Notifications.Send( To.Single( client ), "Want to undo? bind z undo", 5 );
			Props[client.PlayerId] = new();
		}

		return Props[client.PlayerId];
	}

	public static void Register( Entity player, Entity prop )
	{
		var propList = GetPlayerEntry( player.Client );
		propList.Add( new() { Prop = prop } );

		SandboxGame.Instance.UndoCount[player.Client.PlayerId] = propList.Count;
	}

	public static void Register( Entity player, Func<bool> func )
	{
		var propList = GetPlayerEntry( player.Client );
		propList.Add( new() { Undo = func } );

		SandboxGame.Instance.UndoCount[player.Client.PlayerId] = propList.Count;
	}

	[ServerCmd("undo")]
	public static void UndoCMD(int amnt = 1)
	{
		if ( ConsoleSystem.Caller is not Client cl )
			return;

		int undone = DoUndo( cl.PlayerId, amnt );

		if ( undone == 0 )
			return;

		Notifications.SendUndo( To.Single( cl ), undone );
	}

	[ServerCmd("undo_everyone")]
	public static void UndoEveryoneCMD()
	{
		if ( ConsoleSystem.Caller is not Client cl || !cl.IsListenServerHost )
			return;

		Notifications.Send( To.Single( cl ), $"Everyone's entites undone", 2 );

		foreach ( long id in Props.Keys )
			DoUndo( id, -1 );
	}
}
