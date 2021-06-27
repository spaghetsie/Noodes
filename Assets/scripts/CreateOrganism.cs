using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CreateOrganism : MonoBehaviour
{
    public class Muscle
    {
        public int id;
        public SpringJoint2D muscle;

        public Muscle(int Id){
            this.id = Id;
        }
    }

    public class Cell
    {
        public int id;
        public Rigidbody2D rb;
        public Org body;
        public Muscle[] muscles;

        public Cell(Org Body, int outputCount, int muscleCount, int connections, Transform org, GameObject prefab){
            this.id = outputCount;
            this.muscles = new Muscle[connections];
            this.body = Body;
            GameObject cell = Instantiate(prefab);
            Vector2 spawnPos = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
            spawnPos.x += org.position.x;
            spawnPos.y += org.position.y;
            cell.transform.position = spawnPos;
            cell.SetActive(true);
            cell.transform.parent = org;


            this.rb = cell.GetComponent<Rigidbody2D>();
            for(int i=0; i<connections; i++){
                Body.muscles[muscleCount+i] = new Muscle(outputCount+i+1);
                this.muscles[i] = Body.muscles[muscleCount+i];
                Body.muscles[muscleCount+i].muscle = cell.AddComponent<SpringJoint2D>();
                Body.muscles[muscleCount+i].muscle.autoConfigureDistance = false;
                Body.muscles[muscleCount+i].muscle.distance = 5;
                Body.muscles[muscleCount+i].muscle.enableCollision = true;
            }
            
        }
    }

    public class Nnet
    {
        public float[][] nodes;
        public float[][,] weights;

        public Nnet(int Inputs, int HLcount, int HLwidth, int Outputs){
            this.nodes = new float[HLcount+2][];
            this.weights = new float[HLcount+1][,];

            this.nodes[0] = new float[Inputs];
            this.weights[0] = new float[HLwidth, Inputs+1];

            for (int i = 0; i<HLcount; i++){
                this.nodes[i+1] = new float[HLwidth];
            }
            for (int i = 0; i<HLcount-1; i++){
                this.weights[i+2] = new float[HLwidth, HLwidth+1];
            }

            this.nodes[HLcount+1] = new float[Outputs];
            this.weights[HLcount] = new float[Outputs, HLwidth+1];
        }

        public Nnet(Nnet nnet){
            this.nodes = nnet.nodes;
            this.weights = nnet.weights;
        }

        public void UpdateNodes()
        {
            for(int l=1; l < this.nodes.GetLength(0); l++){
                for(int n=0; n<this.nodes[l].GetLength(0); n++){
                    this.nodes[l][n] = 0f;
                    for(int c=0; c<this.weights[l-1].GetLength(1)-1; c++){
                        this.nodes[l][n] += this.weights[l-1][n, c] * this.nodes[l-1][c];
                    }
                    int lastIndex = weights[l-1].GetLength(1)-1;
                    this.nodes[l][n] = (float)System.Math.Tanh(this.nodes[l][n]);
                    this.nodes[l][n] += this.weights[l-1][n, lastIndex];
                }
            }
        }
        
        public void MutateWeights(int chance=5, float range=0.05f){
            for(int l=1; l < this.nodes.GetLength(0); l++){
                for(int n=0; n<this.nodes[l].GetLength(0); n++){
                    for(int c=0; c<this.weights[l-1].GetLength(1); c++){
                        if(Random.Range(0, 101) <= chance)
                            this.weights[l-1][n, c] += Random.Range(-range, range);
                    }
                }
            }
        }
    }

    public class Org
    {
        public Cell[] cells;
        public Muscle[] muscles;
        int[,] connections;
        public Nnet nnet;
        string name;
        public GameObject body;
        int outputs = 0;
        int Mcount = 0;
        public Org(int CellCount, int[][] Connections, Nnet Nnet, string Name){
            this.name = Name;
            this.body = new GameObject(name);
            int muscleCount = 0;
            for(int i = 0; i<Connections.Length; i++){
                muscleCount += Connections[i].Length;
            }
            this.muscles = new Muscle[muscleCount];

            this.cells = new Cell[CellCount];
            GameObject NodePrefab = GameObject.Find("prefabs").transform.GetChild(1).gameObject;
            for(int i=0;i<CellCount;i++){
                this.cells[i] = new Cell(this, this.outputs, this.Mcount, Connections[i].Length, this.body.transform, NodePrefab);
                this.outputs += Connections[i].Length+1;
                this.Mcount += Connections[i].Length;
            }
            if(this.outputs != this.cells.Length+muscles.Length)
                Debug.Log("something fucked up a lot");
            for(int o = 0; o<Connections.Length; o++){
                for(int c = 0; c< Connections[o].Length; c++)
                    {
                        this.cells[o].muscles[c].muscle.connectedBody = this.GetCell(Connections[o][c]).rb;
                    }
                    
            }

            this.nnet = Nnet;
        }
        
        public Cell GetCell(int Id)
        {
            Cell cell = null;
            for(int i = 0; i<this.cells.Length; i++){
                if(this.cells[i].id == Id)
                    cell = this.cells[i];
            }
            return cell;
        }

        public Muscle GetMuscle(int Id)
        {
            Muscle muscle = null;
            for(int i = 0; i<this.muscles.Length; i++){
                if(this.muscles[i].id == Id)
                    muscle = this.muscles[i];
            }
            return muscle;
        }

        void UpdateOutput(int Id, float value)
        {
            Cell cell = GetCell(Id);
            Muscle muscle = null;

            if(value>3)
                value = 3;
            else if(value<-3)
                value = 1;

            if(cell == null)
                muscle = GetMuscle(Id);
                
            if(cell != null)
                cell.rb.drag = value*8;
            else if(muscle != null)
                muscle.muscle.distance = value;
            else
                Debug.Log("Didnt find any output... fuck");
        }

        void WakeUp(){
            foreach(Cell cell in cells){
                cell.rb.WakeUp();
            }
        }

        void UpdateInputs(){
            int Icount = this.nnet.nodes[0].Length;
            for(int i = 0; i<Icount-1; i++){

            }
            this.nnet.nodes[0][Icount-1] = Mathf.Sin(Time.time);
        }

        void UpdateOutputs(){
            for(int i = 0; i<this.outputs; i++){
                float outputValue = this.nnet.nodes[this.nnet.nodes.Length-1][i]*3;
                UpdateOutput(i, outputValue);
            }
        }

        public void Update(){
            WakeUp();
            UpdateInputs();
            this.nnet.UpdateNodes();
            UpdateOutputs();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        int[][] con = new int[2][];
        con[0] = new int[]{2};
        con[1] = new int[]{};
        Org org = new Org(2, con, new Nnet(1, 1, 1, 3), "first");
        org.nnet.MutateWeights(5, 0.05f);
        org.body.AddComponent<OrgController>();
        org.body.SendMessage("GetOrg", org);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
