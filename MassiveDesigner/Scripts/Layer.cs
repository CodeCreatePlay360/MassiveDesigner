using System.Collections.Generic;
using UnityEngine;
using CodeCreatePlay.AutoInspector;


namespace MassiveDesinger
{
    [System.Serializable]
    public class Layer
    {
        [System.Serializable]
        public class Settings
        {
            [EditorFieldAttr(ControlType.boldLabel, "Layer Settings")]
            [EditorFieldAttr(ControlType.ItemsType, "itemsType")]
            public PaintMesh.ItemType itemsType = PaintMesh.ItemType.Trees;

            [EditorFieldAttr(ControlType.intField, "priorityIdx")]
            public int priorityIdx = 0;

            [EditorFieldAttr(ControlType.layerField, "layerMask")]
            public LayerMask layerMask = default; // the layer to paint on
             
            [FloatSliderAttr(ControlType.floatSlider, "sparse", 1, 5)]
            public float sparse = 1;

            [EditorFieldAttr(ControlType.textControl, "splatLayers")]
            public string splatLayers = "-1"; // indexes into terrain splat layers

            [EditorFieldAttr(ControlType.boolField, "useTerrainTextureStrength")]
            public bool useTerrainTextureStrength = false;

            public Settings()
            {

            }

            /// <summary>
            /// Copy constructor.
            /// </summary>
            /// <param name="settings"></param>
            public Settings(Settings settings)
            {
                itemsType = settings.itemsType;
                layerMask = settings.layerMask;
                splatLayers = settings.splatLayers;
                useTerrainTextureStrength = settings.useTerrainTextureStrength;
            }
        }

        public List<PaintMesh> paintMeshes = new List<PaintMesh>();
        [SerializeField] Dictionary<string, List<PaintMesh>> sortedPaintMeshes;  // sorted paintmeshes according to specie name
        public List<int> splatLayers = new List<int>();
        public Settings settings = new ();
        public string layerName = "NewLayer";
        public bool active = true;
        public int layerIndex;

        // the auto layout controls of currently selected PaintMesh
        public AutoInspector selPaintMeshAutoCntrls = null;
        public AutoInspector layerSettingsAutoCntrls = null;

        [SerializeField] private int selectedPaintMeshIdx = 0;
        [SerializeField] private string guid = "";


        public int SelectedPaintMeshIdx {
            get
            {
                return selectedPaintMeshIdx;
            }
            set
            {
                selectedPaintMeshIdx = value;
            }
        }

        public PaintMesh SelectedPaintMesh {
            get
            {
                if (selectedPaintMeshIdx < 0 || selectedPaintMeshIdx > paintMeshes.Count - 1)
                    return null;

                return paintMeshes[selectedPaintMeshIdx];
            }
        }

        [SerializeField] public List<GameObject> prototypes = new List<GameObject>();

        public string GUID
        {
            get
            {
                return guid;
            }
        }


        // constructor
        public Layer()
        {
            guid = System.Guid.NewGuid().ToString();
        }

        public void OnEnable()
        {
            foreach (var item in paintMeshes)
                if(item != null)
                    item.LoadAutoControls();
             
            LoadAutoControls();
        }

        public void OnSelect()
        {
            LoadAutoControls();
        }

        public void OnAddPrototype(GameObject gameObject)
        {
            prototypes.Add(null);
            paintMeshes.Add(null);
        }

        public void OnRemovePrototype(int idx)
        {
            MassiveDesigner.Instance.RemoveAllItems(onLayer: false, layerIdx: -1, paintMesh: paintMeshes[idx]);

            prototypes.RemoveAt(idx);
            paintMeshes.RemoveAt(idx);

            if(paintMeshes.Count > 1)
                SelectPaintMesh(idx-1);

            MassiveDesigner.Externals.UpdateTerrainPrototypes(MassiveDesigner.Instance.Layers);
        }

        public void OnPrototypeChange(int index, GameObject prototype)
        {
            void Remove()
            {
                prototypes.RemoveAt(index);
                paintMeshes.RemoveAt(index);
            }

            if(MassiveDesigner.Externals.VerifyUnityTerrainItem(prototype, settings.itemsType))
            {
                prototypes[index] = prototype;
                paintMeshes[index] = prototype.GetComponent<PaintMesh>();
                paintMeshes[index].properties.itemType = settings.itemsType;
                paintMeshes[index].layerPriorityIdx = settings.priorityIdx;

                LoadAutoControls();
                SelectPaintMesh(index);

                MassiveDesigner.Externals.UpdateTerrainPrototypes(MassiveDesigner.Instance.Layers);
            }
            else
            {
                Remove();
            }
        }

