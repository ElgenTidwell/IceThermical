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
		List<Tuple<BoundingBox,VertexPositionNormalTexture[]>> brushes = new List<Tuple<BoundingBox, VertexPositionNormalTexture[]>>();

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

				BoundingBox bbox = new BoundingBox();
				bbox.Min = new Vector3(brush.centerX, brush.centerY, brush.centerZ) - new Vector3(brush.height / 2, brush.length / 2, brush.width / 2);
				bbox.Max = new Vector3(brush.centerX, brush.centerY, brush.centerZ) + new Vector3(brush.height / 2, brush.length / 2, brush.width / 2);

				VertexPositionNormalTexture[] verts=new VertexPositionNormalTexture[brush.triangles.Length];
				int i = 0;
				foreach (int tri in brush.triangles)
				{
					verts[i] = new VertexPositionNormalTexture(brush.vertices[tri], brush.normals[tri], brush.uvs[tri]);
					i++;
				}
				brushes.Add(Tuple.Create(bbox,verts));
			}
		}
		public void RenderMap(Camera cam,GraphicsDevice gd)
		{
			if (brushes.Count == 0) return;

			for(int i = 0; i < brushes.Count; i ++)
			{
				if (Vector3.Dot(cam.camPos - brushes[i].Item1.Min, cam.forward) < 1) continue;

				VertexBuffer buffer = new VertexBuffer(gd, typeof(VertexPositionNormalTexture),brushes[i].Item2.Length,BufferUsage.WriteOnly);
				buffer.SetData(brushes[i].Item2);

				gd.SetVertexBuffer(buffer);
				gd.DrawPrimitives(PrimitiveType.TriangleList,0,buffer.VertexCount);
				gd.SetVertexBuffer(null);
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
