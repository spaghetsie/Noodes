using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOrganism : MonoBehaviour
{
    public class Cell
    {
        public Rigidbody2D rb;
        public SpringJoint2D[] muscles;

        public Cell(string Type, int connections, Transform org, GameObject prefab){
            
            GameObject cell = Instantiate(prefab);
            cell.SetActive(true);
            cell.transform.parent = org;
            muscles = new SpringJoint2D[connections];
            rb = cell.GetComponent<Rigidbody2D>();
            for(int i=0; i< connections; i++){
                muscles[i] = cell.AddComponent<SpringJoint2D>();
                muscles[i].autoConfigureDistance = false;
                muscles[i].distance = 1;
            }
            
        }
    }

    public class Nnet
    {
        public float[][] nodes;
        float[][,] weights;

        public Nnet(int Inputs, int HLcount, int HLwidth, int Outputs){
            nodes = new float[HLcount+2][];
            weights = new float[HLcount+1][,];

            nodes[0] = new float[Inputs];
            weights[0] = new float[HLwidth, Inputs];

            for (int i = 0; i<HLcount; i++){
                nodes[i+1] = new float[HLwidth];
            }
            for (int i = 0; i<HLcount-1; i++){
                weights[i+2] = new float[HLwidth, HLwidth];
            }

            nodes[HLcount+1] = new float[Outputs];
            weights[HLcount] = new float[Outputs, HLwidth];
            }

        void UpdateNodes()
        {
            for(int l=1; l < nodes.GetLength(0); l++){
                for(int n=0; n<nodes[l].GetLength(0); n++){
                    nodes[l][n] = 0f;
                    for(int c=0; c<weights[l-1].GetLength(1); c++){
                        nodes[l][n] += weights[l-1][n, c] * nodes[l-1][c];
                    }
                }
            }
        }
        
        void MutateWeights(int chance=5, float range=0.05f){
            for(int l=1; l < nodes.GetLength(0); l++){
                for(int n=0; n<nodes[l].GetLength(0); n++){
                    for(int c=0; c<weights[l-1].GetLength(1); c++){
                        if(Random.Range(0, 101) <= chance)
                            weights[l-1][n, c] += Random.Range(-range, range);
                    }
                }
            }
        }
    }

    public class Org
    {
        Cell[] cells;
        int[,] connections;
        Nnet nnet;
        string name;
        public Org(int CellCount, int[][] Connections, Nnet Nnet, string Name){
            name = Name;
            GameObject org = new GameObject(name);

            cells = new Cell[CellCount];
            GameObject NodePrefab = GameObject.Find("prefabs").transform.GetChild(1).gameObject;
            for(int i=0;i<CellCount;i++){
                cells[i] = new Cell("basic", Connections[i].Length, org.transform, NodePrefab);
            }
            for(int o = 0; o<Connections.Length; o++){
                for(int c=0; c<Connections[o].Length; c++){
                    cells[o].muscles[c].connectedBody = cells[Connections[o][c]].rb;
                }
            }

            nnet = Nnet;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        int[][] con = new int[3][];
        con[0] = new int[]{};
        con[1] = new int[]{0, 2};
        con[2] = new int[]{};
        Org org = new Org(3, con, new Nnet(2, 2, 5, 5), "first");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
