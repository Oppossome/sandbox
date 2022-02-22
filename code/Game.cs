using System.Collections.Generic;
using System.Linq;
using Sandbox;

partial class SandboxGame : Game
{
	public static SandboxGame Instance => (SandboxGame)Current;
	
	[Net]
	public Dictionary<long, int> UndoCount { get; set; } = new();

	public SandboxGame()
	{
		if ( IsServer )
		{
			// Create the HUD
			_ = new SandboxHud();
		}
	}

	public override void ClientJoined( Client cl )
	{
		base.ClientJoined( cl );
		var player = new SandboxPlayer( cl );
		player.Respawn();

		cl.Pawn = player;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	[ServerCmd( "spawn" )]
	public static void Spawn( string modelname )
	{
		if ( ConsoleSystem.Caller == null )
			return;

		var owner = ConsoleSystem.Caller.Pawn;
		Tool ownerTool = (Tool)owner.Children.FirstOrDefault( x => x is Tool );

		var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 500 )
			.UseHitboxes()
			.Ignore( owner )
			.Run();

		var ent = new Prop();
		ent.Position = tr.EndPosition;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) ) * Rotation.FromAxis( Vector3.Up, 180 );
		ent.SetModel( modelname );
		ent.Position = tr.EndPosition - Vector3.Up * ent.CollisionBounds.Mins.z;

		if(modelname == "models/citizen/citizen.vmdl" && ownerTool.CurrentTool is Dresser dresser )
			dresser.ApplyClothing(ent);

		UndoHandler.Register( owner, ent );
	}

	[ServerCmd( "spawn_entity" )]
	public static void SpawnEntity( string entName )
	{
		var clPawn = ConsoleSystem.Caller.Pawn;
		if ( clPawn is not Player owner ) return;

		var attribute = Library.GetAttribute( entName );

		if ( attribute == null || !attribute.Spawnable )
			return;

		var tr = Trace.Ray( owner.EyePosition, owner.EyePosition + owner.EyeRotation.Forward * 200 )
			.UseHitboxes()
			.Ignore( owner )
			.Size( 2 )
			.Run();

		var ent = Library.Create<Entity>( entName );
		if ( ent is BaseCarriable && owner.Inventory != null )
		{
			if ( owner.Inventory.Add( ent, true ) )
				return;
		}

		ent.Position = tr.EndPosition;
		ent.Rotation = Rotation.From( new Angles( 0, owner.EyeRotation.Angles().yaw, 0 ) );
		UndoHandler.Register(owner, ent );

		//Log.Info( $"ent: {ent}" );
	}

	public override void DoPlayerNoclip( Client player )
	{
		if ( player.Pawn is Player basePlayer )
		{
			if ( basePlayer.DevController is NoclipController )
			{
				Log.Info( "Noclip Mode Off" );
				basePlayer.DevController = null;
			}
			else
			{
				Log.Info( "Noclip Mode On" );
				basePlayer.DevController = new NoclipController();
			}
		}
	}

	[ClientCmd( "debug_write" )]
	public static void Write()
	{
		ConsoleSystem.Run( "quit" );
	}
}
