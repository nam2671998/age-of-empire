using UnityEngine;

public partial class UnitHarvesterController
{
    private sealed class UnitHarvesterSearchingState : IUnitHarvesterControllerState
    {
        private readonly Collider[] overlapResults = new Collider[128];

        void IUnitHarvesterControllerState.Enter(UnitHarvesterController controller)
        {
        }

        void IUnitHarvesterControllerState.Tick(UnitHarvesterController controller)
        {
            var next = FindNext(controller);
            if (next == null)
            {
                controller.movement?.StopMovement();
                controller.StopHarvest();
                return;
            }

            controller.SetCurrentTarget(next);
            controller.SetState(movingToTargetState);
        }

        void IUnitHarvesterControllerState.Exit(UnitHarvesterController controller)
        {
        }

        private IHarvestable FindNext(UnitHarvesterController controller)
        {
            if (controller == null)
                return null;

            int hitCount = Physics.OverlapSphereNonAlloc(controller.transform.position, controller.findNextRadius,
                overlapResults, controller.harvestableResourceMask);

            if (hitCount == 0)
                return null;

            IHarvestable best = null;
            float bestSqrDistance = float.PositiveInfinity;
            var currentGo = controller.currentTarget != null ? controller.currentTarget.GetGameObject() : null;

            for (int i = 0; i < hitCount; i++)
            {
                var col = overlapResults[i];
                if (col == null)
                    continue;

                if (!col.TryGetComponent<IHarvestable>(out var candidate))
                    continue;

                if (candidate == null)
                    continue;

                var candidateGo = candidate.GetGameObject();
                if (candidateGo == null || (currentGo != null && candidateGo == currentGo) || !candidateGo.activeInHierarchy)
                    continue;

                if (candidate.IsDepleted())
                    continue;

                if (candidate.GetResourceType() != controller.currentResourceType)
                    continue;

                Vector3 candidatePos = candidate.GetHarvestPosition();
                float sqr = (candidatePos - controller.transform.position).sqrMagnitude;
                if (sqr < bestSqrDistance)
                {
                    bestSqrDistance = sqr;
                    best = candidate;
                }
            }

            return best;
        }
    }
}
