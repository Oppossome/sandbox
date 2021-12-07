using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using assetmanager;

[UseTemplate]
public class SpawnIcon : Panel
{
	public Panel InnerPanel { get; set; }
	public Panel IconPanel { get; set; }
	public string IconText { get; set; }
	ScenePanel scenePanel;
	
	public SpawnIcon( string iconName )
	{
		IconText = iconName;
	}

	private void SetIcon(Texture icon) =>
		IconPanel.Style.BackgroundImage = icon;

	public SpawnIcon WithIcon(string iconPath)
	{
		SetIcon( Texture.Load( iconPath ) );
		return this;
	}

	public SpawnIcon WithCallback( Action clickCallback )
	{
		InnerPanel.AddEventListener( "onclick", clickCallback );
		return this;
	}

	public SpawnIcon WithRenderedIcon( string modelPath )
	{
		Model renderModel = Assets.Get<Model>( $"models/{modelPath}" );

		Vector3 maxs = renderModel.RenderBounds.Maxs;
		float maxDist = Vector3.DistanceBetween( Vector3.Zero, maxs );

		Vector3 camNormal = new( .6f, .7f, .4f ); 
		Vector3 camPos = camNormal * maxDist + Vector3.Up * (maxs.z / 2);
		Rotation camRot = Rotation.LookAt( - camNormal );


		using (SceneWorld.SetCurrent( new SceneWorld() ) )
		{
			SceneObject.CreateModel( $"models/{modelPath}", Transform.Zero );
			scenePanel = IconPanel.Add.ScenePanel( SceneWorld.Current, camPos, camRot, 90, "renderedCam" );
			scenePanel.AmbientColor = Color.White * 1;
			scenePanel.RenderOnce = true;
		}

		IconPanel.AddClass( "noicon" );
		return this;
	}

	[ClientCmd]
	public static void ResetNormal()
	{
		Local.Pawn.EyeRot = Rotation.FromPitch(0);
	}

	[ClientCmd]
	public static void GetNormal()
	{
		Log.Info( Local.Pawn.EyeRot.Forward );

	}
}
