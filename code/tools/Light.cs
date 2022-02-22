using System.IO;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Sandbox.Tools
{
	[Library( "tool_light", Title = "Lights", Description = "A dynamic point light", Group = "construction" )]
	public partial class LightTool : BaseTool
	{
		PreviewEntity previewModel;

		public Color Color = Color.White;
		public float RopeLength = 100;
		public float Range = 128;


		private string Model => "models/light/light_tubular.vmdl";


		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is LightEntity )
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
				bool useRope = Input.Pressed( InputButton.Attack1 );
				if ( !useRope && !Input.Pressed( InputButton.Attack2 ) )
					return;

				var startPos = Owner.EyePosition;
				var dir = Owner.EyeRotation.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				CreateHitEffects( tr.EndPosition );

				if ( tr.Entity is LightEntity )
				{
					// TODO: Set properties

					return;
				}

				var light = new LightEntity
				{
					Enabled = true,
					DynamicShadows = false,
					Range = Range,
					Falloff = 1.0f,
					LinearAttenuation = 0.0f,
					QuadraticAttenuation = 1.0f,
					Brightness = 1,
					Color = Color,
					//LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
				};

				light.UseFogNoShadows();
				light.SetModel( Model );
				light.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
				light.Position = tr.EndPosition + -light.CollisionBounds.Center + tr.Normal * light.CollisionBounds.Size * 0.5f;
				UndoHandler.Register( Owner, light );

				if ( !useRope )
					return;

				var rope = Particles.Create( "particles/rope.vpcf" );
				rope.SetEntity( 0, light, Vector3.Down * 6.5f ); // Should be an attachment point

				var attachEnt = tr.Body.IsValid() ? tr.Body.GetEntity() : tr.Entity;
				var attachLocalPos = tr.Body.Transform.PointToLocal( tr.EndPosition ) * (1.0f / tr.Entity.Scale);

				if ( attachEnt.IsWorld )
				{
					rope.SetPosition( 1, attachLocalPos );
				}
				else
				{
					rope.SetEntityBone( 1, attachEnt, tr.Bone, new Transform( attachLocalPos ) );
				}

				var spring = PhysicsJoint.CreateLength( light.PhysicsBody.LocalPoint( Vector3.Down * 6.5f), tr.Body.WorldPoint( tr.EndPosition ), RopeLength );
				spring.SpringLinear = new PhysicsSpring( 5.0f, 0.7f );
				spring.EnableAngularConstraint = false;
				spring.Collisions = true;

				spring.OnBreak += () => {
					rope?.Destroy( true );
					spring.Remove();
				};
			}
		}

		public override void ReadSettings( BinaryReader streamReader )
		{
			Color = Color.Read( streamReader );
			Range = streamReader.ReadSingle();
			RopeLength = streamReader.ReadSingle();
		}

		private void UpdateSettings()
		{
			using ( SettingsWriter writer = new() )
			{
				Color.Write( writer );
				Range.Write( writer );
				RopeLength.Write( writer );
			}
		}

		public override Panel MakeSettingsPanel()
		{
			SettingsPanel sPanel = new();
			sPanel.AddChild( new Title( "Light Color" ) );

			ColorPicker cPicker = sPanel.Add.ColorPicker( ( Color clr ) => {
				Color = clr;

				UpdateSettings();
			} );

			SliderLabeled rangeSlider = sPanel.Add.SliderLabeled( "Range", 32, 1024, 1 );
			rangeSlider.Value = Range;

			rangeSlider.OnFinalValue = ( float range ) =>
			{
				Range = range;
				UpdateSettings();
			};

			SliderLabeled ropeSlider = sPanel.Add.SliderLabeled( "Rope Length", 0, 256, 1 );
			ropeSlider.Value = RopeLength;

			ropeSlider.OnFinalValue = ( float ropeSlider ) =>
			{
				RopeLength = ropeSlider;
				UpdateSettings();
			};


			return sPanel;
		}
	}
}
