using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

// Thank you to the lovely dude at
// https://www.flipcode.com/archives/Quake_2_BSP_File_Format.shtml

namespace assetmanager.quakebsp
{
	public struct BSPLump
	{
		public uint Offset;
		public uint Length;
	}

	public struct BSPEdge
	{
		public Vector3 Start;
		public Vector3 End;

		public BSPEdge Inverse()
		{
			return new BSPEdge()
			{
				Start = End,
				End = Start
			};
		}
	}

	public struct BSPPlane
	{
		public Vector3 Normal;
		public float Distance;
	}

	public struct BSPFace
	{
		public BSPPlane Plane;
		public List<BSPEdge> Edges;
		public BSPTexture TextureInfo;
	}

	public struct BSPTexture
	{
		public Vector3 UAxis; 
		public float UOffset; // 16

		public Vector3 VAxis;
		public float VOffset; // 32

		public string TextureName;
	}

	public class BSPParser
	{
		public byte[] FileBytes;

		public BSPLump[] Lumps = new BSPLump[19];
		public List<BSPTexture> Textures = new();
		public List<Vector3> Vertices = new();
		public List<BSPPlane> Planes = new();
		public List<BSPEdge> Edges = new();
		public List<BSPFace> Faces = new();
		public List<int> FaceEdges = new();

		public BSPParser(byte[] fileBytes)
		{
			FileBytes = fileBytes;

			if ( Encoding.ASCII.GetString( FileBytes, 0, 4 ) != "IBSP" )
				throw new Exception("Incorrect file magic");

			if ( BitConverter.ToUInt32( FileBytes, 4 ) != 38 )
				throw new Exception( $"Incorrect file version" );

			ReadLumps();
			ReadTextureInfo();
			ReadVertices();
			ReadPlanes();

			ReadEdges();
			ReadFaceEdges();
			
			ReadFaces();
		}

		public void ReadLumps()
		{
			for( int i = 0; i < 19; i++ )
			{
				int lumpStart = 8 + 8 * i;
				
				Lumps[i] = new BSPLump()
				{
					Offset = BitConverter.ToUInt32(FileBytes, lumpStart),
					Length = BitConverter.ToUInt32(FileBytes, lumpStart + 4)
				};
			}
		}

		public void ReadVertices()
		{
			byte[] vertexBytes = FileBytes.Skip( (int)Lumps[2].Offset ).Take( (int)Lumps[2].Length ).ToArray();
			
			for(int start = 0; start < vertexBytes.Length; start += 12 )
				Vertices.Add( ReadVector( vertexBytes, start )  );
			
		}

		public void ReadEdges()
		{
			byte[] edgeBytes = FileBytes.Skip( (int)Lumps[11].Offset ).Take( (int)Lumps[11].Length ).ToArray();

			for(int start = 0; start < edgeBytes.Length; start += 4)
			{
				Edges.Add( new BSPEdge()
				{
					Start = Vertices[BitConverter.ToInt16( edgeBytes, start )],
					End = Vertices[BitConverter.ToInt16( edgeBytes, start + 2 )]
				} );
			}
		}

		public void ReadFaceEdges()
		{
			byte[] faceEdgeBytes = FileBytes.Skip( (int)Lumps[12].Offset ).Take( (int)Lumps[12].Length ).ToArray();

			for(int start = 0; start < faceEdgeBytes.Length; start += 4 )
			{
				FaceEdges.Add( BitConverter.ToInt32( faceEdgeBytes, start ) );
			}
		}

		public void ReadFaces()
		{
			byte[] faceBytes = FileBytes.Skip( (int)Lumps[6].Offset ).Take( (int)Lumps[6].Length ).ToArray();

			for( int start = 0; start < faceBytes.Length; start += 20 )
			{
				BSPPlane plane = Planes[BitConverter.ToUInt16(faceBytes, start)];
				int first = (int)BitConverter.ToUInt32( faceBytes, start + 4 );
				List<BSPEdge> faceEdges = new();

				for( int i = 0; i < (int)BitConverter.ToUInt16( faceBytes, start + 8 ); i++ )
				{
					int edge = FaceEdges[first + i];
					if ( edge > 0 ) faceEdges.Add( Edges[edge] );
					else faceEdges.Add( Edges[ -edge ].Inverse() );
				}

				Faces.Add( new()
				{
					TextureInfo = Textures[BitConverter.ToUInt16( faceBytes, start + 10 )],
					Edges = faceEdges,
					Plane = plane
				} );
			}
		}

