using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class ObjectiveQueue : MonoBehaviour
    {

        public Queue<GameObject> Objectives;

        public bool ActivateNextObjective()
        {
            if (Objectives.Count == 0) { return false; }
            GameObject NewObjective = Objectives.Dequeue();
            NewObjective.SetActive(true);
            return true;
        }
    }
}
