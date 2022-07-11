using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using IceThermical.EngineBase;

namespace IceThermical.Map
{
	public class LoadedITM
	{
		List<Tuple<BoundingBox,VertexPositionNormalTexture[]>> brushes = new List<Tuple<BoundingBox, VertexPositionNormalTexture[]>>();
		Effect effect;
		Texture2D uvgrid;
		public void LoadMap(string path)
		{
			effect = Engine.instance.Content.Load<Effect>("Shaders/Textured");
			uvgrid = Engine.instance.Content.Load<Texture2D>("Textures/UVGrid");
			ITMF map = Engine.instance.Content.Load<ITMF>(path);
			
			//effect.Parameters["DiffuseLightDirection"].SetValue(over);

			effect.Parameters["AmbientColor"].SetValue(Color.LightBlue.ToVector4());
			effect.Parameters["AmbientIntensity"].SetValue(0.02f);
			effect.Parameters["DiffuseLightDirection"].SetValue(new Vector3(0.8f, 0.8f, 0.8f));
			effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector4());
			effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
			effect.Parameters["SpecularIntensity"].SetValue(0.0f);

			foreach (Brush brush in map.brushes)
			{
				if(brush.rotX == 0 && brush.rotY == 0 && brush.rotZ == 0)
				{
					BoundingBox box = new BoundingBox();
					box.Min = new Vector3(brush.centerX, brush.centerY, brush.centerZ) - new Vector3(brush.length / 2, brush.width / 2, brush.height / 2);
					box.Max = new Vector3(brush.centerX, brush.centerY, brush.centerZ) + new Vector3(brush.length / 2, brush.width / 2, brush.height / 2);
					Engine.instance.boxes.Add(box);
				}
				else
				{
					OrientedBoundingBox box = new OrientedBoundingBox(new Vector3(brush.centerX, brush.centerY, brush.centerZ),
						new Vector3(brush.rotX, brush.rotY, brush.rotZ), brush.length / 2, brush.width / 2, brush.height / 2);
					Engine.instance.orientedBoxes.Add(box);
				}

				if (brush.invis) continue;

				BoundingBox bbox = new BoundingBox();
				bbox.Min = new Vector3(brush.centerX, brush.centerY, brush.centerZ) - new Vector3(brush.length / 2, brush.width / 2, brush.height / 2);
				bbox.Max = new Vector3(brush.centerX, brush.centerY, brush.centerZ) + new Vector3(brush.length / 2, brush.width / 2, brush.height / 2);

				VertexPositionNormalTexture[] verts=new VertexPositionNormalTexture[brush.triangles.Length];
				for(int i = 0; i < brush.triangles.Length; i ++)
				{
					verts[i] = new VertexPositionNormalTexture(brush.vertices[brush.triangles[i]] + new Vector3(brush.centerX, brush.centerY, brush.centerZ),
										brush.normals[brush.triangles[i]], brush.uvs[brush.triangles[i]]);
				}
				brushes.Add(Tuple.Create(bbox,verts));
			}
		}
		public void RenderMap(Camera cam,GraphicsDevice gd)
		{
			if (brushes.Count == 0) return;

			effect.Parameters["World"].SetValue(cam.worldMatrix);
			effect.Parameters["View"].SetValue(cam.viewMatrix);
			effect.Parameters["Projection"].SetValue(cam.projectionMatrix);

			Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(cam.worldMatrix));

			effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

			Vector3 viewVector = Vector3.Transform(Engine.instance.player.camera.lookTarget - Engine.instance.player.camera.camPos, Matrix.CreateRotationY(0));
			viewVector.Normalize();

			effect.Parameters["ViewVector"].SetValue(viewVector);

			RasterizerState state = new RasterizerState();
			state.CullMode = CullMode.CullClockwiseFace;

			RasterizerState _old = gd.RasterizerState;

			gd.RasterizerState = state;

			for (int i = 0; i < brushes.Count; i ++)
			{
				//if (Vector3.Dot(cam.camPos - brushes[i].Item1.Min, cam.forward) > .5f && Vector3.Dot(cam.camPos - brushes[i].Item1.Max, cam.forward) > .5f) continue;

				VertexBuffer buffer = new VertexBuffer(gd, typeof(VertexPositionNormalTexture),brushes[i].Item2.Length,BufferUsage.WriteOnly);
				buffer.SetData(brushes[i].Item2);

				gd.SetVertexBuffer(buffer);
				effect.Parameters["ModelTexture"].SetValue(uvgrid);

				foreach (EffectPass pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();
					gd.DrawPrimitives(PrimitiveType.TriangleList, 0, brushes[i].Item2.Length);
				}
			}

			gd.RasterizerState = _old;
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
		public Vec2[] uvs;
		public Vec3[] normals;
		public Vec3[] vertices;
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
[System.Serializable]
public struct Vec3
{
	public float X, Y, Z;

	public Vec3(float x, float y, float z)
	{
		this.X = x;
		this.Y = y;
		this.Z = z;
	}

	public static implicit operator Vector3(Vec3 other)
	{
		return new Vector3(other.X, other.Y, other.Z);
	}
	public static implicit operator Vec3(Vector3 other)
	{
		return new Vec3(other.X, other.Y, other.Z);
	}
}
[System.Serializable]
public struct Vec2
{
	public float X, Y;
	public Vec2(float x, float y)
	{
		this.X = x;
		this.Y = y;
	}

	public static implicit operator Vector2(Vec2 other)
	{
		return new Vector2(other.X, other.Y);
	}
	public static implicit operator Vec2(Vector2 other)
	{
		return new Vec2(other.X, other.Y);
	}
}