using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridDataStructure;

namespace GridSystemBuilder
{
    public abstract class ScalarGridBuilder
    {
        public abstract ScalarGrid build(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox);
    }

    public abstract class VectorGridBuilder
    {
        public abstract VectorGrid build(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox);
    }

    public class CellCenteredScalarGridBuilder : ScalarGridBuilder
    {
        public override ScalarGrid build(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox)
        {
            return new CellCenterScalarGrid(gridSpacing, resoloution, origin, boundingBox);
        }   
    }

    public class VertexCenteredScalarGridBuilder : ScalarGridBuilder
    {
        public override ScalarGrid build(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox)
        {
            return new VertexCenterScalarGrid(gridSpacing, resoloution, origin, boundingBox);
        }
    }

    public class CollocatedVectorGridBuilder : VectorGridBuilder
    {
        public override VectorGrid build(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CellCenteredCollocatedVectorGridBuilder : CollocatedVectorGridBuilder
    {
        public override VectorGrid build(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox)
        {
            return new CellCenteredCollocatedVector(gridSpacing, resoloution, origin, boundingBox);
        }
    }

    public class VertexCenteredCollocatedVectorGridBuilder : CollocatedVectorGridBuilder
    {
        public override VectorGrid build(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox)
        {
            return new VertexCenteredCollocatedVector(gridSpacing, resoloution, origin, boundingBox);
        }
    }

    public class FaceCenteredVectorGridBuilder : VectorGridBuilder
    {
        public override VectorGrid build(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox)
        {
            return new FaceCenteredGrid(gridSpacing, resoloution, origin, boundingBox);
        }
    }
}
