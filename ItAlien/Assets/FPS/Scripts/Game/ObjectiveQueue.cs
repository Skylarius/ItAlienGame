using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    [System.Serializable]
    public struct ObjectiveGameObjectsListWrapper
    {
        public GameObject Objective;

        public List<GameObject> Objects;
    }

    public class ObjectiveQueue : MonoBehaviour
    {

        public List<ObjectiveGameObjectsListWrapper> ObjectivesList;


        Queue<ObjectiveGameObjectsListWrapper> Objectives;

        private void Start()
        {
            Objectives = new Queue<ObjectiveGameObjectsListWrapper>();
            foreach (ObjectiveGameObjectsListWrapper o in ObjectivesList)
            {
                Objectives.Enqueue(o);
            }
        }

        public bool ActivateNextObjective()
        {
            if (Objectives.Count == 0) { return false; }
            ObjectiveGameObjectsListWrapper NewObjective = Objectives.Dequeue();
            NewObjective.Objective.SetActive(true);
            foreach (GameObject go in NewObjective.Objects)
            {
                go.SetActive(true);
            }
            return true;
        }
    }
}
