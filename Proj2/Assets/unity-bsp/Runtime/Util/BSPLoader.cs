using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor.AssetImporters;
using UnityEditor;
#endif
using LibBSP;

namespace BSPImporter
{
#if UNITY_5_6_OR_NEWER
    using Vertex = UIVertex;
#endif

    public class BSPLoader
    {
        public enum MeshCombineOptions
        {
            None,
            PerMaterial,
            PerEntity,
        }

        /// <summary>
        /// Struct containing various settings for the BSP Import process.
        /// </summary>
        [Serializable]
        public struct Settings
        {
            public string path;
            public MeshCombineOptions meshCombineOptions;
            public int curveTessellationLevel;
            public Action<EntityInstance, List<EntityInstance>> entityCreatedCallback;
            public float scaleFactor;
        }

        public struct EntityInstance
        {
            public Entity entity;
            public GameObject gameObject;
        }

        public static bool IsRuntime
        {
            get
            {
#if UNITY_EDITOR
                return EditorApplication.isPlaying;
#else
                return true;
#endif
            }
        }
#if UNITY_EDITOR

        public Settings settings;

        private BSP bsp;
        private GameObject root;
        private List<EntityInstance> entityInstances = new List<EntityInstance>();
        private Dictionary<string, List<EntityInstance>> namedEntities = new Dictionary<string, List<EntityInstance>>();
        private Dictionary<string, Material> materialDirectory = new Dictionary<string, Material>();
        private Color[] palette = new Color[256];
        private AssetImportContext ctx;
        private bool savePrefab = false;


        public GameObject LoadBSP(AssetImportContext ctx)
        {
            if (string.IsNullOrEmpty(settings.path) || !File.Exists(settings.path))
            {
                Debug.LogError("Cannot import " + settings.path + ": The path is invalid.");
                return null;
            }

            BSP bsp = new BSP(new FileInfo(settings.path));

            if (bsp == null)
            {
                Debug.LogError("Cannot import BSP: The object was null.");
                return null;
            }
            this.bsp = bsp;

            if (ctx != null)
            {
                this.ctx = ctx;
                savePrefab = true;
            }

            LoadPalette("Assets/qpalette.png");

            for (int i = 0; i < bsp.Entities.Count; ++i)
            {
                Entity entity = bsp.Entities[i];

                EntityInstance instance = CreateEntityInstance(entity);
                entityInstances.Add(instance);
                namedEntities[entity.Name].Add(instance);

                int modelNumber = entity.ModelNumber;
                if (modelNumber >= 0)
                {
                    BuildMesh(instance);
                }
                else
                {
                    Vector3 angles = entity.Angles;
                    instance.gameObject.transform.rotation = Quaternion.Euler(-angles.x, angles.y, angles.z);
                }

                instance.gameObject.transform.position = entity.Origin.SwizzleYZ() * settings.scaleFactor;

                if (instance.entity.ClassName == "worldspawn")
                {
                    instance.gameObject.layer = LayerMask.NameToLayer("worldspawn");
                    GameObjectUtility.SetStaticEditorFlags(instance.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
                    MeshRenderer meshRenderer = instance.gameObject.GetComponent<MeshRenderer>();
                    if (meshRenderer == null) meshRenderer = instance.gameObject.AddComponent<MeshRenderer>();
                    meshRenderer.staticShadowCaster = true;
                    meshRenderer.lightProbeUsage = LightProbeUsage.Off;
                }
            }

            root = new GameObject(bsp.MapName);
            foreach (KeyValuePair<string, List<EntityInstance>> pair in namedEntities)
            {
                SetUpEntityHierarchy(pair.Value);
            }

            if (settings.entityCreatedCallback != null)
            {
                foreach (EntityInstance instance in entityInstances)
                {
                    string target = instance.entity["target"];
                    if (namedEntities.ContainsKey(target) && !string.IsNullOrEmpty(target))
                    {
                        settings.entityCreatedCallback(instance, namedEntities[target]);
                    }
                    else
                    {
                        settings.entityCreatedCallback(instance, new List<EntityInstance>(0));
                    }
                }
            }

            SetStatic(root);

            if (savePrefab)
            {
                ctx.AddObjectToAsset(root.name, root);
                ctx.SetMainObject(root);
            }
            
            return root;
        }

        static void SetStatic(GameObject obj)
        {
            obj.isStatic = true;

            foreach (Transform child in obj.transform)
            {
                SetStatic(child.gameObject);
            }
        }

        private void LoadPalette(string palettePath)
        {
            Texture2D png = AssetDatabase.LoadAssetAtPath(palettePath, typeof(Texture2D)) as Texture2D;

            if (png == null) return;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    palette[i * 16 + j] = png.GetPixel(j * 16, (15 - i) * 16);
                }
            }
        }

