using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour
{
    public class mySortClass : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            RaycastHit mX = (RaycastHit)x;
            RaycastHit mY = (RaycastHit)y;
            if (mY.distance > mX.distance)
                return -1;
            else
                return 1;
        }
    }

    public class AStarNode
    {
        public AStarNode(Vector3 iS, Vector3 iE, AStarNode iParent)
        {
            float cost_value = 2.0f;
            if (iParent != null)
            {
                m_F = (iS - iParent.m_point).magnitude + iParent.m_F;
            }
            else
            {
                m_F = 0;
            }
            m_H = cost_value * (iE - iS).magnitude;
            m_point = iS;
            m_parent = iParent;
        }

        public int mFindNodeByPoint(ArrayList iList, Vector3 iPoint)
        {
            int index_ret_node = -1;
            bool flag_continue = true;
            for (int i = 0; i < iList.Count && flag_continue; i++)
            {
                AStarNode node_i = (AStarNode)iList[i];
                if ((node_i.m_point - iPoint).magnitude == 0)
                {
                    flag_continue = false;
                    index_ret_node = i;
                }
            }
            return index_ret_node;
        }

        public int mFindNodeByMinVal(ArrayList iList)
        {
            int index_ret_node = -1;
            float min_dis = float.MaxValue;
            for (int i = 0; i < iList.Count; i++)
            {
                AStarNode node_i = (AStarNode)iList[i];
                float val_i = node_i.m_F + node_i.m_H;
                if (val_i < min_dis)
                {
                    min_dis = val_i;
                    index_ret_node = i;
                }
            }
            return index_ret_node;
        }

        public Vector3 m_point;
        public float m_F;
        public float m_H;
        public AStarNode m_parent;
    }

    public enum myEnemyType { FollowingPlayer = 0, NotFollowingPlayer = 1 };

    public Vector3 TragetPos;
    public int Enemy_ith_Place = 0;
    public float EnemySpeed = 0.1f;
    public myEnemyType EnemyType = myEnemyType.FollowingPlayer;

    public void setEnemyType(myEnemyType iEnemyType) { EnemyType = iEnemyType; }

    bool updated_env = false;

    CreateMap myMapInfo;
    ArrayList myMap;
    Vector3 myLocationOfFirstCube;
    Vector3 myGridSize;

    ArrayList myUpdatedPath;
    bool flag_update_path = true;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!updated_env)
        {
            myMapInfo = FindObjectOfType<CreateMap>();
            myGridSize = myMapInfo.getGridSize();
            myLocationOfFirstCube = myMapInfo.getFirstLocationOfCube();
            myMap = myMakeCopyOf(myMapInfo.GetMyMap());
            int rand_col = UnityEngine.Random.Range((int)0, (int)2);
            this.transform.localScale = new Vector3(myGridSize[0]/2, this.transform.localScale[1], myGridSize[2]/2);
            transform.position = myFindFirstRandomPlace(myMap, myLocationOfFirstCube, myGridSize, rand_col) + new Vector3(0f, transform.position[1], 0f);
            setTargetPos(transform.position - new Vector3(0f, this.transform.position[1], 0f), false);
            updated_env = true;

        }

        if (EnemyType == myEnemyType.NotFollowingPlayer)
        {
            float threshold = Mathf.Abs(EnemySpeed);
            bool flag_find_new_target = false;
            if ((this.transform.position - new Vector3(0.0f, transform.position[1], 0.0f) - TragetPos).magnitude < threshold)
            {
                flag_find_new_target = true;
            }
            else if (myUpdatedPath != null)
            {
                if (myUpdatedPath.Count == 0)
                {
                    flag_find_new_target = true;
                }
            }
            if (flag_find_new_target)
            {
                int rand_pos = UnityEngine.Random.Range((int)0, (int)2);
                if (rand_pos == 0)
                    TragetPos = myFindRandomPlace(myMap, myLocationOfFirstCube, myGridSize, true) + new Vector3(0f, transform.position[1], 0f);
                else
                    TragetPos = myFindRandomPlace(myMap, myLocationOfFirstCube, myGridSize, false) + new Vector3(0f, transform.position[1], 0f);
                flag_update_path = true;
            }
        }

        if (flag_update_path)
        {
            if (EnemyType == myEnemyType.FollowingPlayer)
            {
            }
            else
            {
                setTargetPos(TragetPos, false);
            }
            Vector3 pos1 = mPosToMapIndex(transform.position, myLocationOfFirstCube, myGridSize);
            pos1[0] = Mathf.Round(pos1[0]);
            pos1[2] = Mathf.Round(pos1[2]);
            Vector3 pos2 = TragetPos;
            myUpdatedPath = mFindPath(pos1, pos2, myUpdatedPath);
            flag_update_path = false;
        }

        mMoveOnThePath(myUpdatedPath, myLocationOfFirstCube, myGridSize);
    }

    bool DoesRaycastHitObject(Vector3 originPos)
    {
        if (originPos[1] == 0)
            originPos[1] = 1.5f;
        Vector3 originPosMirror = new Vector3(originPos.x, -originPos.y, originPos.z);
        Ray newRay = new Ray(originPos, Vector3.down);      //Seems like Vector3.down works well

        RaycastHit[] hit = Physics.RaycastAll(newRay, (originPosMirror - originPos).magnitude);
        for (int i = 0; i < hit.Length; i++)
        {
            Debug.DrawRay(originPos, Vector3.down, Color.red);
            if (hit[i].transform.name == "Cube") //wall or indestructible wall
            {
                return true;
            }
        }

        return false;
    }

    public void mUpdateMyMap(Vector3 iDeletedPosCube)
    {
        Vector3 mindex_cube = mPosToMapIndex(iDeletedPosCube, myLocationOfFirstCube, myGridSize);
        ArrayList mCol_x = (ArrayList)myMap[(int)mindex_cube[0]];
        mCol_x[(int) mindex_cube[2]] = (int) CreateMap.GridType.Free;
        flag_update_path = true;
    }

    ArrayList myMakeCopyOf(ArrayList iMap)
    {
        ArrayList cMap = new ArrayList();
        for (int i = 0; i < iMap.Count; i++)
        {
            ArrayList nCol = new ArrayList();
            ArrayList iCol = (ArrayList)iMap[i];
            for (int j = 0; j < iCol.Count; j++)
            {
                nCol.Add((int) iCol[j]);
            }
            cMap.Add(nCol);
        }
        return cMap;
    }

    public void setTargetPos(Vector3 iPos, bool iFromPlayer)
    {
        Vector3 nPos = mPosToMapIndex(iPos, myLocationOfFirstCube, myGridSize);
        nPos[0] = Mathf.Round(nPos[0]);
        nPos[2] = Mathf.Round(nPos[2]);

        if (iFromPlayer && EnemyType == myEnemyType.NotFollowingPlayer)
            return;

        if ((nPos - TragetPos).magnitude > 0)
        {
            TragetPos = nPos;
            flag_update_path = true;
        }

    }

    void mMoveOnThePath(ArrayList iUpdatedPath, Vector3 iLocationOfFirstCube, Vector3 iGridSize)//Move Enemy on the path
    {
        if (iUpdatedPath == null)
        {
            DoesRaycastHitObject(transform.position);
            return;
        }
        if (iUpdatedPath.Count == 0)
        {
            DoesRaycastHitObject(transform.position);
            return;
        }
        Vector3 m_MapIndex = (Vector3)iUpdatedPath[iUpdatedPath.Count-1];
        Vector3 m_next_pos = mMapIndexToPos(m_MapIndex, iLocationOfFirstCube, iGridSize);
        float m_dis = (m_next_pos - this.transform.position).magnitude; //detect enemy direction here
        if (m_dis <= Mathf.Abs(EnemySpeed))
        {
            //this.transform.position = m_next_pos;
            iUpdatedPath.RemoveAt(iUpdatedPath.Count - 1);
        }
        else
        {
            //this.transform.position += EnemySpeed * ((m_next_pos - this.transform.position) / m_dis);
            m_next_pos = this.transform.position + EnemySpeed * ((m_next_pos - this.transform.position) / m_dis);

        }

        if (!DoesRaycastHitObject(m_next_pos))
        {
            this.transform.position = m_next_pos;

        }
    }

    Vector3 mMapIndexToPos(Vector3 iMapIndex, Vector3 iLocationOfFirstCube, Vector3 iGridSize)
    {
        Vector3 nPos = new Vector3(iMapIndex[0] * iGridSize[0], 0f, iMapIndex[2] * iGridSize[2]);
        return nPos + new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]) + new Vector3(0f, transform.position[1], 0f);
    }

    Vector3 mPosToMapIndex(Vector3 iPos, Vector3 iLocationOfFirstCube, Vector3 iGridSize)
    {
        iPos = iPos - new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]);
        return new Vector3(iPos[0] / iGridSize[0], 0f, iPos[2] / iGridSize[2]);
    }

    ArrayList getUpdatedMap()
    {
        return myMap;
    }

    int getMapVal(ArrayList iMap, int i, int j)
    {
        if (i >= iMap.Count || j >= ((ArrayList) iMap[0]).Count)
            return -1;
        ArrayList col_i = (ArrayList)iMap[i]; //each index_i in the map returns a column on the X direction
        return (int) col_i[j];
    }

    bool isFreePointOnMap(ArrayList iUpdatedMap, Vector3 i3Dindex)
    {
        if (getMapVal(iUpdatedMap, (int) i3Dindex[0], (int) i3Dindex[2]) == (int) CreateMap.GridType.Free)
            return true;
        return false;
    }

    ArrayList mFindPath(Vector3 start, Vector3 end, ArrayList iUpdatedPath)//start and end are on the map
    {
        ArrayList m_path = new ArrayList();

        if (iUpdatedPath != null)
        {
            if (iUpdatedPath.Count != 0)
            {
                start = (Vector3) iUpdatedPath[iUpdatedPath.Count - 1];
            }
        }

        AStarNode p_node = new AStarNode(start, end, null);
        ArrayList updated_map = getUpdatedMap();
        ArrayList open_list = new ArrayList();

        open_list.Add(p_node);
        ArrayList closed_list = new ArrayList();

        ArrayList end_points = new ArrayList();
        while (open_list.Count > 0)
        {
            //if ((start - end).magnitude < 2 && open_list.Count > 20)
            //{
            //    int notifyme = 1;//for debug
            //}
            int f_index = p_node.mFindNodeByMinVal(open_list);
            AStarNode best_node = (AStarNode)open_list[f_index];
            open_list.RemoveAt(f_index);
            Vector3 index_best_node = best_node.m_point;

            if ((best_node.m_point - end).magnitude > EnemySpeed)
            {
                bool flag_added_next_node = false;
                for (int i = 0; i < 4; i++)
                {
                    Vector3 mDir = getDirection(i);
                    if (isFreePointOnMap(updated_map, index_best_node + mDir) && p_node.mFindNodeByPoint(closed_list, index_best_node + mDir) == -1)
                    {
                        open_list.Add(new AStarNode(index_best_node + mDir, end, best_node));
                        flag_added_next_node = true;
                    }
                }
                if (!flag_added_next_node)
                {
                    end_points.Add(best_node);
                }
            }
            else
            {
                end_points.Clear();
                end_points.Add(best_node);
                open_list.Clear();
            }
            closed_list.Add(best_node);
        }
        int index_closest_node = p_node.mFindNodeByMinVal(end_points);

        if (index_closest_node != -1)
        {
            AStarNode c_node = (AStarNode)end_points[index_closest_node];
            while (c_node != null)
            {
                m_path.Add(c_node.m_point);
                c_node = c_node.m_parent;
            }
        }
        return m_path;// path from end to start -> must traverse reversely
    }

    Vector3 myFindRandomPlace(ArrayList iMap, Vector3 iLocationOfFirstCube, Vector3 iGridSize, bool onX)
    {
        bool flag_continue = true;
        int rand_pos = 1;
        int max_counter = 0;
        if (onX)
        {
            rand_pos = UnityEngine.Random.Range((int) 1, (int) myMapInfo.M - 1);
            max_counter = (int) myMapInfo.N;
        }
        else
        {
            rand_pos = UnityEngine.Random.Range((int) 1, (int) myMapInfo.N - 1);
            max_counter = (int) myMapInfo.M;
        }
        int index_i = 0;
        int index_j = 0;
        int ret_i = 1;
        int ret_j = 1;
        int rand_counter = UnityEngine.Random.Range((int)0, (int)max_counter-1);
        for (int i = 0; i < max_counter && flag_continue; i++)
        {
            if (onX)
            {
                index_i = rand_pos;
                index_j = i;
            }
            else
            {
                index_i = i;
                index_j = rand_pos;
            }
            if (getMapVal(myMap, index_i, index_j) == (int) CreateMap.GridType.Free)
            {
                if (i >= rand_counter)
                {
                    flag_continue = false;
                }
                ret_i = index_i;
                ret_j = index_j;
            }
        }
        return new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]) + new Vector3(ret_i * iGridSize[0], 0f, ret_j * iGridSize[2]);
    }

    Vector3 myFindFirstRandomPlace(ArrayList iMap, Vector3 iLocationOfFirstCube, Vector3 iGridSize, int iRandcol)
    {
        bool flag_continue = true;
        int rand_pos = 1;
        int max_counter = 0;
        if (iRandcol == 0)
        {
            rand_pos = UnityEngine.Random.Range((int)1, (int)myMapInfo.N - 1);
            max_counter = (int)myMapInfo.N;
        }
        else
        {
            rand_pos = UnityEngine.Random.Range((int)1, (int)myMapInfo.M - 1);
            max_counter = (int)myMapInfo.M;
        }
        int index_i = 0;
        int index_j = 0;
        int ret_i = -1;
        int ret_j = -1;
        int rand_counter = UnityEngine.Random.Range((int)0, (int)max_counter - 1);
        float m_dis = -max_counter;
        for (int i = 0; i < max_counter && flag_continue; i++)
        {
            if (iRandcol == 0)
            {
                index_i = rand_pos;
                index_j = i;
            }
            else
            {
                index_i = i;
                index_j = rand_pos;
            }
            if (getMapVal(myMap, index_i, index_j) == (int)CreateMap.GridType.Free)
            {
                
                //Vector3 index_player_pos = mPosToMapIndex(iPlayerPos, iLocationOfFirstCube, iGridSize);
                //index_player_pos.x = Mathf.Round(index_player_pos.x);
                //index_player_pos.z = Mathf.Round(index_player_pos.z);

                //Vector3 found_index = new Vector3(index_i, 0, index_j);
                //if ((index_player_pos - found_index).magnitude >= m_dis)
                //{
                //    m_dis = (index_player_pos - found_index).magnitude;
                    ret_i = index_i;
                    ret_j = index_j;
                    
                //}
                    flag_continue = false;
            }
        }

        return new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]) + new Vector3(ret_i * iGridSize[0], 0f, ret_j * iGridSize[2]);
    }

    //Find the ith last available free tile
    Vector3 LocateFirstAvailableSpace(ArrayList iMap, Vector3 iLocationOfFirstCube, Vector3 iGridSize, int iCount)
    {
        bool flag_continue = true;
        int index_i = 0;
        int index_j = 0;
        int m_count = 0;
        for (int i = iMap.Count - 1; i >= 0 && flag_continue; i--)
        {
            ArrayList iMap_col = (ArrayList)iMap[i];
            for (int j = iMap_col.Count - 1; j >= 0 && flag_continue; j--)
            {
                int val_ij = (int)iMap_col[j];
                if (val_ij == (int) CreateMap.GridType.Free)
                {
                    if (m_count >= iCount)
                    {
                        flag_continue = false;
                    }
                    m_count++;
                    index_i = i;
                    index_j = j;
                }
            }
        }
        return new Vector3(iLocationOfFirstCube[0], 0f, iLocationOfFirstCube[2]) + new Vector3(index_i * iGridSize[0], 0f, index_j * iGridSize[2]);
    }

    Vector3 getDirection(int iDir_i)
    {
        switch (iDir_i)
        {
            case 0:
                return Vector3.forward;
            case 1:
                return Vector3.back;
            case 2:
                return Vector3.right;
            case 3:
                return Vector3.left;
            case 4:
                return Vector3.down;
        }
        return Vector3.up;
    }

    void mRestartPos()
    {
        int rand_col = UnityEngine.Random.Range((int)0, (int)2);
        transform.position = myFindFirstRandomPlace(myMap, myLocationOfFirstCube, myGridSize, rand_col) + new Vector3(0f, transform.position[1], 0f);
        myUpdatedPath.Clear();
        flag_update_path = true;
    }

    void setMapVal(ArrayList iMap, int i, int j, int iVal)
    {
        if (i >= iMap.Count || j >= ((ArrayList) iMap[0]).Count)
            return;
        ArrayList col_i = (ArrayList)iMap[i]; //each index_i in the map returns a column on the X direction
        col_i[j] = iVal;
    }
}
