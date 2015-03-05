﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Area
{

    #region Plans

    //TODO: Multithreaded Area generation, when the player is in an adjacent Area. To reduce load times.

    //TODO: Create a function that randomly assigns a quest to one of its Rooms.

    #endregion


    #region Variables

    //AreaGroup which this Area is a part of.
    private AreaGroup group;

    //Areas which this Area is connected to. True, if connected to an area in that direction.
    public bool north;
    public bool east;
    public bool south;
    public bool west;


    //This area's position on the map.
    public Point position
    {
        get;
        private set;
    }

	//This area's default spawn location if the player did not enter the area via a portal
	public Point defaultSpawn
	{
		get
		{
			return rooms[0].center * 10;
		}
	}
    
    //The Map which this Area is a part of.
    private Map parentMap;

    private List<Room> rooms; //The rooms which this Area contains.
    private List<TileData> corridors; //This might be useless?
    private TileData[,] tiles;

    private List<Tile> objects;
    public List<Tile> portals;

    private int areaSeed;

    private bool isGenerated = false; //If the data has been made.
    private bool isCreated = false; //If the GameObjects have been made.
    private bool isHidden = false; //If the GameObjects are hidden.

    private bool questArea = false; //If this Area contains quest material. (if so, it CANNOT EVER BE NULLED! MUAHAHAHA!)

    #endregion


    #region Constructors

    public Area(Map parentMap, Point position, int seed, bool N, bool E, bool S, bool W)
    {
        this.parentMap = parentMap;
        this.position = position;
        this.areaSeed = seed;

        north = N;
        east = E;
        south = S;
        west = W;

        rooms = new List<Room>();

    }

    #endregion


    #region Generation methods

    //Generates the 2D array and Rooms List needed to show this area.
    public void generateArea()
    {
        if (!isGenerated)
        {
            //Default Area Generation.
            AreaGen.defaultGen(areaSeed, out tiles, out rooms, out corridors);

            setupPortals();

            isGenerated = true;

            //Will have a switch statement determining which mapGenerator to use based on the tileSet, if we
            //want to have Area's generated differently.
        }
    }

    //Creates GameObjects of the 2D array, and displays them. Or if they're already created, just display them.
    public void showArea()
    {
        //Don't make GameObjects, if this area is already created.
        if (!isCreated)
        {
            //Ensure that this area has been generated. If not, do the generation now.
            if (!isGenerated)
            {
                generateArea();
            }

            //Determine the TileSet to be used.
            TileSet mySet;
            switch (group.biome)
            {
                    //In the case of any of these Biomes, use the grassyPath Tile set, until we get new tile sets.
                case(Biome.C):
                case(Biome.HTML):
                case(Biome.PYTHON):
                    mySet = LoadResources.Instance.grassyPath.GetComponent<TileSet>();
                    break;
                default:
                    throw new System.MissingFieldException("Area does not have an Area Group!");
            }

            objects = new List<Tile>();
            portals = new List<Tile>();

            GameObject parent = new GameObject();
            parent.name = "Area Parent";

            //Create the GameObjects by iterating through the information.
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    //Do GameObject creation here.
                    if (tiles[i, j] != null)
                    {

                        if (tiles[i, j].isTile)
                        {
                            //Create Tile Object
							Tile temp = (Tile) GameObject.Instantiate(mySet.tiles[0],
                                new Vector3(i * 10, mySet.tiles[0].y, j * 10), Quaternion.identity);

                            temp.transform.parent = parent.transform;
                            temp.x = i;
                            temp.y = j;

                            objects.Add(temp);
                        }
                        else if (tiles[i,j].isBorder)
                        {
                            //Create Wall Object
                            Tile temp = (Tile) GameObject.Instantiate(mySet.tiles[1],
                                new Vector3(i * 10, mySet.tiles[1].y, j * 10), Quaternion.identity);

                            temp.transform.parent = parent.transform;
                            temp.x = i;
                            temp.y = j;

                            objects.Add(temp);
                        }
                        else if (tiles[i,j].isPortal)
                        {
                            //Create Portal Object.
                            Tile temp = (Tile) GameObject.Instantiate(LoadResources.Instance.portal,
                                new Vector3(i * 10, LoadResources.Instance.portal.y, j * 10), Quaternion.identity);

                            switch (tiles[i, j].portalDirection)
                            {
                                case (Direction.UP):
                                    break;
                                case (Direction.LEFT):
                                    temp.transform.eulerAngles = new Vector3(0, 270, 0);
                                    break;
                                case (Direction.DOWN):
                                    temp.transform.eulerAngles = new Vector3(0, 180, 0);
                                    break;
                                case (Direction.RIGHT):
                                    temp.transform.eulerAngles = new Vector3(0, 90, 0);
                                    break;
                            }

                            temp.gameObject.GetComponent<Portal>().dir = tiles[i, j].portalDirection;
                            temp.x = i;
                            temp.y = j;

                            objects.Add(temp);
                            portals.Add(temp);
                        }
                    }
                }
            }

            isHidden = false;
        }

        if (isHidden)
        {
            System.Random random = new System.Random(areaSeed);

            foreach(Room r in rooms)
            {
                r.showRoom(random);
            }

            isHidden = false;
        }

    }

    //Disables the GameObjects for this Area.
    public void hideArea()
    {
        if (isCreated && !isHidden)
        {
            foreach(Room r in rooms)
            {
                r.hideRoom();
            }

            foreach(Tile t in objects)
            {
                t.gameObject.active = false;
            }

            isHidden = true;
        }
    }

    //Nulls the data of this Area for garbage collection.
    public void releaseData()
    {
        if (isGenerated)
        {
            foreach(Tile t in objects)
            {
                GameObject.Destroy(t.gameObject);
            }
            objects = null;

            foreach (Room r in rooms)
            {
                r.destroyRoom();
            }

            if (!questArea)
            {
                rooms = null;

                tiles = null;
                corridors = null;

                isGenerated = false;
                isCreated = false;
            }
            else
            {
                isGenerated = true;
            }
        }
    }

    #endregion


    #region Public methods

    //Returns the neighbors connected to this Area. (Should always have at least one)
    public Area[] getNeighbors()
    {
        Area[] temp = new Area[connections()];

        int i = 0;

        if (north && parentMap.withinMapBounds(position.up))
        {
            temp[i] = parentMap.getArea(position.up);
            i++;
        }
        if (east && parentMap.withinMapBounds(position.right))
        {
            temp[i] = parentMap.getArea(position.right);
            i++;
        }
        if (south && parentMap.withinMapBounds(position.down))
        {
            temp[i] = parentMap.getArea(position.down);
            i++;
        }
        if (west && parentMap.withinMapBounds(position.left))
        {
            temp[i] = parentMap.getArea(position.left);
            i++;
        }

        return temp;
    }

    public void setGroup(AreaGroup g)
    {
        group = g;
    }
    public AreaGroup getGroup()
    {
        return group;
    }
    public Biome getBiome()
    {
        if (group == null)
        {
            return Biome.NOT_ASSIGNED;
        }
        return group.biome;
    }

    public void executeSpawnCommand(SpawnCommand sc)
    {
        questArea = true;

        System.Random rand = new System.Random();
        rooms[rand.Next(rooms.Count)].generateQuestMaterial(sc);
    }

    #endregion


    #region Helper methods

    //Returns the number of connections this Area has to other Areas.
    private int connections()
    {
        return (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);
    }

    private void setupPortals()
    {
        if (north)
        {
            //Make North Portal.

            //Find northmost Room.
            Room temp = rooms[0];
            for (int i = 1; i < rooms.Count; i++)
            {
                if (rooms[i].getTopRight().y > temp.getTopRight().y)
                {
                    temp = rooms[i];
                }
            }

            //Create a portal in it.
            temp.createPortal(Direction.UP, ref tiles);
        }
        if (east)
        {
            //Make East Portal.

            //Find eastmost Room.
            Room temp = rooms[0];
            for (int i = 1; i < rooms.Count; i++)
            {
                if (rooms[i].getTopRight().x > temp.getTopRight().x)
                {
                    temp = rooms[i];
                }
            }

            //Create a portal in it.
            temp.createPortal(Direction.RIGHT, ref tiles);
        }
        if (south)
        {
            //Make South Portal.

            //Find southmost Room.
            Room temp = rooms[0];
            for (int i = 1; i < rooms.Count; i++)
            {
                if (rooms[i].getBotLeft().y < temp.getBotLeft().y)
                {
                    temp = rooms[i];
                }
            }

            //Create a portal in it.
            temp.createPortal(Direction.DOWN, ref tiles);
        }
        if (west)
        {
            //Make West Portal.

            //Find eastmost Room.
            Room temp = rooms[0];
            for (int i = 1; i < rooms.Count; i++)
            {
                if (rooms[i].getBotLeft().x < temp.getBotLeft().x)
                {
                    temp = rooms[i];
                }
            }

            //Create a portal in it.
            temp.createPortal(Direction.LEFT, ref tiles);
        }
    }

    #endregion


    #region Debug Methods

    public void debugShowArea()
    {
        if (!isGenerated)
        {
            generateArea();
        }

        Debug.Log("Map seed is : " + parentMap.getSeed());
        Debug.Log("Area Seed is: " + areaSeed);

        GameObject parent = new GameObject();
        parent.name = "Area Debug Parent";

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                if (tiles[i,j] != null)
                {
                    GameObject current = (GameObject) GameObject.Instantiate(LoadResources.Instance.spriteHolder, new Vector3(i, j, 0), Quaternion.identity);

                    SpriteRenderer renderer = current.GetComponent<SpriteRenderer>();
                    renderer.sprite = LoadResources.Instance.fourWay;

                    current.transform.parent = parent.transform;

                    if(tiles[i,j].isBorder)
                    {
                        renderer.color = Color.black;
                    }
                    else if (tiles[i,j].isTile)
                    {
                        renderer.color = Color.white;
                    }
                    else
                    {
                        renderer.color = Color.red;
                    }
                }
            }
        }
    }

    #endregion


}
