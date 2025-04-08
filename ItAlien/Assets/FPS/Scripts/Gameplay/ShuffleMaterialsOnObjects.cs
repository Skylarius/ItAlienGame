using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleMaterialsOnObjects : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject[] Objects;
    void Start()
    {
        List<Material> Materials = new List<Material>();
        foreach (GameObject obj in Objects) 
        {
            Materials.Add(obj.GetComponent<Renderer>().material);
        }
        foreach (GameObject obj in Objects)
        {
            int MatIndex = Random.Range(0, Materials.Count);
            obj.GetComponent<Renderer>().material = Materials[MatIndex];
            Materials.RemoveAt(MatIndex);
        }
    }
}
