using UnityEngine;

public class QATester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        QATestingSystem.RunAllTests();
    }

}
