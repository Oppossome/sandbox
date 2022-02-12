using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace Sandbox.Tools
{
	[Library( "tool_camera", Title = "Camera" )]
	public class CameraTool : BaseTool
	{
		public override void Activate()
		{
			base.Activate();
			if ( Host.IsServer ) return;
			Notifications.Send( "UI hidden for screenshot purposes", 3 );
			Notifications.Send( "Menus and tool switching will still work", 3 );
		}

		public override void Simulate()
		{
			if ( Parent?.ViewModelEntity is ModelEntity ent )
				ent.EnableDrawing = false;

			base.Simulate();
		}

		public override void Deactivate()
		{
			if( Parent?.ViewModelEntity is ModelEntity ent )
				ent.EnableDrawing = true;

			base.Deactivate();
		}
	}
}
