namespace Sandbox.Tools
{
	[Library( "tool_boxgun", Title = "Box Shooter", Description = "Shoot boxes", Group = "fun" )]
	public class BoxShooter : BaseTool
	{
		[ConVar.ClientData( "boxshooter_prop" )]
		public static string ShooterProp { get; set; } = "models/citizen_props/crate01.vmdl";

		TimeSince timeSinceShoot;

		public override void Simulate()
		{
			if ( Host.IsServer )
			{
				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					ShootBox();
				}

				if ( Input.Down( InputButton.Attack2 ) && timeSinceShoot > 0.05f )
				{
					timeSinceShoot = 0;
					ShootBox();
				}
			} else
			{
				if ( Input.Pressed( InputButton.Reload ) )
				{
					TraceResult tr = Trace.Ray( Owner.EyePosition, Owner.EyePosition + Owner.EyeRotation.Forward * 20000 )
						.Ignore( Owner )
						.Run();

					if ( tr.Entity is ModelEntity ent && !string.IsNullOrEmpty(ent.GetModelName() ) )
						ShooterProp = ent.GetModelName();
				}
			}
		}

		void ShootBox()
		{
			var ent = new Prop
			{
				Position = Owner.EyePosition + Owner.EyeRotation.Forward * 50,
				Rotation = Owner.EyeRotation
			};

			string shooterModel = Owner.Client.GetClientData<string>( "boxshooter_prop", "models/citizen_props/crate01.vmdl" );
			ent.SetModel( shooterModel )
				;
			ent.Velocity = Owner.EyeRotation.Forward * 1000;
			UndoHandler.Register( Owner, ent );
		}
	}
}