        public void VerifyPaintMeshes()
        {
            for (int i = 0; i < paintMeshes.Count; i++)
            {
                if (paintMeshes[i] == null)
                    continue;
                if(!MassiveDesigner.Externals.VerifyUnityTerrainItem(paintMeshes[i].gameObject, settings.itemsType))
                    OnRemovePrototype(i);
            }
        }

        public void SelectPaintMesh(int idx)
        {
            if (idx < 0)
                idx = 0;

            selectedPaintMeshIdx = idx;
            paintMeshes[selectedPaintMeshIdx].OnSelect();
            LoadAutoControls();
        }

        public int GetSelectedPaintMeshIdx()
        {
            return selectedPaintMeshIdx;
        }

        /// <summary>
        /// updates paint meshes in this layer, this method should be called only once
        /// before any spawn operation.
        /// </summary>
        public void UpdatePaintMeshes(int layerPriorityIdx)
        {
            for (int i = 0; i < paintMeshes.Count; i++)
            {
                if (paintMeshes[i] != null)
                {
                    paintMeshes[i].properties.itemType = settings.itemsType;
                    paintMeshes[i].layerPriorityIdx = layerPriorityIdx;
                    UnityEditor.EditorUtility.SetDirty(paintMeshes[i]);
                }
            }
        }
          
        /// <summary>
        /// sorts paintmeshes according to specie name, this method should be called only once
        /// before any spawn operation.
        /// </summary>
        public void SortPaintMeshes()
        {
            sortedPaintMeshes = new Dictionary<string, List<PaintMesh>>();

            foreach (var item in paintMeshes)
            {
                if (item != null)
                {
                    // add a key for specie if already does not exists.
                    if (!sortedPaintMeshes.ContainsKey(item.properties.specieName))
                        sortedPaintMeshes[item.properties.specieName] = new List<PaintMesh>();

                    sortedPaintMeshes[item.properties.specieName].Add(item);
                }
            }
        }

        public bool HasGameObject(GameObject go)
        {
            foreach (var item in paintMeshes)
            {
                if (item != null && item.gameObject == go)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// gets splat layers as list of ints from settings.splatLayers string, this method should be called only once
        /// before any spawn operation.
        /// </summary>   
        public void GetSplatLayers()
        {
            splatLayers = new List<int>();
            string[] splatLayers_str = settings.splatLayers.Split(",");
            int splatLayerIndex = -1;

            // make sure splat layers only contains "ints" and ","
            bool error = false;
            try
            {
                for (int i = 0; i < splatLayers_str.Length; i++)
                {
                    if (splatLayers_str[i] != ",")
                    {
                        splatLayerIndex = int.Parse(splatLayers_str[i]);
                        splatLayers.Add(Mathf.Clamp(splatLayerIndex, 0, MassiveDesigner.Externals.numSplatLayers-1));
                    }
                }
            }
            catch
            {
                error = true;
            }
            finally
            {
                if(error)
                {
                    splatLayers.Clear();
                    Debug.LogWarningFormat("Unable to parse splatlayers for layer {0}", layerName);
                }
            }
        }


        private void LoadAutoControls()
        {
            System.Type t;
            object obj;

            // layer AutoEd
            t = settings.GetType();
            obj = settings;
            layerSettingsAutoCntrls = new AutoInspector(t, ref obj);

            // paint mesh AutoEd
            if (paintMeshes.Count > 0)
            {
                if (paintMeshes[selectedPaintMeshIdx] != null)
                {
                    t = paintMeshes[selectedPaintMeshIdx].GetType();
                    obj = paintMeshes[selectedPaintMeshIdx];
                    selPaintMeshAutoCntrls = new AutoInspector(t, ref obj);
                }
            }
        }

        public void CopySettings()
        {
            MassiveDesigner.Instance.layerCopiedSettings = settings;
            Debug.LogFormat("[MassiveDesigner] Copied layer {0} settings", layerName);
        }

        public void PasteSettings()
        {
            if (MassiveDesigner.Instance.layerCopiedSettings != null)
            {
                Settings settings = new Settings(MassiveDesigner.Instance.layerCopiedSettings);
                this.settings = settings;
                LoadAutoControls();
            }

            MassiveDesigner.Instance.layerCopiedSettings = null;
        }
    }
}