        private Texture2D LoadTextureFromWAD(LibBSP.Texture textureData)
        {
            Texture2D texture = new Texture2D((int)textureData.Dimensions.x, (int)textureData.Dimensions.y);
            texture.filterMode = FilterMode.Point;

            if (textureData.Mipmaps.Length > 0 && (bsp.MapType == MapType.Quake || bsp.MapType == MapType.Quake2))
            {
                for (int i = 0; i < textureData.Mipmaps[0].Length; i++)
                {
                    texture.SetPixel((int)(i % textureData.Dimensions.x), (int)(textureData.Dimensions.y - (i / textureData.Dimensions.x)), palette[textureData.Mipmaps[0][i]]);
                }
            }
            else // Half Life uses external wad
            {

            }

            texture.name = textureData.Name;
            texture.Apply(true, false);

            if (savePrefab)
            {
                ctx.AddObjectToAsset(texture.name + ".asset", texture);
            }

            return texture;
        }

        private Texture2D LoadTextureFromPNG(LibBSP.Texture textureData)
        {
            string path = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ctx.assetPath)), "textures/" + textureData.Name);
            Debug.Log(path);

            Texture2D texture;
            if ((texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path + ".png", typeof(Texture2D))) != null) return texture;
            else if ((texture = (Texture2D)AssetDatabase.LoadAssetAtPath(path + ".jpg", typeof(Texture2D))) != null) return texture;

            return null;
        }


        private Texture2D LoadTexture(Face face, string textureName)
        {
            int textureIndex = bsp.GetTextureIndex(face);

            if (textureIndex != -1)
            {
                LibBSP.Texture textureData = bsp.Textures[textureIndex];

                // try png first
                Texture2D texture = LoadTextureFromPNG(textureData);
                if (texture != null) return texture;
                else return LoadTextureFromWAD(textureData);
            }
      
            return new Texture2D(1, 1);
        }

        public void LoadMaterial(Face face, string textureName)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");

            Texture2D texture = LoadTexture(face, textureName);
            texture.filterMode = FilterMode.Point;

            Material material = new Material(shader);
            material.name = textureName;

            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Glossiness", 0);
            material.SetFloat("_Smoothness", 0);
            material.SetFloat("_SpecularHighlights", 0f);
            material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF");

            if (savePrefab)
            {
                ctx.AddObjectToAsset(textureName + ".mat", material);
            }

            materialDirectory[textureName] = material;
        }

        protected EntityInstance CreateEntityInstance(Entity entity)
        {
            // Entity.name guaranteed not to be null, empty string is a valid Dictionary key
            if (!namedEntities.ContainsKey(entity.Name) || namedEntities[entity.Name] == null)
            {
                namedEntities[entity.Name] = new List<EntityInstance>();
            }

            EntityInstance instance = new EntityInstance()
            {
                entity = entity,
                gameObject = new GameObject(entity.ClassName + (!string.IsNullOrEmpty(entity.Name) ? " " + entity.Name : string.Empty))
            };

            instance.gameObject.name += instance.gameObject.GetInstanceID();

            return instance;
        }

        protected void SetUpEntityHierarchy(List<EntityInstance> instances)
        {
            foreach (EntityInstance instance in instances)
            {
                SetUpEntityHierarchy(instance);
            }
        }

        protected void SetUpEntityHierarchy(EntityInstance instance)
        {
            if (!instance.entity.ContainsKey("parentname"))
            {
                instance.gameObject.transform.parent = root.transform;
                return;
            }

            if (namedEntities.ContainsKey(instance.entity["parentname"]))
            {
                if (namedEntities[instance.entity["parentname"]].Count > 1)
                {
                    Debug.LogWarning(string.Format("Entity \"{0}\" claims to have parent \"{1}\" but more than one matching entity exists.",
                        instance.gameObject.name,
                        instance.entity["parentname"]), instance.gameObject);
                }
                instance.gameObject.transform.parent = namedEntities[instance.entity["parentname"]][0].gameObject.transform;
            }
            else
            {
                Debug.LogWarning(string.Format("Entity \"{0}\" claims to have parent \"{1}\" but no such entity exists.",
                    instance.gameObject.name,
                    instance.entity["parentname"]), instance.gameObject);
            }
        }

        protected void BuildMesh(EntityInstance instance)
        {
            int modelNumber = instance.entity.ModelNumber;
            Model model = bsp.Models[modelNumber];
            Dictionary<string, List<Mesh>> textureMeshMap = new Dictionary<string, List<Mesh>>();
            GameObject gameObject = instance.gameObject;

            List<Face> faces = bsp.GetFacesInModel(model);
            int i = 0;
            for (i = 0; i < faces.Count; ++i)
            {
                Face face = faces[i];
                if (face.NumEdgeIndices <= 0 && face.NumVertices <= 0)
                {
                    continue;
                }

                int textureIndex = bsp.GetTextureIndex(face);
                string textureName = "";
                if (textureIndex >= 0)
                {
                    LibBSP.Texture texture = bsp.Textures[textureIndex];
                    textureName = LibBSP.Texture.SanitizeName(texture.Name, bsp.MapType);

                    if (!textureName.StartsWith("tools/", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!textureMeshMap.ContainsKey(textureName) || textureMeshMap[textureName] == null)
                        {
                            textureMeshMap[textureName] = new List<Mesh>();
                        }

                        textureMeshMap[textureName].Add(CreateFaceMesh(face, textureName));
                    }
                }
            }

            if (modelNumber == 0)
            {
                if (bsp.LODTerrains != null)
                {
                    foreach (LODTerrain lodTerrain in bsp.LODTerrains)
                    {
                        if (lodTerrain.TextureIndex >= 0)
                        {
                            LibBSP.Texture texture = bsp.Textures[lodTerrain.TextureIndex];
                            string textureName = texture.Name;

                            if (!textureMeshMap.ContainsKey(textureName) || textureMeshMap[textureName] == null)
                            {
                                textureMeshMap[textureName] = new List<Mesh>();
                            }

                            textureMeshMap[textureName].Add(CreateLoDTerrainMesh(lodTerrain, textureName));
                        }
                    }
                }
            }

            if (settings.meshCombineOptions != MeshCombineOptions.None)
            {
                Mesh[] textureMeshes = new Mesh[textureMeshMap.Count];
                Material[] materials = new Material[textureMeshes.Length];
                i = 0;
                foreach (KeyValuePair<string, List<Mesh>> pair in textureMeshMap)
                {
                    textureMeshes[i] = MeshUtils.CombineAllMeshes(pair.Value.ToArray(), true, false);

                    if (textureMeshes[i].vertices.Length > 0)
                    {
                        if (materialDirectory.ContainsKey(pair.Key))
                        {
                            materials[i] = materialDirectory[pair.Key];
                        }
                        if (settings.meshCombineOptions == MeshCombineOptions.PerMaterial) // MeshCombineOptions.PerMaterial
                        {
                            GameObject textureGameObject = new GameObject(pair.Key);
                            textureGameObject.transform.parent = gameObject.transform;
                            textureGameObject.transform.localPosition = Vector3.zero;
                            textureMeshes[i].Scale(settings.scaleFactor);

                            textureMeshes[i].RecalculateNormals();
                            Unwrapping.GenerateSecondaryUVSet(textureMeshes[i]);

                            textureMeshes[i].name = gameObject.name + "_mesh_" + materials[i].name;

                            textureMeshes[i].AddToGameObject(new Material[] { materials[i] }, textureGameObject);

                            if (savePrefab)
                                ctx.AddObjectToAsset(textureMeshes[i].name, textureMeshes[i]);
                        }
                        ++i;
                    }
                }

                if (settings.meshCombineOptions == MeshCombineOptions.PerEntity) // MeshCombineOptions.PerEntity
                { 
                    Mesh mesh = MeshUtils.CombineAllMeshes(textureMeshes, false, false);

                    if (mesh.vertices.Length > 0)
                    {
                        mesh.TransformVertices(gameObject.transform.localToWorldMatrix);
                        mesh.Scale(settings.scaleFactor);

                        mesh.RecalculateNormals();
                        Unwrapping.GenerateSecondaryUVSet(mesh);

                        mesh.name = gameObject.name + "_mesh";

                        mesh.AddToGameObject(materials, gameObject);

                        if (savePrefab)
                            ctx.AddObjectToAsset(mesh.name, mesh);
                    }
                }
            }

            else // MeshCombineOptions.None
            {
                i = 0;
                foreach (KeyValuePair<string, List<Mesh>> pair in textureMeshMap)
                {
                    GameObject textureGameObject = new GameObject(pair.Key);
                    textureGameObject.transform.parent = gameObject.transform;
                    textureGameObject.transform.localPosition = Vector3.zero;
                    Material material = materialDirectory[pair.Key];
                    foreach (Mesh mesh in pair.Value)
                    {
                        if (mesh.vertices.Length > 0)
                        {
                            GameObject faceGameObject = new GameObject("Face");
                            faceGameObject.transform.parent = textureGameObject.transform;
                            faceGameObject.transform.localPosition = Vector3.zero;
                            mesh.Scale(settings.scaleFactor);

                            mesh.RecalculateNormals();
                            Unwrapping.GenerateSecondaryUVSet(mesh);

                            mesh.AddToGameObject(new Material[] { material }, faceGameObject);

                            if (savePrefab)
                                ctx.AddObjectToAsset("mesh", mesh);
                        }
                    }
                    ++i;
                }
            }

        }

        protected Mesh CreateFaceMesh(Face face, string textureName)
        {
            Vector2 dims;
            if (!materialDirectory.ContainsKey(textureName))
            {
                LoadMaterial(face, textureName);
            }
            if (materialDirectory[textureName].HasProperty("_BaseMap") && materialDirectory[textureName].mainTexture != null)
            {
                dims = new Vector2(materialDirectory[textureName].mainTexture.width, materialDirectory[textureName].mainTexture.height);
            }
            else
            {
                dims = new Vector2(128, 128);
            }

            Mesh mesh;
            if (face.DisplacementIndex >= 0)
            {
                mesh = MeshUtils.CreateDisplacementMesh(bsp, face, dims);
            }
            else
            {
                mesh = MeshUtils.CreateFaceMesh(bsp, face, dims, settings.curveTessellationLevel);
            }

            return mesh;
        }

        // Ainda nao sei para que é isto
        protected Mesh CreateLoDTerrainMesh(LODTerrain lodTerrain, string textureName)
        {
            if (!materialDirectory.ContainsKey(textureName))
            {
                //LoadMaterial(textureName);
            }

            return MeshUtils.CreateMoHAATerrainMesh(bsp, lodTerrain);
        }
#endif
    }
}