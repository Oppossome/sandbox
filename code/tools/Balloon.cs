using System;
using System.IO;
using System.Threading.Tasks;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.Tools
{
	[Library( "tool_balloon", Title = "Balloons", Description = "Create Balloons!", Group = "construction" )]
	public partial class BalloonTool : BaseTool
	{
		public float BalloonForce = 10;
		public float RopeLength = 100;
		public Color Tint = Color.Red;

		PreviewEntity previewModel;

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is BalloonEntity )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, "models/citizen_props/balloonregular01.vmdl" ) )
			{
				previewModel.RelativeToNormal = false;
			}
		}

		public override void Simulate()
		{
			if ( previewModel.IsValid() )
			{
				previewModel.RenderColor = Tint;
			}

			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				bool useRope = Input.Pressed( InputButton.Attack1 );
				if ( !useRope && !Input.Pressed( InputButton.Attack2 ) )
					return;

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit )
					return;

				if ( !tr.Entity.IsValid() )
					return;

				CreateHitEffects( tr.EndPos );

				if ( tr.Entity is BalloonEntity )
					return;

				var ent = new BalloonEntity
				{
					GravityScale = -BalloonForce / 50,
					Position = tr.EndPos,
				};

				ent.SetModel( "models/citizen_props/balloonregular01.vmdl" );
				ent.RenderColor = Tint;

				UndoHandler.Register( Owner, ent );

				if ( !useRope )
					return;

				var rope = Particles.Create( "particles/rope.vpcf" );
				rope.SetEntity( 0, ent );

				var attachEnt = tr.Body.IsValid() ? tr.Body.Entity : tr.Entity;
				var attachLocalPos = tr.Body.Transform.PointToLocal( tr.EndPos ) * (1.0f / tr.Entity.Scale);

				if ( attachEnt.IsWorld )
				{
					rope.SetPosition( 1, attachLocalPos );
				}
				else
				{
					rope.SetEntityBone( 1, attachEnt, tr.Bone, new Transform( attachLocalPos ) );
				}

				var spring = PhysicsJoint.Spring
					.From( ent.PhysicsBody )
					.To( tr.Body, tr.Body.Transform.PointToLocal( tr.EndPos ) )
					.WithFrequency( 5.0f )
					.WithDampingRatio( 0.7f )
					.WithReferenceMass( ent.PhysicsBody.Mass )
					.WithMinRestLength( 0 )
					.WithMaxRestLength( RopeLength )
					.WithCollisionsEnabled()
					.Create();

				spring.EnableAngularConstraint = false;
				spring.OnBreak( () =>
				{
					rope?.Destroy( true );
					spring.Remove();
				} );
			}
		}

		public override void ReadSettings( BinaryReader streamReader )
		{
			Tint = Color.Read( streamReader );
			RopeLength = streamReader.ReadSingle();
			BalloonForce = streamReader.ReadSingle();
		}


		private void UpdateSettings()
		{
			using ( SettingsWriter writer = new() )
			{
				Tint.Write( writer );
				RopeLength.Write( writer );
				BalloonForce.Write( writer );
			}
		}

		public override Panel MakeSettingsPanel()
		{
			SettingsPanel sPanel = new();
			sPanel.AddChild( new Title( "Color" ) );

			ColorPicker cPicker = sPanel.Add.ColorPicker( ( Color clr ) => {
				Tint = clr;
				UpdateSettings();
			} );

			cPicker.ColorHSV = Tint;

			SliderLabeled lengthSlider = sPanel.Add.SliderLabeled( "Rope Length", 0, 256, 1 );
			lengthSlider.Value = RopeLength;

			lengthSlider.OnFinalValue = ( float lengthValue ) =>
			{
				RopeLength = lengthValue;
				UpdateSettings();
			};

			SliderLabeled forceSlider = sPanel.Add.SliderLabeled( "Balloon Force", 0, 100 );
			forceSlider.Value = BalloonForce;

			forceSlider.OnFinalValue = ( float forceValue ) => {
				BalloonForce = forceValue;
				UpdateSettings();
			};


			return sPanel;
		}
	}
}
