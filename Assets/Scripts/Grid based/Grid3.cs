using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GridDataStructure
{
    public class Grid3
    {
        private Vector3 _gridSpacing;
        public Vector3 GridSpacing { get { return _gridSpacing; } set { if (value != null) _gridSpacing = value; } }
        private Vector3 _resoloution;
        public Vector3 Resoloution { get { return _resoloution; } set { if (value != null) _resoloution = value; } }
        private Vector3 _origin;
        public Vector3 Origin { get { return _origin; } set { if (value != null) _origin = value; } }
        private BoxCollider _boundBox;
        public BoxCollider BoundingBox { get { return _boundBox; } set { if (value != null) _boundBox = value; } }

        public Grid3(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox)
        {
            GridSpacing = gridSpacing;
            Resoloution = resoloution;
            Origin = origin;
            BoundingBox = boundinBox;
        }

        protected void ResetParameters(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin)
        {
            GridSpacing = gridSpacing;
            Resoloution = resoloution;
            Origin = origin;
            BoundingBox.size = new Vector3(resoloution.x * gridSpacing.x, resoloution.y * gridSpacing.y, resoloution.z * gridSpacing.z);
            BoundingBox.center = origin + new Vector3(
                0.5f * resoloution.x * gridSpacing.x,
                0.5f * resoloution.y * gridSpacing.y,
                0.5f * resoloution.z * gridSpacing.z);
        }
    }

    public abstract class ScalarGrid : Grid3
    {

        protected float initialValue;
        protected float[] data;

        public ScalarGrid(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox) :
            base(gridSpacing, resoloution, origin, boundinBox)
        {   
        }

        public abstract Vector3 DataSize();
        public abstract Vector3 DataOrigin();
        public abstract void Resize(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin);

        public float GetDataAt(int i, int j, int k, Vector3 dataSize)
        {
            int index = k * (int)dataSize.y * (int)dataSize.x + j * (int)dataSize.x + i;
            return data[index];
        }

        public float[] GetData()
        {
            return data;
        }

        public float[] InitializeData(Vector3 dataSize, float initialValue)
        {
            data = new float[(int)dataSize.x * (int)dataSize.y * (int)dataSize.z];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = initialValue;
            }
            return data;
        }
    }

    public abstract class VectorGrid : Grid3
    {
        public VectorGrid(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox) :
            base(gridSpacing, resoloution, origin, boundinBox)
        {
        }
        public abstract Vector3 DataSize();
        public abstract Vector3 DataOrigin();
        public abstract void Resize(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin);
    }

    public class CellCenterScalarGrid : ScalarGrid
    {
        public CellCenterScalarGrid(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox) :
            base(gridSpacing, resoloution, origin, boundinBox)
        {
            initialValue = 0;
            data = InitializeData(DataSize(), initialValue);
        }

        public override Vector3 DataSize()
        {
            return Resoloution;
        }

        public override Vector3 DataOrigin()
        {
            return Origin + 0.5f * GridSpacing;
        }

        public override void Resize(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin)
        {
            ResetParameters(gridSpacing, resoloution, origin);
            data = InitializeData(DataSize(), initialValue);
        }
        
    }

    public class VertexCenterScalarGrid : ScalarGrid
    {
        public VertexCenterScalarGrid(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox) :
            base(gridSpacing, resoloution, origin, boundinBox)
        {
            initialValue = 0;
            data = InitializeData(DataSize(), initialValue);
        }

        public override Vector3 DataOrigin()
        {
            return Origin;
        }

        public override Vector3 DataSize()
        {
            return Resoloution + Vector3.one;
        }

        public override void Resize(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin)
        {
            ResetParameters(gridSpacing, resoloution, origin);
            data = InitializeData(DataSize(), initialValue);
        }
    }

    public class CollocatedVectorGrid : VectorGrid
    {

        protected Vector3[] data;
        protected Vector3 initialValue;

        public CollocatedVectorGrid(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox) :
            base(gridSpacing, resoloution, origin, boundinBox)
        {
        }

        public override Vector3 DataSize()
        {
            throw new System.NotImplementedException();
        }

        public override Vector3 DataOrigin()
        {
            throw new System.NotImplementedException();
        }

        public override void Resize(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetDataAt(int i, int j, int k, Vector3 dataSize)
        {
            int index = k * (int)dataSize.y * (int)dataSize.x + j * (int)dataSize.x + i;
            return data[index];
        }

        public Vector3[] GetData()
        {
            return data;
        }

        public Vector3[] InitializeData(Vector3 dataSize, Vector3 initialValue)
        {
            data = new Vector3[(int)dataSize.x * (int)dataSize.y * (int)dataSize.z];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = initialValue;
            }
            return data;
        }
    }

    public class CellCenteredCollocatedVector : CollocatedVectorGrid
    {
        public CellCenteredCollocatedVector(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox) :
            base(gridSpacing, resoloution, origin, boundinBox)
        {
            initialValue = Vector3.zero;
            data = InitializeData(DataSize(), initialValue);
        }

        public override Vector3 DataOrigin()
        {
            return Origin + 0.5f * GridSpacing;
        }

        public override Vector3 DataSize()
        {
            return Resoloution;
        }

        public override void Resize(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin)
        {
            ResetParameters(gridSpacing, resoloution, origin);
            data = InitializeData(DataSize(), initialValue);
        }
    }

    public class VertexCenteredCollocatedVector : CollocatedVectorGrid
    {
        public VertexCenteredCollocatedVector(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox) :
            base(gridSpacing, resoloution, origin, boundinBox)
        {
            initialValue = Vector3.zero;
            data = InitializeData(DataSize(), initialValue);
        }

        public override Vector3 DataOrigin()
        {
            return Origin;
        }

        public override Vector3 DataSize()
        {
            return Resoloution + Vector3.one;
        }

        public override void Resize(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin)
        {
            initialValue = Vector3.zero;
            data = InitializeData(DataSize(), initialValue);
        }
    }

    public class FaceCenteredGrid : VectorGrid
    {
        protected float[] u;
        protected float[] w;
        protected float[] v;
        protected float initialValue;

        public FaceCenteredGrid(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundinBox) :
            base(gridSpacing, resoloution, origin, boundinBox)
        {
            initialValue = 0;
            u = InitializeAxisData(u, initialValue, new Vector3(Resoloution.x + 1, Resoloution.y, Resoloution.z));
            w = InitializeAxisData(w, initialValue, new Vector3(Resoloution.x, Resoloution.y, Resoloution.z + 1));
            v = InitializeAxisData(v, initialValue, new Vector3(Resoloution.x, Resoloution.y + 1, Resoloution.z));
        }

        public override Vector3 DataOrigin()
        {
            throw new System.NotImplementedException();
        }

        public override Vector3 DataSize()
        {
            throw new System.NotImplementedException();
        }

        public override void Resize(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin)
        {
            ResetParameters(gridSpacing, resoloution, origin);
            u = InitializeAxisData(u, initialValue, DataSize_U());
            w = InitializeAxisData(w, initialValue, DataSize_U());
            v = InitializeAxisData(v, initialValue, DataSize_V());
        }
        
        public float U_AccessorAt(int i, int j, int k, Vector3 dataSize)
        {
            int index = k * (int)dataSize.y * (int)dataSize.x + j * (int)dataSize.x + i;
            return u[index];
        }

        public float[] U_Accessor()
        {
            return u;
        }

        public Vector3 DataOrigin_U()
        {
            return Origin + 0.5f * new Vector3(0, GridSpacing.y, GridSpacing.z);
        }

        public Vector3 DataSize_U()
        {
            return new Vector3(Resoloution.x + 1, Resoloution.y, Resoloution.z);
        }

        public float V_AccessorAt(int i, int j, int k, Vector3 dataSize)
        {
            int index = k * (int)dataSize.y * (int)dataSize.x + j * (int)dataSize.x + i;
            return v[index];
        }

        public float[] V_Accessor()
        {
            return v;
        }

        public Vector3 DataOrigin_V()
        {
            return Origin + 0.5f * new Vector3(GridSpacing.x, 0, GridSpacing.z);
        }

        public Vector3 DataSize_V()
        {
            return new Vector3(Resoloution.x, Resoloution.y + 1, Resoloution.z);
        }

        public float W_AccessorAt(int i, int j, int k, Vector3 dataSize)
        {
            int index = k * (int)dataSize.y * (int)dataSize.x + j * (int)dataSize.x + i;
            return w[index];
        }

        public float[] W_Accessor()
        {
            return w;
        }

        public Vector3 DataOrigin_W()
        {
            return Origin + 0.5f * new Vector3(GridSpacing.x, GridSpacing.y, 0);
        }

        public Vector3 DataSize_W()
        {
            return new Vector3(Resoloution.x, Resoloution.y, Resoloution.z + 1);
        }

        float[] InitializeAxisData(float[] x, float initialValue, Vector3 dataSize)
        {
            x = new float[(int)dataSize.x * (int)dataSize.y * (int)dataSize.z];
            for (int i = 0; i < x.Length; i++)
            {
                x[i] = initialValue;
            }
            return x;
        }
    }
}
