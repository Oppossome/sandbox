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
}
