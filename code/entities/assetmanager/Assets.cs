using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace assetmanager
{
	public class AssetProvider : LibraryMethod
	{
		public Type Type { get; set; }
		public bool Fallback { get; set; }
	}

	public static class Assets 
	{
		private static Dictionary<Type, Dictionary<string, Object>> Cache = new();

		[ClientVar( "asset_nocache" )]
		public static bool NoCache { get; set; } = false;

		public static T Get<T>(string filePath) where T : Resource
		{
			if ( Cache.TryGetValue( typeof( T ), out var resourceCache ) )
				if ( resourceCache.TryGetValue( filePath, out object result ) )
					return (T)result;

			var attr = Library.GetAttributes<AssetProvider>().Where( x => x.Type == typeof( T ) );

			foreach ( AssetProvider aP in attr.Where( x => !x.Fallback ) )
			{
				T result = (T)aP.InvokeStatic( filePath );
				if ( result == null ) continue;

				if ( !NoCache )
				{
					if ( !Cache.ContainsKey( typeof( T ) ) )
						Cache.Add( typeof( T ), new() );

					Cache[typeof( T )][filePath] = result;
				}

				return result;
			}

			AssetProvider fallbackProvider = attr.Where(x => x.Fallback ).FirstOrDefault();
			return (T)fallbackProvider.InvokeStatic( filePath );
		}

		[AssetProvider( Type = typeof( Model ) )]
		public static Model DefaultModel( string filePath ) 
			=> filePath.EndsWith( "vmdl" ) ? Model.Load( filePath ) : null;
		
		[AssetProvider( Type = typeof( Model ), Fallback = true )]
		public static Model FallbackModel( string filePath )
			=> Model.Load( "models/dev/error.vmdl" );

		[AssetProvider( Type = typeof( Material ) )]
		public static Material DefaultMaterial( string filePath ) 
			=> filePath.EndsWith( "vmat" ) ? Material.Load( filePath ) : null;

		[AssetProvider( Type = typeof( Material ), Fallback = true )]
		public static Material FallbackMaterial( string filePath ) 
			=> Material.Load( "materials/error.vmat" );

		[Event.Entity.PostSpawn]
		public static void DoLoad()
		{
			Assets.Get<Material>( "" );
			Assets.Get<Model>( "" );
		}
	}
}
