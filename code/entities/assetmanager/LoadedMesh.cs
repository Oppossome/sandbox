using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace assetmanager
{
	public partial class LoadedMesh : ModelEntity
	{
		[Net]
		public string ModelPath { get; set; }

		private string LastModel { get; set; }

		[Event.Tick]
		public void Tick()
		{
			if ( LastModel == ModelPath || !IsValid )
				return;

			LastModel = ModelPath;
			Model = Assets.Get<Model>( ModelPath );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		}

		[ServerCmd]
		public static void SpawnModel(string model)
		{
			if ( ConsoleSystem.Caller.Pawn is not Player ply )
				return;

			LoadedMesh mesh = new();
			mesh.ModelPath = model;

			UndoHandler.Register( ply, mesh );
		}
	}
}
