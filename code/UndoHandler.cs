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
	public static Dictionary<int, List<UndoEntry>> Props = new();

	public static void DoUndo(int userId, int count = 1 )
	{
		var userProps = Props.GetValueOrDefault( userId );

		while(count > 0 && userProps.Count > 0 )
		{
			UndoEntry entry = userProps[userProps.Count - 1];
			userProps.Remove( entry );

			if ( AttemptUndo( entry ) )
				count--;
		}
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
		if ( !Props.ContainsKey( client.UserId ) )
			Props[client.UserId] = new();

		return Props[client.UserId];
	}

	public static void Register( Entity player, Entity prop )
	{
		GetPlayerEntry( player.Client ).Add( new()
		{
			Prop = prop
		} );
	}

	public static void Register( Entity player, Func<bool> func )
	{
		GetPlayerEntry( player.Client ).Add( new()
		{
			Undo = func
		} );
	}

	[ServerCmd("undo")]
	public static void UndoCMD(int amnt = 1)
	{
		if ( ConsoleSystem.Caller is not Client cl )
			return;

		DoUndo( cl.UserId, amnt );
	}

	[AdminCmd("undo_everyone")]
	public static void UndoEveryoneCMD()
	{
		foreach( int id in Props.Keys )
			DoUndo( id, int.MaxValue );
	}
}
