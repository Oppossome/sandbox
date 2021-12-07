using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace assetmanager.quakebsp
{
	public class PAKParser
	{
		private static Dictionary<string, byte[]> Files = new();
		private static bool HasParsed = false;

		private static void Parse(byte[] fileBytes)
		{
			if ( Encoding.ASCII.GetString( fileBytes, 0, 4 ) != "PACK" )
				return;

			int tblOffset = BitConverter.ToInt32( fileBytes, 4 );
			int tblLength = BitConverter.ToInt32( fileBytes, 8 );

			for ( int start = tblOffset; start < tblOffset + tblLength; start += 64 )
			{
				string name = Encoding.ASCII.GetString( fileBytes, start, 56 ).Trim( '\0' );
				if ( Files.ContainsKey( name ) ) continue;

				int fileOffset = BitConverter.ToInt32( fileBytes, start + 56 );
				int fileLength = BitConverter.ToInt32( fileBytes, start + 60 );
				Files.Add( name, fileBytes.Skip( fileOffset ).Take( fileLength ).ToArray() );
			}
		}

		public static byte[] GetFile(string filePath)
		{
			ParseFiles();
			return Files.GetValueOrDefault(filePath, null);
		} 

		private static void ParseFiles()
		{
			if ( HasParsed ) return;
			HasParsed = true;

			FileSystem.Data.CreateDirectory( "quake2" );
			foreach (string fileName in FileSystem.Data.FindFile( "quake2", "*.pak", true )) {
				byte[] fileBytes = FileSystem.Data.ReadAllBytes( "quake2/" + fileName ).ToArray();
				Parse( fileBytes );
			}

			RegisterFiletype( "bsp" );
			RegisterFiletype( "wal" );
		}

		[AssetListing(Title = "Quake 2")]
		public static List<string> GetModels()
		{
			ParseFiles();
			List<string> modelList = new();

			foreach ( string key in Files.Keys )
				if ( key.ToLower().EndsWith( ".bsp" ) )
					modelList.Add( key );

			return modelList;
		}

		private static void RegisterFiletype(string filetype )
		{
			foreach ( string fileName in FileSystem.Data.FindFile( "quake2", $"*.{filetype}", true ) )
			{
				if ( Files.ContainsKey( fileName ) ) continue;
				byte[] fileBytes = FileSystem.Data.ReadAllBytes( "quake2/" + fileName ).ToArray();
				Files.Add( fileName, fileBytes );
			}
		}
	}
}
