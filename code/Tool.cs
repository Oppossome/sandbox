using Sandbox;
using Sandbox.UI;
using Sandbox.Tools;
using System.IO;
using System.Linq;
using System;

[Library( "weapon_tool", Title = "Toolgun" )]
partial class Tool : Carriable
{
	[ConVar.ClientData( "tool_current" )]
	public static string UserToolCurrent { get; set; } = "tool_boxgun";

	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	[Net, Predicted]
	public BaseTool CurrentTool { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override void Simulate( Client owner )
	{
		UpdateCurrentTool( owner );

		CurrentTool?.Simulate();
	}

	private void UpdateCurrentTool( Client owner )
	{
		var toolName = owner.GetClientData<string>( "tool_current", "tool_boxgun" );
		if ( toolName == null )
			return;

		// Already the right tool
		if ( CurrentTool != null && Library.GetAttribute( CurrentTool.GetType() ).Name == toolName )
			return;

		if ( CurrentTool != null )
		{
			CurrentTool?.Deactivate();
			CurrentTool = null;
		}

		CurrentTool = Library.Create<BaseTool>( toolName, false );

		if ( CurrentTool != null )
		{
			CurrentTool.Owner = owner.Pawn as Player;
			CurrentTool.Parent = this;
			CurrentTool.Activate();
		}
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		if( CurrentTool is not null )
		{
			CurrentTool.Owner = Owner as Player;
			CurrentTool.Parent = this;
			CurrentTool.Activate();
		}

	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		CurrentTool?.Deactivate();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		CurrentTool?.Deactivate();
		CurrentTool = null;
	}

	public override void OnCarryDrop( Entity dropper )
	{
	}

	[Event.Frame]
	public void OnFrame()
	{
		if ( Owner is Player player && player.ActiveChild != this ) 
			return;

		CurrentTool?.OnFrame();
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 1 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
		anim.SetAnimParameter( "holdtype_handedness", 1 );
	}
}

namespace Sandbox.Tools
{
	public class SettingsWriter : BinaryWriter
	{		
		public SettingsWriter() : base( new MemoryStream() )
		{

		}

		protected override void Dispose( bool disposing )
		{
			MemoryStream streamData = (MemoryStream)BaseStream;
			string streamString = Convert.ToBase64String( streamData.ToArray() );
			BaseTool.ReplicateSettings( streamString );
			base.Dispose( disposing );
		}
	}

	public partial class BaseTool : BaseNetworkable
	{
		public Tool Parent { get; set; }
		public Player Owner { get; set; }

		protected virtual float MaxTraceDistance => 10000.0f;

		public virtual Panel MakeSettingsPanel()
		{
			return null;
		}

		public virtual void ReadSettings( BinaryReader streamReader )
		{

		}

		public virtual void Activate()
		{
			CreatePreviews();
		}

		public virtual void Deactivate()
		{
			DeletePreviews();
		}

		public virtual void Simulate()
		{

		}

		public virtual void OnFrame()
		{
			UpdatePreviews();
		}

		public virtual void CreateHitEffects( Vector3 pos )
		{
			Parent?.CreateHitEffects( pos );
		}

		[ServerCmd]
		public static void ReplicateSettings( string rawStream )
		{
			Tool callerTool = (Tool)ConsoleSystem.Caller.Pawn.Children.FirstOrDefault( x => x is Tool );
			if( callerTool == null) return;

			byte[] streamBytes = Convert.FromBase64String( rawStream );
			BinaryReader bReader = new( new MemoryStream( streamBytes ) );

			callerTool.CurrentTool.ReadSettings( bReader );
			bReader.Close();
		}
	}
}
