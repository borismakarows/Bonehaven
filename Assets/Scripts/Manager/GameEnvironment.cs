using System.Collections.Generic;
using UnityEngine;

public sealed class GameEnvironment : MonoBehaviour
{
    private static GameEnvironment instance;
    // Listeyi burada initialize (new) ediyoruz, yoksa hata verir.
    private List<Transform> checkpoints = new List<Transform>();
    public List<Transform> Checkpoints { get { return checkpoints; } }

    public static GameEnvironment Singleton
    {
        get
        {
            if (instance == null)
            {
                // Sahne içinde bu script'in olduğu objeyi bulur
                instance = FindFirstObjectByType<GameEnvironment>();

                if (instance == null)
                {
                    // Eğer sahnede yoksa yeni bir GameObject oluşturup ekler
                    GameObject go = new GameObject("GameEnvironment");
                    instance = go.AddComponent<GameEnvironment>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Singleton kontrolü
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        instance = this;

        // "Checkpoint" tag'ine sahip objeleri listeye ekle
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Checkpoint");
        foreach (GameObject checkpoint in gos)
        {
            checkpoints.Add(checkpoint.transform);
        }
    }
}