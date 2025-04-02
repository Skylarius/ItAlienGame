using Codice.Client.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.FPS.Game
{
    public interface IObjectiveAction
    {
        public void ObjectiveEnabled();
    }

    [System.Serializable]
    public class SpawnObjectsInRange : IObjectiveAction
    {
        [Header("SpawnObjectsInRange")]
        /// <summary>
        /// Object to clone
        /// </summary>
        public GameObject Sample;

        /// <summary>
        /// Amount of objects to spawn
        /// </summary>
        public int Num = 5;

        /// <summary>
        /// Minimum X, Z
        /// </summary>
        public Vector2 BottomLeft = new Vector2(-45, 90);
        /// <summary>
        /// Maximum X, Z
        /// </summary>
        public Vector2 TopRight = new Vector2(-10, 30);
        public void ObjectiveEnabled()
        {
            for (int i = 0; i < Num; i++)
            {
                GameObject NewGameObject = GameObject.Instantiate(Sample);
                NewGameObject.name = Sample.name + "_Clone_" + i.ToString();
                float x = Random.Range(BottomLeft.x, TopRight.x);
                float y = Sample.transform.position.y;
                float z = Random.Range(TopRight.y, BottomLeft.y);
                NewGameObject.transform.position = new Vector3(x, y, z);
                NewGameObject.SetActive(true);
            }
        }
    }

    [System.Serializable]
    public class AcivateObjects : IObjectiveAction
    {
        [Header("ActivateObjects")]
        public List<GameObject> Objects;

        public void ObjectiveEnabled()
        {
            foreach (GameObject obj in Objects)
            { 
                obj.SetActive(true);
            }
        }
    }

    [System.Serializable]
    public class DisableObjects : IObjectiveAction
    {
        [Header("DisableObjects")]
        public List<GameObject> Objects;

        public void ObjectiveEnabled()
        {
            foreach (GameObject obj in Objects)
            {
                obj.SetActive(false);
            }
        }
    }

    [System.Serializable]
    public struct ObjectiveGameObjectsListWrapper
    {
        public GameObject Objective;

        [SerializeField, SerializeReference]
        public List<IObjectiveAction> Actions;
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
            ActivateObjective(Objectives.Dequeue());
        }

        private void OnValidate()
        {
            foreach (ObjectiveGameObjectsListWrapper Wrapper in ObjectivesList)
            {
                if (Wrapper.Actions.Count == 0)
                {
                    Wrapper.Actions.Add(new SpawnObjectsInRange());
                    Wrapper.Actions.Add(new AcivateObjects());
                    Wrapper.Actions.Add(new DisableObjects());
                    ObjectivesList.Add(Wrapper);
                }
                for (int i=0; i < Wrapper.Actions.Count; i++)
                {
                    if (Wrapper.Actions[i] == null || Wrapper.Actions[i].GetType() == typeof(IObjectiveAction))
                    {
                        switch(Random.Range(0,3))
                        {
                            case 0:
                                Wrapper.Actions[i] = new SpawnObjectsInRange();
                                break;
                            case 1:
                                Wrapper.Actions[i] = new AcivateObjects();
                                break;
                            case 2:
                                Wrapper.Actions[i] = new DisableObjects();
                                break;
                        }
                    }
                }
            }
        }

        public bool ActivateNextObjective()
        {
            if (Objectives.Count == 0) { return false; }
            ObjectiveGameObjectsListWrapper NewObjective = Objectives.Dequeue();
            ActivateObjective(NewObjective);
            return true;
        }

        private void ActivateObjective(ObjectiveGameObjectsListWrapper newObjective)
        {
            newObjective.Objective.SetActive(true);
            foreach (IObjectiveAction Action in newObjective.Actions)
            {
                Action.ObjectiveEnabled();
            }
        }
    }
}
