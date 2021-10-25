using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PredictionManager : MonoBehaviour
{
    public static PredictionManager Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = FindObjectOfType<PredictionManager>();
            return _Instance;
        }
    }
    private static PredictionManager _Instance;

    public PhysicsScene predictionPhysicScene;
    public Scene predictionScene;
    public PhysicsScene currentPhysicScene;
    public GameObject ground;

    private GameObject _dummy;
    private GameObject _groundDummy;

    void Start()
    {
        Physics.autoSimulation = false;
        currentPhysicScene = SceneManager.GetActiveScene().GetPhysicsScene();

        CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics3D);
        predictionScene = SceneManager.CreateScene("Prediction", csp);
        predictionPhysicScene = predictionScene.GetPhysicsScene();
    }

    
    public Vector3 GetPrediction(Rigidbody obj, Vector3 force, float sec)
    {
        if (currentPhysicScene.IsValid() && predictionPhysicScene.IsValid())
        {
            if (_dummy == null)
            {
                _dummy = Instantiate(obj.gameObject);
                _groundDummy = Instantiate(ground);
                Destroy(_dummy.GetComponent<Player>());
                foreach(Transform child in _dummy.transform)
                {
                    Destroy(child.gameObject);
                }
                SceneManager.MoveGameObjectToScene(_dummy, predictionScene);
                SceneManager.MoveGameObjectToScene(_groundDummy, predictionScene);
            }

            _dummy.transform.position = obj.gameObject.transform.position;
            _dummy.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);


            while (sec > 0)
            {
                predictionPhysicScene.Simulate(Time.fixedDeltaTime);
                sec -= Time.fixedDeltaTime;
            }

            Vector3 result = _dummy.transform.position;
            Destroy(_dummy);
            return result;
        }
        else
        {
           return Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        if (currentPhysicScene.IsValid() == true)
            currentPhysicScene.Simulate(Time.fixedDeltaTime);
    }

}