		public void ReadPlanes()
		{
			byte[] planeBytes = FileBytes.Skip( (int)Lumps[1].Offset ).Take( (int)Lumps[1].Length ).ToArray(); 

			for( int start = 0; start < planeBytes.Length; start += 20 )
			{
				Planes.Add( new()
				{
					Normal = ReadVector( planeBytes, start ),
					Distance = BitConverter.ToSingle( planeBytes, start + 12 )
				} );
			}
		}

		public void ReadTextureInfo()
		{
			byte[] texBytes = FileBytes.Skip( (int)Lumps[5].Offset ).Take( (int)Lumps[5].Length ).ToArray();

			for( int start = 0; start < texBytes.Length; start += 76 )
			{
				Textures.Add( new()
				{
					UAxis = ReadVector( texBytes, start ),
					UOffset = BitConverter.ToSingle( texBytes, start + 12 ),

					VAxis = ReadVector( texBytes, start + 16 ),
					VOffset = BitConverter.ToSingle( texBytes, start + 28 ),

					TextureName = Encoding.ASCII.GetString(texBytes, start + 40, 32).Trim( '\0' )
				} );
			}
		}

		public Vector3 ReadVector(byte[] bytes, int start)
		{
			return new Vector3(
				BitConverter.ToSingle( bytes, start ),
				BitConverter.ToSingle( bytes, start + 4 ),
				BitConverter.ToSingle( bytes, start + 8 )
			);
		}

		/* ---------------------------------
			       FILE PROVIDER 
		--------------------------------- */

		[AssetProvider( Type = typeof( Model ) )]
		public static Model ParseBSP( string filePath )
		{
			if ( PAKParser.GetFile( filePath ) is not byte[] fileBytes )
				return null;

			Dictionary<string, List<Vertex>> meshVertices = new();
			BSPParser parsed = new( fileBytes );
			List<Vector3> collVertices = new();

			foreach(BSPFace face in parsed.Faces )
			{
				string textureName = face.TextureInfo.TextureName;
				if ( textureName.Contains("trigger") ) continue; // So it doesn't collide or render

				List<Vertex> meshVerts = meshVertices.GetOrCreate( textureName );
				var edges = face.Edges;

				for ( int i = 1; i < edges.Count - 1; i++ )
				{
					var verts = new Vector3[] { edges[0].Start, edges[i].End, edges[i].Start };

					foreach ( Vector3 vert in verts )
					{
						meshVerts.Add( MakeVertex( vert, face ) );
						collVertices.Add( vert );
					}
				}
			}

			ModelBuilder builder = new();

			foreach ( var kvp in meshVertices )
			{
				if ( kvp.Key.Contains( "sky" ) ) continue; // So it collides but doesn't render

				Mesh mesh = new( Assets.Get<Material>( kvp.Key ) );
				mesh.CreateVertexBuffer( kvp.Value.Count, Vertex.Layout, kvp.Value );
				builder.AddMesh( mesh );
			}

			builder.AddCollisionMesh(collVertices.ToArray(), Enumerable.Range(0, collVertices.Count() - 1).ToArray() );
			return builder.Create();
		}

		private static Vertex MakeVertex( Vector3 pos, BSPFace face )
		{
			Vector3 rot = Rotation.LookAt( face.Plane.Normal ).Up;

			var texInfo = face.TextureInfo;
			Vector2 texSize = WALParser.GetDimensions(texInfo.TextureName);
			float uAxis = pos.x*texInfo.UAxis.x + pos.y*texInfo.UAxis.y + pos.z*texInfo.UAxis.z + texInfo.UOffset;
			float vAxis = pos.x*texInfo.VAxis.x + pos.y*texInfo.VAxis.y + pos.z*texInfo.VAxis.z + texInfo.VOffset;

			return new Vertex()
			{
				Position = pos,
				Normal = face.Plane.Normal,
				Tangent = new Vector4( rot.x, rot.y, rot.z, -1 ),
				TexCoord0 = new Vector2( uAxis, vAxis ) / texSize,
				Color = Color.White
			};
		}
	}
}
