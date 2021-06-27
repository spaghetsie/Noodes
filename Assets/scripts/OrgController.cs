using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrgController : MonoBehaviour
{
    
    CreateOrganism.Org me;
    void GetOrg(CreateOrganism.Org org){
        me = org;
        me.nnet.weights[0][0, 0] = 1f;
        me.nnet.weights[1][0, 0] = -10f;
        me.nnet.weights[1][1, 0] = 3f;
        me.nnet.weights[1][2, 0] = 10f;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        me.Update();
    }
}
