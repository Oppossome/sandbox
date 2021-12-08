using System;
using System.IO;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.Tools
{
	[Library( "tool_color", Title = "Color", Description = "Change render color and alpha of entities", Group = "construction" )]
	public partial class ColorTool : BaseTool
	{
		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				if ( !Input.Pressed( InputButton.Attack1 ) ) return;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
				   .Ignore( Owner )
				   .UseHitboxes()
				   .HitLayer( CollisionLayer.Debris )
				   .Run();

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				if ( tr.Entity is not ModelEntity modelEnt )
					return;

				modelEnt.RenderColor = Color.Random;

				CreateHitEffects( tr.EndPos );
			}
		}

		public override void ReadSettings( BinaryReader streamReader )
		{
			Log.Info( new Color().Read(streamReader) );

		}

		public override Panel MakeSettingsPanel()
		{

			return new();
		}
	}
}
