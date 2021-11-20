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
		public string Model { get; set; }

		private string LastModel { get; set; }

		[Event.Tick]
		public void Tick()
		{
			if ( LastModel == Model || !IsValid )
				return;

			LastModel = Model;
			SetModel( Assets.Get<Model>( Model ) );
			SetupPhysicsFromModel( PhysicsMotionType.Keyframed );
		}

		[ServerCmd]
		public static void SpawnModel(string model)
		{
			if ( ConsoleSystem.Caller.Pawn is not Player ply )
				return;

			LoadedMesh mesh = new();
			mesh.Model = model;

			UndoHandler.Register( ply, mesh );
		}
	}
}
