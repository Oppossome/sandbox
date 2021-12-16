using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

public partial class Notifications : Panel
{
	public static Notifications Instance;

	public Notifications()
	{
		StyleSheet.Load( "/ui/notifications/Notifications.scss" );
		Instance = this;
	}

	[ClientRpc]
	public static void Send( string text, float lifetime = 5f)
	{
		Notification newNotif = new( text, lifetime );
		Instance.AddChild( newNotif );
	}

	[ClientRpc]
	public static void SendUndo(int count)
	{
		Notification undoNotif = (Notification)Instance.Children.FirstOrDefault( x => ((Notification)x).Type == "undo" && !x.IsDeleting );

		if( undoNotif != null )
		{
			int undoCount = (int)undoNotif.Data + count;
			undoNotif.Data = undoCount;

			undoNotif.Text = $"{undoCount} props undone";
			undoNotif.Lifetime = -3f;
			undoNotif.Jiggle = -.1f;
			return;
		}

		string notif = $"{count} prop{(count > 1 ? "s" : "")} undone";
		Notification newNotif = new( notif, 3, "undo" );
		newNotif.Data = count;
		
		Instance.AddChild( newNotif );
	}
}
