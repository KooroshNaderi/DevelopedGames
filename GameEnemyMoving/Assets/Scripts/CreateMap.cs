using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class CreateMap : MonoBehaviour
{
    public enum myDividerType { X, Y };
    public enum GridType { Wall = 1, Free = 0};
    public class myDividedArea
    {
        public myDividedArea(Vector2 iFirstBorder, Vector2 iEndBorder, int iNumberOfRandSamples)
        {
            m_divider_type = myDividerType.X;
            mFirstBorder = iFirstBorder;
            mEndBorder = iEndBorder;
            mLinkToFather = null;
            //mLeftChild = null;
            //mRightChild = null;
            numberOfRandSamples = iNumberOfRandSamples;
        }

        public myDividedArea(Vector2 iFirstBorder, Vector2 iEndBorder, myDividedArea iLinkToFather, myDividerType iNextType, int iNumberOfRandSamples)
        {
            m_divider_type = iNextType;
            mFirstBorder = iFirstBorder;
            mEndBorder = iEndBorder;
            mLinkToFather = iLinkToFather;
            //mLeftChild = null;
            //mRightChild = null;
            numberOfRandSamples = iNumberOfRandSamples;
        }

        public int getMapVal(ArrayList iMap, int i, int j)
        {
            if (i >= iMap.Count || j >= ((ArrayList) iMap[0]).Count)
                return -1;
            ArrayList col_i = (ArrayList)iMap[i]; //each index_i in the map returns a column on the X direction
            return (int) col_i[j];
        }

        public void setMapVal(ArrayList iMap, int i, int j, int iVal)
        {
            if (i >= iMap.Count || j >= ((ArrayList) iMap[0]).Count)
                return;
            ArrayList col_i = (ArrayList)iMap[i]; //each index_i in the map returns a column on the X direction
            col_i[j] = iVal;
        }

        public void myMakeWall(ArrayList iMap, Vector2 iPoint, myDividerType iTypeOfWall, ArrayList iEmptyPoints, Vector2 iFirstBoundary, Vector2 iEndBoundary)
        {
            bool flag_continue = true;
            int w_counter = 0;//wall counter
            int m_start_point = 0;
            int m_end_point = 0;
            int index_i_map = 0;
            int index_j_map = 0;
            while (flag_continue)
            {
                if (iTypeOfWall == myDividerType.X)//wall on X
                {
                    m_start_point = (int) iFirstBoundary[1];
                    m_end_point = (int) iEndBoundary[1];
                    index_i_map = (int) iPoint[0];
                    index_j_map = m_start_point + w_counter;
                }
                else //wall on Y
                {
                    m_start_point = (int) iFirstBoundary[0];
                    m_end_point = (int) iEndBoundary[0];
                    index_i_map = m_start_point + w_counter;
                    index_j_map = (int) iPoint[1];
                }
                if (m_start_point + w_counter > m_end_point)
                {
                    flag_continue = false;
                }
                else
                {
                    setMapVal(iMap, index_i_map, index_j_map, (int) GridType.Wall);
                }
                w_counter++;
            }

            if (iEmptyPoints != null)
            {
                for (int i = 0; i < iEmptyPoints.Count; i++)
                {
                    Vector2 myEmptyPoint = (Vector2)iEmptyPoints[i];
                    if (myCheckOneWayPath(iMap, myEmptyPoint))
                    {
                        setMapVal(iMap, (int) myEmptyPoint[0], (int) myEmptyPoint[1], (int) GridType.Free);
                    }
                }
            }

            return;
        }

        private bool myCheckOneWayPath(ArrayList iMap, Vector2 iEmptyPoint)
        {
            int index_i_map = (int)iEmptyPoint[0];
            int index_j_map = (int)iEmptyPoint[1];

            if (getMapVal(iMap, index_i_map, index_j_map - 1) == 0 && getMapVal(iMap, index_i_map - 1, index_j_map - 1) == 0
                && getMapVal(iMap, index_i_map - 1, index_j_map) == 0)//i,j-1|i-1,j-1|i-1,j
            {
                return false;
            }

            if (getMapVal(iMap, index_i_map, index_j_map - 1) == 0 && getMapVal(iMap, index_i_map + 1, index_j_map - 1) == 0
                && getMapVal(iMap, index_i_map + 1, index_j_map) == 0)//i,j-1|i+1,j-1|i+1,j
            {
                return false;
            }

            if (getMapVal(iMap, index_i_map, index_j_map + 1) == 0 && getMapVal(iMap, index_i_map - 1, index_j_map + 1) == 0
                && getMapVal(iMap, index_i_map - 1, index_j_map) == 0)//i,j+1|i-1,j+1|i-1,j
            {
                return false;
            }

            if (getMapVal(iMap, index_i_map, index_j_map + 1) == 0 && getMapVal(iMap, index_i_map + 1, index_j_map + 1) == 0
                && getMapVal(iMap, index_i_map + 1, index_j_map) == 0)//i,j+1|i+1,j+1|i+1,j
            {
                return false;
            }
            return true;
        }

        public void myDivideAreaFunc(ArrayList iMap, myDividedArea iSubArea)
        {
            int started_empty_poses = 0;
            int end_empty_poses = 0;
            if (iSubArea.mFirstBorder[0] == iSubArea.mEndBorder[0] || iSubArea.mFirstBorder[1] == iSubArea.mEndBorder[1])
                return;
            ArrayList setEmptyPoses = new ArrayList();
            int x_sample = 0;
            int y_sample = 0;
            myDividerType next_type;
            Vector2 left_child_boundary_start;
            Vector2 left_child_boundary_end;
            Vector2 right_child_boundary_start;
            Vector2 right_child_boundary_end;
            if (iSubArea.m_divider_type == myDividerType.X)//take the first sample to divide X, other samples to place holes on X walls
            {
                x_sample = UnityEngine.Random.Range((int) iSubArea.mFirstBorder[0] + 1, (int) iSubArea.mEndBorder[0] - 1 + 1);
                //for placing holes in the wall
                started_empty_poses = (int) iSubArea.mFirstBorder[1];
                end_empty_poses = (int) iSubArea.mEndBorder[1];

                if (getMapVal(iMap, x_sample, started_empty_poses - 1) == 0)
                {
                    setEmptyPoses.Add(new Vector2(x_sample, started_empty_poses));
                }
                if (getMapVal(iMap, x_sample, end_empty_poses + 1) == 0)
                {
                    setEmptyPoses.Add(new Vector2(x_sample, end_empty_poses));
                }
                y_sample = started_empty_poses;
                for (int j = 0; j < numberOfRandSamples; j++)
                {
                    y_sample = UnityEngine.Random.Range(started_empty_poses, end_empty_poses + 1);
                    setEmptyPoses.Add(new Vector2(x_sample, y_sample));
                }
                next_type = myDividerType.Y;

                left_child_boundary_start = new Vector2(iSubArea.mFirstBorder[0], iSubArea.mFirstBorder[1]);
                if (x_sample - 1 >= iSubArea.mFirstBorder[0])
                    left_child_boundary_end = new Vector2(x_sample - 1, iSubArea.mEndBorder[1]);
                else
                    left_child_boundary_end = new Vector2(iSubArea.mFirstBorder[0], iSubArea.mEndBorder[1]);
                if (x_sample + 1 <= iSubArea.mEndBorder[0])
                    right_child_boundary_start = new Vector2(x_sample + 1, iSubArea.mFirstBorder[1]);
                else
                    right_child_boundary_start = new Vector2(iSubArea.mEndBorder[0], iSubArea.mFirstBorder[1]);
                right_child_boundary_end = new Vector2(iSubArea.mEndBorder[0], iSubArea.mEndBorder[1]);
            }
            else
            {
                y_sample = UnityEngine.Random.Range((int) iSubArea.mFirstBorder[1] + 1, (int) iSubArea.mEndBorder[1] - 1 + 1);
                //for placing holes in the wall
                started_empty_poses = (int) iSubArea.mFirstBorder[0];
                end_empty_poses = (int) iSubArea.mEndBorder[0];

                if (getMapVal(iMap, started_empty_poses - 1, y_sample) == 0)
                {
                    setEmptyPoses.Add(new Vector2(started_empty_poses, y_sample));
                }
                if (getMapVal(iMap, end_empty_poses + 1, y_sample) == 0)
                {
                    setEmptyPoses.Add(new Vector2(end_empty_poses, y_sample));
                }
                x_sample = started_empty_poses;
                for (int j = 0; j < numberOfRandSamples; j++)
                {
                    x_sample = UnityEngine.Random.Range(started_empty_poses, end_empty_poses + 1);
                    setEmptyPoses.Add(new Vector2(x_sample, y_sample));
                }
                next_type = myDividerType.X;

                left_child_boundary_start = new Vector2(iSubArea.mFirstBorder[0], iSubArea.mFirstBorder[1]);
                if (y_sample - 1 >= iSubArea.mFirstBorder[1])
                    left_child_boundary_end = new Vector2(iSubArea.mEndBorder[0], y_sample - 1);
                else
                    left_child_boundary_end = new Vector2(iSubArea.mEndBorder[0], iSubArea.mFirstBorder[1]);
                if (y_sample + 1 <= iSubArea.mEndBorder[1])
                    right_child_boundary_start = new Vector2(iSubArea.mFirstBorder[0], y_sample + 1);
                else
                    right_child_boundary_start = new Vector2(iSubArea.mFirstBorder[0], iSubArea.mEndBorder[1]);
                right_child_boundary_end = new Vector2(iSubArea.mEndBorder[0], iSubArea.mEndBorder[1]);
            }

            myMakeWall(iMap, new Vector2(x_sample, y_sample), iSubArea.m_divider_type, setEmptyPoses, iSubArea.mFirstBorder, iSubArea.mEndBorder);

            myDividedArea left_divided_area = new myDividedArea(left_child_boundary_start, left_child_boundary_end, this, next_type, numberOfRandSamples);
            myDividedArea right_divided_area = new myDividedArea(right_child_boundary_start, right_child_boundary_end, this, next_type, numberOfRandSamples);

            myDivideAreaFunc(iMap, left_divided_area);
            myDivideAreaFunc(iMap, right_divided_area);
            return;
        }
        
        int numberOfRandSamples;
        public myDividedArea mLinkToFather;
        myDividerType m_divider_type;
        public Vector2 mFirstBorder;
        public Vector2 mEndBorder;
    }

    //public GameObject[] EnemyInfo;
    //ArrayList myEnemies;
    //public int EnemyNumbers = 5;
    //public int numberOfFollowingEnemies = 0;
    ////Add a selection of enemies randomized
    //void initialization() /////////////////////////////////////////////////////////////////////Do everything related to the level here: initialization for level
    //{
    //    int mLevel = Application.loadedLevel;
    //    myEnemies = new ArrayList();
    //    for (int i = 0; i < EnemyNumbers; i++)
    //    {
    //        int random = UnityEngine.Random.Range(0, EnemyInfo.Length);
    //        GameObject newEnemy = Instantiate(EnemyInfo[random]) as GameObject;
    //        newEnemy.name = "Enemy" + i.ToString();
    //        EnemyScript mEnemy_i_script = newEnemy.GetComponent<EnemyScript>();
    //        mEnemy_i_script.setEnemyType(EnemyScript.myEnemyType.NotFollowingPlayer);
    //        mEnemy_i_script.Enemy_ith_Place = i;
    //        if (numberOfFollowingEnemies > 0)
    //        {
    //            newEnemy.GetComponent<EnemyScript>().setEnemyType(EnemyScript.myEnemyType.FollowingPlayer);
    //            numberOfFollowingEnemies--;
    //        }
    //        myEnemies.Add(newEnemy);
    //    }
    //}

    // Use this for initialization
    void Start()
    {
        //initialization();
        //Automatically set N, M and Plane
 //       N = GetComponent<Playground>().width;
 //       M = GetComponent<Playground>().height;
        

        myAreaDivider = new myDividedArea(new Vector2(1, 1), new Vector2(N - 2, M - 2), numberOfRandSamples);
        initializing_Map(N, M);
        myAreaDivider.myDivideAreaFunc(myMap, myAreaDivider);
        myWriteMap();
        myGameObjects = new ArrayList();

        Transform mPlane_Transform = this.transform;
        Vector3 plane_3d_size = 10 * mPlane_Transform.localScale;
        grid_size = new Vector3(plane_3d_size[0] / (float)N, 0.5f, plane_3d_size[2] / (float)M);
        first_cube_location = mPlane_Transform.localPosition - new Vector3(plane_3d_size[0] / 2, 0f, plane_3d_size[2] / 2)
            + new Vector3(plane_3d_size[0] / (2 * N), 0f, plane_3d_size[2] / (2 * M));
        myCreateScene(grid_size, first_cube_location);
    }

    // Update is called once per frame
    void Update()
    {

    }

 //   public ArrayList getEnemies() { return myEnemies; }
    
    public Vector3 getGridSize()
    {
        return grid_size;
    }

    public Vector3 getFirstLocationOfCube()
    {
        return first_cube_location;
    }

    public ArrayList GetMyMap()
    {
        return myMap;
    }

    public int numberOfRandSamples = 200;//number of random zeros inside the map (we want this much zeros but size all zeros are not acceptable we might not get as much zeros as we want)
    Vector3 grid_size;
    Vector3 first_cube_location;
    GameObject plane;
    public int N = 50;//create a (N-2)*(N-2) map with boarder
    public int M = 50;
    ArrayList myMap;
    myDividedArea myAreaDivider;
    ArrayList myGameObjects;
    int numberOfDestroyableCubes = 0;

    void initializing_Map(int iN, int iM)
    {
        myMap = new ArrayList(iN);
        for (int i = 0; i < iN; i++)
        {
            myMap.Add(new ArrayList(iM));
            for (int j = 0; j < iM; j++)
            {
                ((ArrayList) myMap[i]).Add(0);
            }
        }
        Vector2 FirstWallBoundary = new Vector2(0f, 0f);
        Vector2 EndWallBoundary = new Vector2((float)(N - 1), (float)(M - 1));
        myAreaDivider.myMakeWall(myMap, FirstWallBoundary, myDividerType.X, null, FirstWallBoundary, EndWallBoundary);//wall on X = 0
        myAreaDivider.myMakeWall(myMap, FirstWallBoundary, myDividerType.Y, null, FirstWallBoundary, EndWallBoundary);//wall on Y = 0
        myAreaDivider.myMakeWall(myMap, EndWallBoundary, myDividerType.X, null, FirstWallBoundary, EndWallBoundary);//wall on X = N-1
        myAreaDivider.myMakeWall(myMap, EndWallBoundary, myDividerType.Y, null, FirstWallBoundary, EndWallBoundary);//wall on Y = N-1
    }

    String myFileGenerator()
    {
        //StreamReader m_Reader;
        String mFile_path = "";
        //String directory_name = "MapsBomberMan\\";
        //int num_file = 0;
        //if (!Directory.Exists(directory_name))
        //    Directory.CreateDirectory(directory_name);
        //if (File.Exists(directory_name + "number_file.txt"))
        //{
        //    m_Reader = new StreamReader(directory_name + "number_file.txt");
        //    String str_num = m_Reader.ReadLine();
        //    num_file = ((int) str_num[0] - 48) + 1;
        //    mFile_path = directory_name + "Map" + str_num + ".txt";
        //    m_Reader.Close();
        //}
        //else
        //{
        //    mFile_path = directory_name + "Map.txt";
        //}
        //StreamWriter m_writer_file = new StreamWriter(directory_name + "number_file.txt");
        //m_writer_file.WriteLine(num_file);
        //m_writer_file.Flush();
        //m_writer_file.Close();
        return mFile_path;
    }

    void myWriteMap()
    {
        //String str_file_name = myFileGenerator();
        //StreamWriter m_writer = new StreamWriter(str_file_name, true);
        //for (int j = 0; j < M; j++)
        //{
        //    String myWriteString = "";
        //    for (int i = 0; i < N; i++)
        //    {
        //        myWriteString += myAreaDivider.getMapVal(myMap, i, j).ToString() + " ";
        //    }
        //    m_writer.WriteLine(myWriteString);
        //}
        //m_writer.Flush();
        //m_writer.Close();
    }

    void myCreateScene(Vector3 iGridSize, Vector3 iLocation)
    {
        //Spawn Indestructible Walls
        Vector3 d_pos = new Vector3(iLocation[0], iLocation[1] + iGridSize[1] / 2, iLocation[2]);
        for (int j = 0; j < M; j++)
        {
            for (int i = 0; i < N; i++)
            {
                if (myAreaDivider.getMapVal(myMap, i, j) == (int) GridType.Wall)
                {
                    GameObject cube_ij = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube_ij.transform.position = new Vector3(i * iGridSize[0], iGridSize[1]/8, j * iGridSize[2]) + d_pos;//x <- X(i), y <- Y(j)
                    cube_ij.transform.localScale = iGridSize;
                    //cube_ij.isStatic = true;
                    //NavMeshObstacle mcube_obstacle = cube_ij.AddComponent<NavMeshObstacle>();
                    //mcube_obstacle.size = iGridSize / 5 + new Vector3(0.05f, 0.05f, 0.05f);
                    //mcube_obstacle.carving = true;

                    //Sort all the cube into appropriate parent
                    if (!GameObject.Find("Wall"))
                    {
                        GameObject newParent = new GameObject("Wall");
                        cube_ij.transform.SetParent(newParent.transform);
                    }
                    else
                    {
                        cube_ij.transform.SetParent(GameObject.Find("Wall").transform);
                    }

                    //Add tag "IndestructibleWall" to the cube and add IndestructibleWall class
                   // cube_ij.tag = "Wall";
                    myGameObjects.Add(cube_ij);
                }
            }
        }
    }

//    public int getNumberOfEnemies() { return EnemyNumbers; }
}
