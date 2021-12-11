using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.Tools;

[UseTemplate]
public class DresserMenu : SettingsPanel
{
	ModelSelector ClothingSelection { get; set; }
	SliderLabeled SkinSlider { get; set; }
	Panel RenderContainer { get; set; }
	SceneWorld World = new();
	
	List<AnimSceneObject> Models = new();
	AnimSceneObject Preview;

	public DresserMenu()
	{
		MakePreview();

		foreach ( var file in FileSystem.Mounted.FindFile( "models", "*.vmdl_c", true ) )
		{
			if ( string.IsNullOrWhiteSpace( file ) ) continue;
			if ( !file.Contains( "clothes" ) ) continue;
			if ( file.Contains( "_lod0" ) ) continue;

			ClothingSelection.AddEntry( $"models/{file.Remove(file.Length - 2)}", () => { } );
		}
	}

	private void MakePreview()
	{
		ScenePanel scenePanel;

		using ( SceneWorld.SetCurrent( World ) )
		{
			Preview = new AnimSceneObject( Model.Load("models/citizen/citizen.vmdl"), Transform.Zero );
			Preview.SetAnimInt( "idle_states", 0 );

			scenePanel = RenderContainer.Add.ScenePanel( SceneWorld.Current, new Vector3( 125, 0, 34 ), Rotation.FromYaw( 180 ), 40, "renderview" );
			scenePanel.AmbientColor = Color.White * 1;
		}

		bool isHolding = false;
		scenePanel.AddEventListener( "onmouseup", () =>
		{
			isHolding = false;

		} );

		scenePanel.AddEventListener( "onmousedown", () =>
		{
			isHolding = true;

		} );

		float rot = 180;
		float yAdjust = 0;
		scenePanel.AddEventListener( "onmousemove", () =>
		{
			Rotation cRot = Rotation.FromYaw( rot );

			scenePanel.CameraPosition = cRot.Forward * -125 + Vector3.Up * (32 + yAdjust);
			scenePanel.CameraRotation = cRot;
			
			if ( !isHolding ) return;

			yAdjust = Math.Clamp( yAdjust + Mouse.Delta.y * .5f, -32, 32 );
			rot = (rot - Mouse.Delta.x * 2f) % 360f;

		} );


	}

	public override void Tick()
	{
		base.Tick();

		var toDelete = Models.Where( x => !ClothingSelection.Models.Contains( x.Model.Name ) );
		Preview.SetMaterialGroup( $"Skin0{SkinSlider.Value}" );

		foreach ( var model in toDelete.ToList() )
		{
			Models.Remove( model );
			model.Delete();
		}

		var currentClothes = Models.Select( x => x.Model.Name );
		var toMake = ClothingSelection.Models.Except( currentClothes );

		using( SceneWorld.SetCurrent( World ) )
		{
			foreach( var model in toMake)
			{
				var newObj = new AnimSceneObject( Model.Load( model ), Preview.Transform );
				Preview.AddChild( "clothing", newObj );
				Models.Add( newObj );
			}
		}

		Preview.Update( Time.Delta );
		foreach ( AnimSceneObject mdl in Models )
			mdl.Update( Time.Delta );
	}

	public void UpdateSettings()
	{
		var currentClothes = Models.Select( x => x.Model.Name ).ToList();

		using (SettingsWriter writer = new() )
		{
			SkinSlider.Value.Write( writer );
			currentClothes.Count.Write( writer );
			foreach(var clothing in currentClothes) clothing.Write(writer);
		}
	}

}
