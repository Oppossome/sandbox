using System.IO;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.Tools
{
	[Library( "tool_lamp", Title = "Lamps", Description = "Directional light source that casts shadows", Group = "construction" )]
	public partial class LampTool : BaseTool
	{
		PreviewEntity previewModel;

		public Color Color = Color.White;

		private string Model => "models/torch/torch.vmdl";

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is LampEntity )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Model ) )
			{
				previewModel.RelativeToNormal = false;
				previewModel.OffsetBounds = true;
				previewModel.PositionOffset = -previewModel.CollisionBounds.Center;
			}
		}

		public override void Simulate()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( !Input.Pressed( InputButton.Attack1 ) )
					return;

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				CreateHitEffects( tr.EndPos );

				if ( tr.Entity is LampEntity lamp )
				{
					// TODO: Set properties

					lamp.Flicker = !lamp.Flicker;

					return;
				}

				lamp = new LampEntity
				{
					Enabled = true,
					DynamicShadows = true,
					Range = 512,
					Falloff = 1.0f,
					LinearAttenuation = 0.0f,
					QuadraticAttenuation = 1.0f,
					InnerConeAngle = 25,
					OuterConeAngle = 45,
					Brightness = 10,
					Color = Color,
					Rotation = Rotation.Identity,
					LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
				};

				lamp.SetModel( Model );
				lamp.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
				lamp.Position = tr.EndPos + -lamp.CollisionBounds.Center + tr.Normal * lamp.CollisionBounds.Size * 0.5f;
				UndoHandler.Register( Owner, lamp );
			}
		}

		public override void ReadSettings( BinaryReader streamReader )
		{
			Color = Color.Read( streamReader );
		}

		private void UpdateSettings()
		{
			using ( SettingsWriter writer = new() )
			{
				Color.Write( writer );
			}
		}

		public override Panel MakeSettingsPanel()
		{
			SettingsPanel sPanel = new();
			sPanel.AddChild( new Title( "Lamp Color" ) );

			ColorPicker cPicker = sPanel.Add.ColorPicker( ( Color clr ) => {
				Color = clr;

				UpdateSettings();
			} );

			return sPanel;
		}
	}
}
