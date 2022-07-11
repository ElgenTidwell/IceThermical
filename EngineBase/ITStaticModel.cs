using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace IceThermical.EngineBase
{
	public class ITStaticModel
	{
		public Model model;
		public Vector3 pos,rot;
		List<BoundingBox> boundingBoxes;
		List<OrientedBoundingBox> OBBS; //WIP
		Effect effect;
		public float Shine = 200f, ShineScale = 0f;
		public Texture2D modelTexture;

		public ITStaticModel()
		{
			effect = Engine.instance.Content.Load<Effect>("Shaders/Textured");
		}

		public void init()
		{
			boundingBoxes = new List<BoundingBox>();

			foreach (ModelMesh mesh in model.Meshes)
			{
				Matrix meshTransform = Matrix.Identity;
				var box = BuildBoundingBox(mesh, meshTransform);
				Engine.instance.AddBox(box);
				boundingBoxes.Add(box);
			}
		}

		public void ChangeShader(string shaderName)
		{
			effect = Engine.instance.Content.Load<Effect>(shaderName);
		}

		private BoundingBox BuildBoundingBox(ModelMesh mesh, Matrix meshTransform)
		{
			// Create initial variables to hold min and max xyz values for the mesh
			Vector3 meshMax = new Vector3(float.MinValue);
			Vector3 meshMin = new Vector3(float.MaxValue);

			foreach (ModelMeshPart part in mesh.MeshParts)
			{
				// The stride is how big, in bytes, one vertex is in the vertex buffer
				// We have to use this as we do not know the make up of the vertex
				int stride = part.VertexBuffer.VertexDeclaration.VertexStride;

				VertexPositionNormalTexture[] vertexData = new VertexPositionNormalTexture[part.NumVertices];
				part.VertexBuffer.GetData(part.VertexOffset * stride, vertexData, 0, part.NumVertices, stride);

				// Find minimum and maximum xyz values for this mesh part
				Vector3 vertPosition = new Vector3();

				for (int i = 0; i < vertexData.Length; i++)
				{
					vertPosition = vertexData[i].Position;

					// update our values from this vertex
					meshMin = Vector3.Min(meshMin, vertPosition);
					meshMax = Vector3.Max(meshMax, vertPosition);
				}
			}

			// Create the bounding box
			BoundingBox box = new BoundingBox(meshMin+pos, meshMax+pos);
			return box;
		}

		public void Render(Camera cam, Texture2D UV)
		{
			Matrix localworld = Matrix.CreateFromYawPitchRoll(rot.Y, rot.X, rot.Z);
			localworld.Translation = pos;
			Render(cam, UV, localworld, cam.viewMatrix, new Vector3(-1f, -1f, 0.6f));
		}
		public void Render(Camera cam, Texture2D UV, Matrix w, Matrix v)
		{
			Render(cam, UV, w, v, new Vector3(-1f, -1f, 0.6f));
		}


		public void Render(Camera cam, Texture2D missing, Matrix w, Matrix v,Vector3 over)
		{
			foreach (ModelMesh m in model.Meshes)
			{
				foreach (ModelMeshPart part in m.MeshParts)
				{
					part.Effect = effect;

					Vector3 viewVector = Vector3.Transform(cam.lookTarget - cam.camPos, Matrix.CreateRotationY(0));
					viewVector.Normalize();

					Matrix worldInverseTransposeMatrix = Matrix.Transpose(Matrix.Invert(Matrix.CreateTranslation(pos) * cam.worldMatrix));
					effect.Parameters["World"].SetValue(w);
					effect.Parameters["View"].SetValue(v);
					effect.Parameters["Projection"].SetValue(cam.projectionMatrix);
					effect.Parameters["WorldInverseTranspose"].SetValue(worldInverseTransposeMatrix);

					//effect.Parameters["DiffuseLightDirection"].SetValue(over);

					effect.Parameters["ModelTexture"].SetValue(modelTexture == null ? missing:modelTexture);

					effect.Parameters["AmbientColor"].SetValue(Color.LightBlue.ToVector4());
					effect.Parameters["AmbientIntensity"].SetValue(0.02f);
					effect.Parameters["DiffuseLightDirection"].SetValue(new Vector3(0.8f,0.8f,0.8f));
					effect.Parameters["DiffuseColor"].SetValue(Color.White.ToVector4());
					effect.Parameters["DiffuseIntensity"].SetValue(1.0f);
					effect.Parameters["Shininess"].SetValue(Shine);
					effect.Parameters["SpecularColor"].SetValue(Color.White.ToVector4());
					effect.Parameters["SpecularIntensity"].SetValue(ShineScale);
					effect.Parameters["ViewVector"].SetValue(viewVector);

					//basicEffect.LightingEnabled = true;
					//effect.AmbientLightColor = new Vector3(0.25f, 0.25f, 0.25f);
					//basicEffect.TextureEnabled = true;

					//TODO: replace with actual lights in-game, this is temporary

					//var vec = over;
					//basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.8f,0.8f,0.8f);
					//basicEffect.DirectionalLight0.Direction = vec;

					//basicEffect.PreferPerPixelLighting = false;
				}
				m.Draw();
			}
		}
	}
}
