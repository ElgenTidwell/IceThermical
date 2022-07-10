using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Xna.Framework;
using IceThermical.EngineBase;

namespace IceThermical.Map
{
	public class LoadedITM
	{
		List<VertexPositionNormalTexture> vertices;

		public void LoadMap(string path)
		{
			ITMF map = Engine.instance.Content.Load<ITMF>(path);

			foreach(Brush brush in map.brushes)
			{
				if(brush.rotX == 0 && brush.rotY == 0 && brush.rotZ == 0)
				{
					BoundingBox box = new BoundingBox();
					box.Min = new Vector3(brush.centerX, brush.centerY, brush.centerZ) - new Vector3(brush.height / 2, brush.length / 2, brush.width / 2);
					box.Max = new Vector3(brush.centerX, brush.centerY, brush.centerZ) + new Vector3(brush.height / 2, brush.length / 2, brush.width / 2);
					Engine.instance.boxes.Add(box);
				}
				else
				{
					OrientedBoundingBox box = new OrientedBoundingBox(new Vector3(brush.centerX, brush.centerY, brush.centerZ),
						new Vector3(brush.rotX, brush.rotY, brush.rotZ), brush.height / 2, brush.length / 2, brush.width / 2);
					Engine.instance.orientedBoxes.Add(box);
				}

				if (brush.invis) continue;

				foreach(int tri in brush.triangles)
				{
					vertices.Add(new VertexPositionNormalTexture(brush.vertices[tri], brush.normals[tri], brush.uvs[tri]));
				}
			}
		}
	}

	[System.Serializable]
	public struct ITMF
	{
		public Brush[] brushes;

		public Entity[] entities;
	}
	[System.Serializable]
	public struct Brush
	{
		public float centerX, centerY, centerZ;
		public float length, width, height;
		public float rotX, rotY, rotZ;
		public int texID;
		public bool invis;
		public Vector2[] uvs;
		public Vector3[] normals;
		public Vector3[] vertices;
		public int[] triangles;
	}
	[System.Serializable]
	public struct Entity
	{
		public int pointer;
		public float posX, posY, posZ;
		public string[] data;
	}
}
