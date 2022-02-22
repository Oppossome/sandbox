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
	
	public SpawnIcon( string iconName )
	{
		IconText = iconName;
	}

	public SpawnIcon WithIcon( Texture icon )
	{
		IconPanel.Style.BackgroundImage = icon;
		return this;
	}

	public SpawnIcon WithIcon( string iconPath )
	{
		WithIcon( Texture.Load( FileSystem.Mounted, iconPath ) );
		return this;
	}

	public SpawnIcon WithCallback( Action<bool> clickCallback )
	{
		InnerPanel.AddEventListener( "onrightclick", () => clickCallback.Invoke( false ) );
		InnerPanel.AddEventListener( "onclick", () => clickCallback.Invoke( true ));
		return this;
	}

	public SpawnIcon WithRenderedIcon( string modelPath )
	{
		Model renderModel = Assets.Get<Model>( modelPath );

		Vector3 maxs = renderModel.RenderBounds.Maxs;
		float maxDist = Vector3.DistanceBetween( Vector3.Zero, maxs );

		Vector3 camNormal = new( .6f, .7f, .4f ); 
		Vector3 camPos = camNormal * maxDist + Vector3.Up * (maxs.z / 2);
		Rotation camRot = Rotation.LookAt( - camNormal );


		SceneWorld scene = new SceneWorld();
		new SceneModel( scene, modelPath, Transform.Zero );

		ScenePanel scenePanel = IconPanel.Add.ScenePanel( scene, camPos, camRot, 90, "renderedCam" );
		scenePanel.AmbientColor = Color.White * 1;
		scenePanel.RenderOnce = true;


		IconPanel.AddClass( "noicon" );
		return this;
	}
}
